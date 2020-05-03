using RawSync.Models;
using RawSync.CustomMessageBox;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using WinForms = System.Windows.Forms;
using System.Text.RegularExpressions;

namespace RawSync.ViewModels
{
    class ShellViewModel : INotifyPropertyChanged
    {

        private FolderListingModel MyProcessedFolderListing; // Reference to model, passed in this ctor (one way to communicate to model...)
        private FolderListingModel MyRawFolderListing; // Reference to model, passed in this ctor (one way to communicate to model...)

        public RelayCommand SelectProcessedFolderCommand { get; private set; }
        public RelayCommand SelectRawFolderCommand { get; private set; }
        public RelayCommand RefreshProcessedFileListCommand { get; private set; }
        public RelayCommand RefreshRawFileListCommand { get; private set; }
        public RelayCommand RemoveProcessedItemCommand { get; private set; }
        public RelayCommand RemoveRawItemCommand { get; private set; }
        public RelayCommand GetDeltaCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }
        public RelayCommand ProgramExitCommand { get; private set; }
        public RelayCommand ProgramHelpCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ProcessedFolder { get; set; }
        public string RawFolder { get; set; }

        public ObservableCollection<FileDescriptorModel> MyProcessedFolderFiles { get; set; }
        public ObservableCollection<FileDescriptorModel> MyRawFolderFiles { get; set; }
        public ICollectionView MyProcessedGridView { get; set; }
        public ICollectionView MyRawGridView { get; set; }

        public int MyProcessedFolderFilesNo
        {
            get
            {
                return MyProcessedFolderFiles.Count;
            }
        }

        public int MyProcessedFilesNo
        {
            get
            {
                int j = 0;
                for (int i = 0; i < MyProcessedFolderFiles.Count; i++)
                {
                    if (MyProcessedFolderFiles[i].IsQualified)
                    {
                        j++;
                    }
                }
                return j;
            }
        }

        public int MyProcessedFilesShownNo
        {
            get
            {
                // Grid View needs to be cast, in order to get Count()
                return MyProcessedGridView.Cast<object>().Count();
            }
        }

        public int MyRawFolderFilesNo
        {
            get
            {
                return MyRawFolderFiles.Count;
            }
        }

        public int MyRawFilesNo
        {
            get
            {
                int j = 0;
                for (int i = 0; i < MyRawFolderFiles.Count; i++)
                {
                    if (MyRawFolderFiles[i].IsQualified)
                    {
                        j++;
                    }
                }
                return j;
            }
        }

        public int MyRawFilesShownNo
        {
            get
            {
                // Grid View needs to be cast, in order to get Count()
                return MyRawGridView.Cast<object>().Count();
            }
        }

        public int MyDeltaFilesNo
        {
            get
            {
                int j = 0;
                for (int i = 0; i < MyRawFolderFiles.Count; i++)
                {
                    if (MyRawFolderFiles[i].BelongsToDelta)
                    {
                        j++;
                    }
                }
                return j;
            }
        }

        private bool _CheckProcessedQualified = true;
        private bool _CheckProcessedNonQualified = true;
        private bool _CheckRawQualified = true;
        private bool _CheckRawNonQualified = true;
        private bool _CheckHighlightDelta = false;

        public bool CheckProcessedQualified
        {
            get { return _CheckProcessedQualified; }
            set
            {
                _CheckProcessedQualified = value;
                MyProcessedGridView.Refresh();
                OnPropertyRaised("MyProcessedFilesShownNo");
            }
        }

        public bool CheckProcessedNonQualified
        {
            get { return _CheckProcessedNonQualified; }
            set
            {
                _CheckProcessedNonQualified = value;
                MyProcessedGridView.Refresh();
                OnPropertyRaised("MyProcessedFilesShownNo");
            }
        }

        public bool CheckRawQualified
        {
            get { return _CheckRawQualified; }
            set
            {
                _CheckRawQualified = value;
                MyRawGridView.Refresh();
                OnPropertyRaised("MyRawFilesShownNo");
            }
        }

        public bool CheckRawNonQualified
        {
            get { return _CheckRawNonQualified; }
            set
            {
                _CheckRawNonQualified = value;
                MyRawGridView.Refresh();
                OnPropertyRaised("MyRawFilesShownNo");
            }
        }

        public bool CheckHighlightDelta
        {
            get { return _CheckHighlightDelta; }
            set
            {
                _CheckHighlightDelta = value;
                HighlightDelta(value);
            }
        }

