using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RawSync.Models;

namespace RawSync.ViewModels
{
    class SettingsViewModel
    {
        private RSSettingsModel ProgramSettings;

        private bool HasChanges;

        private string _ProcessedExtensionVM;
        private string _RawExtensionVM;
        private bool _DeletePromptVM;
        private bool _RecycleBinVM;

        public delegate void EventHandler();
        public event EventHandler SettingsChangedEvent;

        public string ProcessedExtensionVM
        {
            get { return _ProcessedExtensionVM; }
            set { _ProcessedExtensionVM = value; HasChanges = true; }
        }
        public string RawExtensionVM
        {
            get { return _RawExtensionVM; }
            set { _RawExtensionVM = value; HasChanges = true; }
        }
        public bool DeletePromptVM
        {
            get { return _DeletePromptVM; }
            set { _DeletePromptVM = value; HasChanges = true; }
        }
        public bool RecycleBinVM
        {
            get { return _RecycleBinVM; }
            set { _RecycleBinVM = value; HasChanges = true; }
        }

        public RelayCommand SaveAndCloseSettingsCommand { get; private set; }
        public RelayCommand CancelSettingsCommand { get; private set; }

        public SettingsViewModel(RSSettingsModel sm)
        {
            ProgramSettings = sm;
            // Populate properties with values from Model
            RawExtensionVM = ProgramSettings.RawExtensionSetting;
            ProcessedExtensionVM = ProgramSettings.ProcessedExtensionSetting;
            DeletePromptVM = ProgramSettings.DeletePromptSetting;
            RecycleBinVM = ProgramSettings.RecycleBinSetting;
            HasChanges = false;

            // Initiate commands
            SaveAndCloseSettingsCommand = new RelayCommand(SaveAndCloseSettings, SaveAndCloseSettingsCanUse);
            CancelSettingsCommand = new RelayCommand(CancelSettings);
        }

        private void SaveAndCloseSettings(object arg)
        {
            // Prepare configuration package and pass it to the Settings Model
            ConfigurationPackage cfgpack = new ConfigurationPackage();
            cfgpack._RawExtension = RawExtensionVM;
            cfgpack._ProcessedExtension = ProcessedExtensionVM;
            cfgpack._DeletePrompt = DeletePromptVM;
            cfgpack._RecycleBin = RecycleBinVM;
            if (!ProgramSettings.saveSettingsPermanent(cfgpack))
            {
                MessageBox.Show("SaveAndCloseSettings():\nCould not save settings.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Notify subscribers that settings changed - raise proper event to interested objects
            SettingsChangedEvent();

            // Close associated view window, xaml passes the view window reference as arg
            if (arg != null)
            {
                ((Window)arg).Close();
            }
            else
            {
                MessageBox.Show("SaveAndCloseSettings():\nView reference is null.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SaveAndCloseSettingsCanUse(object arg)
        {

            return HasChanges && !RawExtensionValidator.HasError && !ProcessedExtensionValidator.HasError;

            // 4. How to use NotifyOnValidationError="true" in XAML binding
            // 5. How to use NotifyOnSourceUpdated="true" in XAML binding
        }

        private void CancelSettings(object arg)
        {
            // Just close associated view window, xaml passes the view window reference as arg
            if (arg != null)
            {
                ((Window)arg).Close();
            }
            else
            {
                MessageBox.Show("CancelSettings():\nView reference is null.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
