using RawSync.Models;
using RawSync.CustomMessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;
using System.Windows.Data;
using System.Threading;

namespace RawSync.ViewModels
{
    class DeltaViewModel : INotifyPropertyChanged
    {

        public ObservableCollection<FileDescriptorModel> MyDeltaFiles { get; set; }
        public ICollectionView MyDeltaFilesView { get; set; }
        public List<FileDescriptorModel> SessionDeletedFiles;

        public int MyDeltaFilesNo { get { return MyDeltaFiles.Count; } }
        public double MyRejectionRatio { get { if (QualifiedRawNo != 0) return (double)MyDeltaFiles.Count / (double)QualifiedRawNo; else return (double)0; } }
        private int MyDeletedFilesNo { get; set; } = 0;
        public int MyDeletionProgress { get; set; } = 0;

        private int QualifiedRawNo;

        private bool _deletionBusy = false;

        public RelayCommand RemoveDeltaListItemCommand { get; private set; }
        public RelayCommand DeleteDeltaFilesCommand { get; private set; }
        public RelayCommand CancelDeletionCommand { get; private set; }
        public RelayCommand CloseDeltaCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void EventHandler();
        public event EventHandler FilesDeletedEvent;

        CancellationTokenSource _cts = null;
        public DeltaViewModel(FolderListingModel rawListingModel)
        {

            // Instantiate the main collection in this view model
            MyDeltaFiles = new ObservableCollection<FileDescriptorModel>();

            // Build the Delta Files collection (candidate files for deletion), based on BelongsToDelta information:
            QualifiedRawNo = 0;
            foreach (FileDescriptorModel fd1 in rawListingModel.FileList)
            {
                if (fd1.IsQualified)
                {
                    QualifiedRawNo++;
                }
                if (fd1.BelongsToDelta)
                {
                    MyDeltaFiles.Add(fd1);
                }
            }

            // Build Views of the above collections, so that we can manipulate it better
            MyDeltaFilesView = new ListCollectionView(MyDeltaFiles);
            MyDeltaFilesView.MoveCurrentTo(null);

            SessionDeletedFiles = new List<FileDescriptorModel>();

            // Initiate commands
            RemoveDeltaListItemCommand = new RelayCommand(RemoveDeleteListItem, RemoveDeleteListItemCanUse);
            DeleteDeltaFilesCommand = new RelayCommand(DeleteDeltaFiles, DeleteDeltaFilesCanUse);
            CancelDeletionCommand = new RelayCommand(CancelDeletion, CancelDeletionCanUse);
            CloseDeltaCommand = new RelayCommand(CloseDelta, CloseDeltaCanUse);
        }