        public ShellViewModel(FolderListingModel pfl, FolderListingModel rfl)
        {

            MyProcessedFolderListing = pfl;
            MyRawFolderListing = rfl;
            ProcessedFolder = MyProcessedFolderListing.FolderPath;
            RawFolder = MyRawFolderListing.FolderPath;

            // Build observable collections of file descriptors in view model
            MyProcessedFolderFiles = new ObservableCollection<FileDescriptorModel>();
            foreach (FileDescriptorModel fd in MyProcessedFolderListing.FileList)
            {
                MyProcessedFolderFiles.Add(fd);
            }

            MyRawFolderFiles = new ObservableCollection<FileDescriptorModel>();
            foreach (FileDescriptorModel fd in MyRawFolderListing.FileList)
            {
                MyRawFolderFiles.Add(fd);
            }

            // Build Views of the above collections, which can then be filtered
            ListCollectionView MyProcessedFolderFilesView = new ListCollectionView(MyProcessedFolderFiles);
            MyProcessedFolderFilesView.Filter = new Predicate<object>(ProcessedQualifyFilter);
            MyProcessedGridView = MyProcessedFolderFilesView;
            MyProcessedGridView.MoveCurrentTo(null);

            ListCollectionView MyRawFolderFilesView = new ListCollectionView(MyRawFolderFiles);
            MyRawFolderFilesView.Filter = new Predicate<object>(RawQualifyFilter);
            MyRawGridView = MyRawFolderFilesView;
            MyRawGridView.MoveCurrentTo(null);

            // Initiate button commands
            SelectProcessedFolderCommand = new RelayCommand(SelectProcessedFolder);
            SelectRawFolderCommand = new RelayCommand(SelectRawFolder);
            RefreshProcessedFileListCommand = new RelayCommand(RefreshProcessedFileList);
            RefreshRawFileListCommand = new RelayCommand(RefreshRawFileList);
            RemoveProcessedItemCommand = new RelayCommand(RemoveProcessedItem, RemoveProcessedItemCanUse);
            RemoveRawItemCommand = new RelayCommand(RemoveRawItem, RemoveRawItemCanUse);
            GetDeltaCommand = new RelayCommand(GetDelta, GetDeltaCanUse);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ProgramExitCommand = new RelayCommand(ProgramExit);
            ProgramHelpCommand = new RelayCommand(ProgramHelp);

        }

        private void OnPropertyRaised(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        private bool ProcessedQualifyFilter(object e)
        {
            var obj = e as FileDescriptorModel;
            if (obj != null)
            {
                if ((CheckProcessedQualified && obj.IsQualified) || CheckProcessedNonQualified && !obj.IsQualified)
                {
                    return true;
                }
            }
            return false;
        }

        private bool RawQualifyFilter(object e)
        {
            var obj = e as FileDescriptorModel;
            if (obj != null)
            {
                if ((CheckRawQualified && obj.IsQualified) || CheckRawNonQualified && !obj.IsQualified)
                {
                    return true;
                }
            }
            return false;
        }

        private void SelectProcessedFolder(object arg)
        {
            WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;
            folderDialog.SelectedPath = ProcessedFolder;
            folderDialog.Description = "Pick Processed files Folder";
            WinForms.DialogResult result = folderDialog.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                // Send path to Model
                MyProcessedFolderListing.FolderPath = folderDialog.SelectedPath;
                // Set path in View Model
                ProcessedFolder = folderDialog.SelectedPath;
                // Write to permanent configuration
                RSSettingsModel.Instance.saveProcessedFolderPermanent(ProcessedFolder);
                // Trigger Refresh
                RefreshProcessedFileList(null);

                OnPropertyRaised("ProcessedFolder");
            }

        }

