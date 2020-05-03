using RawSync.CustomMessageBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RawSync.Models
{
    public class FolderListingModel
    {
        private string Qualifier;

        private const string _folder = @"C:\";

        public string FolderPath { get; set; } = _folder;

        public List<FileDescriptorModel> FileList = new List<FileDescriptorModel>();

        public FolderListingModel(string Qualifier)
        {
            this.Qualifier = Qualifier;
            GetFolderListing();
            ReQualifyListedFiles(Qualifier);
        }

        public FolderListingModel(string folder, string Qualifier)
        {
            FolderPath = folder;
            this.Qualifier = Qualifier;
            GetFolderListing();
            ReQualifyListedFiles(Qualifier);
        }
        public void RefreshFolderListing()
        {
            FileList.Clear();
            GetFolderListing();
            ReQualifyListedFiles(Qualifier);
        }
        public void ReQualifyListedFiles(string Qualifier)

        {
            this.Qualifier = Qualifier;
            foreach(FileDescriptorModel fd in FileList)
            {
                fd.IsQualified = IsQualified(fd.Name, fd.Extension, Qualifier);
            }
        }

        private void GetFolderListing()
        {
            if (!Directory.Exists(FolderPath))
            {
                // Don't throw any exceptions, just inform the user and fallback to default folder, so he can change it later

                // This sometimes is called at startup, before MainWindow is created,
                // For this occasion, App.xaml.cs needs to modify ShutdownMode and MainWindow Application properties.

                WWMessageBox.Show($"GetFolderListing(): Folder does not exist:\n{ FolderPath }\nChanging to default...", "RAW Sync v. 3.0: Warning", CustomMessageBoxButton.OK, MessageBoxImage.Warning);
                FolderPath = _folder; // Fallback to default
            }

            string[] FileEntries = Directory.GetFiles(FolderPath);

            foreach (string s in FileEntries)
            {
                FileDescriptorModel fd = new FileDescriptorModel();
                fd.FilePath = s;
                FileList.Add(fd);
                fd.Id = FileList.Count;
            }
            return;
        }
        private bool IsQualified(string name, string extension, string qualifier)
        {
            // Verify if there is a match between Extension and Qualification Pattern from Program Settings.
            if (extension.Length != qualifier.Length)
            {
                return false;
            }
            for (int i = 0; i < qualifier.Length; i++)
            {
                if (qualifier[i] == '?')
                {
                    if (!Char.IsLetterOrDigit(extension[i]))
                        return false;
                }
                else
                {
                    if (extension[i] != qualifier[i])
                    {
                        return false;
                    }
                }

            }

            // Verify if files's Name is not a name of a lock file ("~$...").
            if (name.Length >= 2)
            {
                if (name[0] == '~' && name[1] == '$')
                {
                    return false;
                }
            }

            return true;

        }
    }
}