        private void OnPropertyRaised(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        private void RemoveDeleteListItem(object arg)
        {
            // Last chance to avoid deleting a file from MyDeltaFiles - remove it from the list.
            if (arg != null && (int)arg > -1)
            {
                FileDescriptorModel fdv = (FileDescriptorModel)MyDeltaFilesView.CurrentItem;
                if (fdv != null)
                {
                    // Remove only from Delta list, leave the rest of collections/lists alone...
                    MyDeltaFiles.Remove(fdv);
                    MyDeltaFilesView.MoveCurrentTo(null);

                    OnPropertyRaised("MyDeltaFilesNo");
                    OnPropertyRaised("MyRejectionRatio");
                }
            }
        }

        private bool RemoveDeleteListItemCanUse(object arg)
        {
            if (arg == null)
            {
                return false;
            }
            if ((int)arg == -1)
            {
                return false;
            }
            return true;
        }

        private async void DeleteDeltaFiles(object arg)
        {
            _deletionBusy = true;

            Progress<DeletionProgressReportModel> progress = new Progress<DeletionProgressReportModel>();
            progress.ProgressChanged += ProgressReportedEventHandler;

            MyDeletedFilesNo = 0;
            MyDeletionProgress = 0;
            string msg = "completed";

            _cts = new CancellationTokenSource();
            try
            {
                if (RSSettingsModel.Instance.DeletePromptSetting)
                {
                    await DeleteDeltaFilesAsync(progress, _cts.Token);
                }
                else
                {
                    await QuickDeleteDeltaFilesAsync(progress, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                msg = "cancelled";
            }
            _cts.Dispose();

            if (RSSettingsModel.Instance.DeletePromptSetting == false)
            {
                // We are using quick parallel delete method (which may have been cancelled) - remove actually deleted items 'manually', when the above loop finishes
                foreach(var fd in SessionDeletedFiles)
                {
                    MyDeltaFiles.Remove(fd);
                    QualifiedRawNo--;
                }
                MyDeltaFilesView.Refresh();

                SessionDeletedFiles.Clear();

                OnPropertyRaised("MyDeltaFilesNo");
                OnPropertyRaised("MyRejectionRatio");
            }
            // We have done what we could, now
            // Fire the FilesDeleted Event so that the Main Window can update its raw list view, and
            // if the confirmation option is set, inform the user about the outcome of deletion.
            FilesDeletedEvent();
            if (RSSettingsModel.Instance.DeletePromptSetting)
            {
                WWMessageBox.Show($"Deletion { msg }.\nDeleted { MyDeletedFilesNo } file(s).", "Information", CustomMessageBoxButton.OK, MessageBoxImage.Information);
            }
            
            _deletionBusy = false;
            return;
        }

        private void ProgressReportedEventHandler(object sender, DeletionProgressReportModel e)
        {

            MyDeletedFilesNo = e.DeletedFilesNo;
            MyDeletionProgress = e.DeletionProgress;
            OnPropertyRaised("MyDeletionProgress");

            if (e.CurrentFd != null)
            {
                // Do this only when deletion is a single-threaded sequential operation (non-parallel)
                if (MyDeltaFiles.Contains(e.CurrentFd))
                {
                    MyDeltaFiles.Remove(e.CurrentFd);
                    MyDeltaFilesView.Refresh();

                    QualifiedRawNo--;

                    OnPropertyRaised("MyDeltaFilesNo");
                    OnPropertyRaised("MyRejectionRatio");
                }
            }
        }

        private async Task DeleteDeltaFilesAsync(IProgress<DeletionProgressReportModel> progress, CancellationToken cancellationToken)
        {

            DeletionProgressReportModel progressReport = new DeletionProgressReportModel(MyDeltaFiles.Count);
            bool ContinueAsking = true;

            List<FileDescriptorModel> IterationListCopy = new List<FileDescriptorModel>();

            for (int i = MyDeltaFiles.Count - 1; i >= 0; i--)
            {
                var fd = MyDeltaFiles[i];
                if (ContinueAsking)
                {
                    // ask user to confirm deletion in modal message box, by checking the returned result (Cancel, Skip, Delete, Delete All)
                    MessageBoxResult result = WWMessageBox.ShowOKYesNoCancel($"Confirm deletion of the file:\n{ fd.FullName }", "User decision required", "Delete All", "Delete this one", "Skip", "Cancel", MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.OK:
                            ContinueAsking = false;
                            break;
                        case MessageBoxResult.Yes:
                            break;
                        case MessageBoxResult.No:
                            continue;
                        case MessageBoxResult.Cancel:
                            return;
                        default:
                            break;
                    }
                }

                await Task.Run(() => DeleteSingleDeltaFile(fd));

                progressReport.DeletedFilesNo++;
                progressReport.CurrentFd = fd;

                progress.Report(progressReport);
                cancellationToken.ThrowIfCancellationRequested();

            }

            return;
        }

        private async Task QuickDeleteDeltaFilesAsync(IProgress<DeletionProgressReportModel> progress, CancellationToken cancellationToken)
        {
            object sync = new object();

            DeletionProgressReportModel progressReport = new DeletionProgressReportModel(MyDeltaFiles.Count);
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cancellationToken;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;

            await Task.Run(() =>
            {
                Parallel.ForEach<FileDescriptorModel>(MyDeltaFiles, po, (fd) =>
                {

                    DeleteSingleDeltaFile(fd);
                    lock(sync)
                    {
                        progressReport.DeletedFilesNo++;
                        // We cannot remove MyDeltaFiles list elements - because List type isn't thread safe
                        // But somehow we can add to the list in the progress report (that's what Tim Corey does, and it works for him...)
                        // We have to do this in order to remove deleted files from the MyDeltaFiles list after the parallel loop finishes.
                        SessionDeletedFiles.Add(fd);

                        progress.Report(progressReport);
                    }
                    po.CancellationToken.ThrowIfCancellationRequested();

                });
            });
            return;
        }

        private void DeleteSingleDeltaFile(FileDescriptorModel fd)
        {
            // Try to delete file, checking for potential IO exceptions:
            //   - if successful, delete this entry from both lists, increment DeletedFilesNo and continue the loop
            //   - if not successful continue the loop leaving the list as is (perhaps for repeated deleting)
            try
            {
                if (File.Exists(fd.FilePath))
                {
                    if (RSSettingsModel.Instance.RecycleBinSetting)
                    {
                        // Move the file to Recycle Bin (watch exceptions if they are the same as from File.Delete()) 
                        FileSystem.DeleteFile(fd.FilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        File.Delete(fd.FilePath);
                    }
                }
                else
                {
                    MessageBox.Show($"File { fd.FullName } do not exist.", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (IOException)
            {
                // The file could not be deleted, possibly due to the lock or some other IO problem    
                MessageBox.Show($"Could not delete file: { fd.FullName } .\nFile may be locked. Hit OK to continue.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Obsługa exception rzucanego przez dialog pochodny (Error) dla dialogu FileSystem.DeleteFile - jeśli naciśnięto Cancel. Działa jak Skip...
                // Microsoft nie dokumentuje jakiego typu to jest Exception, stąd łapane i wyświetlane są wszystkie. Do użytkownika należy decyzja, czy program zterminuje, czy pozwoli dalej działać
                MessageBoxResult result = MessageBox.Show($"Could not delete file: { fd.FullName } .\nException data: { ex.Message }.\nHit OK to Continue, Cancel to terminate App.", "Error!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                if (result == MessageBoxResult.Cancel)
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
        }

        private bool DeleteDeltaFilesCanUse(object arg)
        {
            if(MyDeltaFilesNo > 0 && !_deletionBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CancelDeletion(object arg)
        {
            _cts.Cancel();
        }

        private bool CancelDeletionCanUse(object arg)
        {
            if (_deletionBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CloseDelta(object arg)
        {
            // Just close associated view window, xaml passes the view window reference as arg
            if (arg != null)
            {
                ((Window)arg).Close();
            }
            else
            {
                MessageBox.Show("CloseDelta():\nView reference is null.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CloseDeltaCanUse(object arg)
        {

            if (!_deletionBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