        private void SelectRawFolder(object arg)
        {
            WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;
            folderDialog.SelectedPath = RawFolder;
            folderDialog.Description = "Pick RAW files Folder";
            WinForms.DialogResult result = folderDialog.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                // Send path to Model
                MyRawFolderListing.FolderPath = folderDialog.SelectedPath;
                // Set path in View Model
                RawFolder = folderDialog.SelectedPath;
                // Write to permanent configuration
                RSSettingsModel.Instance.saveRawFolderPermanent(RawFolder);
                // Trigger Refresh
                RefreshRawFileList(null);
                
                OnPropertyRaised("RawFolder");
            }

        }

        private void RefreshProcessedFileList(object arg)
        {
            MyProcessedFolderFiles.Clear();
            MyProcessedFolderListing.RefreshFolderListing();
            MyProcessedFolderListing.ReQualifyListedFiles(RSSettingsModel.Instance.ProcessedExtensionSetting);

            foreach (FileDescriptorModel fd in MyProcessedFolderListing.FileList)
            {
                MyProcessedFolderFiles.Add(fd);
            }

            MyProcessedGridView.MoveCurrentTo(null);

            OnPropertyRaised("MyProcessedFolderFilesNo");
            OnPropertyRaised("MyProcessedFilesNo");
            OnPropertyRaised("MyProcessedFilesShownNo");
        }

        private void RefreshRawFileList(object arg)
        {
            MyRawFolderFiles.Clear();
            MyRawFolderListing.RefreshFolderListing();
            MyRawFolderListing.ReQualifyListedFiles(RSSettingsModel.Instance.RawExtensionSetting);

            foreach (FileDescriptorModel fd in MyRawFolderListing.FileList)
            {
                MyRawFolderFiles.Add(fd);
            }

            MyRawGridView.MoveCurrentTo(null);

            HighlightDelta(CheckHighlightDelta);

            OnPropertyRaised("MyRawFolderFilesNo");
            OnPropertyRaised("MyRawFilesNo");
            OnPropertyRaised("MyRawFilesShownNo");
            
        }

        private void RemoveProcessedItem(object arg)
        {
            if (arg != null && (int)arg > -1)
            {
                // MyProcessedFolderFiles.RemoveAt((int)arg); Can't do this...
                // In order for us to properly remove Collection item, we have to precisely know,
                // what item is at this index of View (View may be filtered, thus index will be different)
                // FileDescriptorModel fdv = MyProcessedGridView.Cast<FileDescriptorModel>().ToArray()[(int)arg]; Although it works, this seems too complicated
                FileDescriptorModel fdv = (FileDescriptorModel)MyProcessedGridView.CurrentItem;
                if (fdv != null)
                {
                    MyProcessedFolderFiles.Remove(fdv);
                    MyProcessedFolderListing.FileList.Remove(fdv);
                    MyProcessedGridView.MoveCurrentTo(null);
                    OnPropertyRaised("MyProcessedFolderFilesNo");
                    OnPropertyRaised("MyProcessedFilesNo");
                    OnPropertyRaised("MyProcessedFilesShownNo");
                }
            }

        }

        private bool RemoveProcessedItemCanUse(object arg)
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

        private void RemoveRawItem(object arg)
        {
            if (arg != null && (int)arg > -1)
            {
                // MyRawFolderFiles.RemoveAt((int)arg); Can't do this...
                // In order for us to properly remove Collection item, we have to precisely know,
                // what item is at this index of View (View may be filtered, thus index will be different)
                // FileDescriptorModel fdv = MyRawGridView.Cast<FileDescriptorModel>().ToArray()[(int)arg]; Although it works, this seems too complicated
                FileDescriptorModel fdv = (FileDescriptorModel)MyRawGridView.CurrentItem;
                if(fdv != null)
                {
                    MyRawFolderFiles.Remove(fdv);
                    MyRawFolderListing.FileList.Remove(fdv);
                    MyRawGridView.MoveCurrentTo(null);
                    OnPropertyRaised("MyRawFolderFilesNo");
                    OnPropertyRaised("MyRawFilesNo");
                    OnPropertyRaised("MyRawFilesShownNo");
                }
            }

        }

        private bool RemoveRawItemCanUse(object arg)
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

        private void HighlightDelta(bool isOn)
        {

            // Mark if the raw file descriptor belongs to Delta, based on processed file list matching criteria:
            bool hasCorrespondent;
            foreach (FileDescriptorModel fd1 in MyRawFolderListing.FileList)
            {
                fd1.BelongsToDelta = false;
                if(!isOn)
                {
                    continue;
                }

                // For each qualified raw file descriptor 
                if (!fd1.IsQualified)
                {
                    continue;
                }

                hasCorrespondent = false;
                foreach (FileDescriptorModel fd2 in MyProcessedFolderListing.FileList)
                {
                    // For each qualified processed file descriptor
                    if (!fd2.IsQualified)
                    {
                        continue;
                    }

                    if (fd2.Name.IndexOf(fd1.Name) != -1)
                    {
                        string raw_num_str = Regex.Match(fd1.Name, @"\d+").Value;
                        string proc_num_str = Regex.Match(fd2.Name, @"\d+").Value;
                        int raw_num = Int32.Parse(raw_num_str);
                        int proc_num = Int32.Parse(proc_num_str);
                        
                        if (raw_num == proc_num)
                        {
                            hasCorrespondent = true;
                            break;
                        }
                    }
                }

                if (hasCorrespondent == false)
                {
                    fd1.BelongsToDelta = true;
                }
            }

            OnPropertyRaised("MyDeltaFilesNo");
            MyRawGridView.Refresh();

        }

        private void GetDelta(object arg)
        {
            // MessageBox.Show("GetDelta()", "Command invoked", MessageBoxButton.OK, MessageBoxImage.Information);

            // Instantiate Delta View Model
            DeltaViewModel DeltaVM = new DeltaViewModel(MyRawFolderListing);

            // Subscribe to the FilesDeleted Event
            DeltaVM.FilesDeletedEvent += OnFilesDeleted_EventHandler;
            
            // Instantiate the Delta dialog box View
            DeltaDialog DeltaDlg = new DeltaDialog();

            // Configure the dialog box
            DeltaDlg.Owner = Application.Current.MainWindow;
            DeltaDlg.DataContext = DeltaVM;
            DeltaDlg.Left = DeltaDlg.Owner.Left + 20;
            DeltaDlg.Top = DeltaDlg.Owner.Top + 50;
            DeltaDlg.Height = DeltaDlg.Owner.Height - 70;
            DeltaDlg.Width = DeltaDlg.Owner.Width / 2 - 11;

            // Open the dialog box non-modally (in order we can observe the entire raw list too...)
            DeltaDlg.Show();

            // When Delta Dialog Window closes, the DeltaVM object should be collected by GC and destroyed.
            // the DeltaVM lives shorter than the ShellVM, and has no references to ShellVM, so there is no need to remove the event handler from DeltaVM.
            // DeltaVM will not be destroyed by GC as long as the Delta Window is opened (per DataContext reference).
            return;
        }

        private void OnFilesDeleted_EventHandler()
        {
            // Refresh Models' listings, ShellViewModel's Collections, and their Views
            RefreshRawFileList(null);
            HighlightDelta(CheckHighlightDelta);
        }

        private bool GetDeltaCanUse(object arg)
        {
            return CheckHighlightDelta;
        }

        private void OpenSettings(object arg)
        {
            // MessageBox.Show("OpenSettings()", "Command invoked", MessageBoxButton.OK, MessageBoxImage.Information);

            // Instantiate Settings View Mmodel
            SettingsViewModel SVM = new SettingsViewModel(RSSettingsModel.Instance);

            // Subscribe for the SettingsChanged Event
            SVM.SettingsChangedEvent += OnSettingsChange_EventHandler;

            // Instantiate the Settings dialog box View
            SettingsDialog SettingsDlg = new SettingsDialog();

            // Configure the dialog box
            SettingsDlg.Owner = Application.Current.MainWindow;
            SettingsDlg.DataContext = SVM;
            
            // Open the dialog box modally
            SettingsDlg.ShowDialog();

            // When Settings Dialog Window closes, the SettingsVM object should be collected by GC and destroyed.
            // the SettingsVM lives shorter than the ShellVM, and has no references to ShellVM, so there is no need to remove the event handler from DeltaVM.
            // SettingsVM will not be destroyed by GC as long as the Delta Window is opened (per DataContext reference).
            return;
        }

        private void OnSettingsChange_EventHandler()
        {
            // Perform requalification of file listings (Models) and refresh collection Views -
            // file qualifications may have changed after settings changed...
            MyProcessedFolderListing.ReQualifyListedFiles(RSSettingsModel.Instance.ProcessedExtensionSetting);
            MyRawFolderListing.ReQualifyListedFiles(RSSettingsModel.Instance.RawExtensionSetting);
            MyProcessedGridView.Refresh();
            HighlightDelta(CheckHighlightDelta);
            return;
        }

        private void ProgramExit(object arg)
        {
            Application.Current.Shutdown();
        }
        
        private void ProgramHelp(object arg)
        {
            WWMessageBox.Show("Raw Sync 3.0:\n\nSafely delete rejected RAW files,\nbased on matching the list of Processed files.\n\n© 2020 by Wojtek Wasiukiewicz.", "Help", CustomMessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
