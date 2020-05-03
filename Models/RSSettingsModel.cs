using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MVVMDemo.Models
{
    class RSSettingsModel
    {
        private static readonly object padlock = new object();
        private static RSSettingsModel instance = null;

        public string ProcessedExtensionSetting { get; private set; }
        public string RawExtensionSetting { get; private set; }
        public bool DeletePromptSetting { get; private set; }
        public bool RecycleBinSetting { get; private set; }
        public string ProcessedFolderSetting { get; set; }
        public string RawFolderSetting { get; set; }

        private RSSettingsModel()
        {
            loadSettings();
        }

        public static RSSettingsModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new RSSettingsModel();
                    }
                    return instance;
                }

            }
        }

        private bool loadSettings()
        {
            // Wczytanie opcji ustawień z pliku kofiguracyjnego
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.AllKeys.Contains("RawExtension"))
                {
                    RawExtensionSetting = appSettings["RawExtension"];
                }
                else
                {
                    RawExtensionSetting = ".CR2"; // DEFAULT
                }

                if (appSettings.AllKeys.Contains("ProcessedExtension"))
                {
                    ProcessedExtensionSetting = appSettings["ProcessedExtension"];
                }
                else
                {
                    ProcessedExtensionSetting = ".JPG"; // DEFAULT
                }

                if (appSettings.AllKeys.Contains("DeletePrompt"))
                {
                    DeletePromptSetting = bool.Parse(appSettings["DeletePrompt"]);
                }
                else
                {
                    DeletePromptSetting = true; // DEFAULT
                }

                if (appSettings.AllKeys.Contains("RecycleBin"))
                {
                    RecycleBinSetting = bool.Parse(appSettings["RecycleBin"]);
                }
                else
                {
                    RecycleBinSetting = true; // DEFAULT
                }

                if (appSettings.AllKeys.Contains("RawFolder"))
                {
                    RawFolderSetting = appSettings["RawFolder"];
                }
                else
                {
                    RawFolderSetting = @"C:\"; // DEFAULT
                }

                if (appSettings.AllKeys.Contains("ProcessedFolder"))
                {
                    ProcessedFolderSetting = appSettings["ProcessedFolder"];
                }
                else
                {
                    ProcessedFolderSetting = @"C:\"; // DEFAULT
                }

            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error reading App settings.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public bool saveSettingsPermanent(ConfigurationPackage cfgpack)
        {
            //Zapisanie przekazanej konfiguracji do właściwości Modelu
            RawExtensionSetting = cfgpack._RawExtension;
            ProcessedExtensionSetting = cfgpack._ProcessedExtension;
            DeletePromptSetting = cfgpack._DeletePrompt;
            RecycleBinSetting = cfgpack._RecycleBin;
            
            // Zapisanie opcji ustawień do pliku konfiguracyjnego
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                if (settings["RawExtension"] == null)
                {
                    settings.Add("RawExtension", RawExtensionSetting);
                }
                else
                {
                    settings["RawExtension"].Value = RawExtensionSetting;
                }

                if (settings["ProcessedExtension"] == null)
                {
                    settings.Add("ProcessedExtension", ProcessedExtensionSetting);
                }
                else
                {
                    settings["ProcessedExtension"].Value = ProcessedExtensionSetting;
                }

                if (settings["DeletePrompt"] == null)
                {
                    settings.Add("DeletePrompt", DeletePromptSetting.ToString());
                }
                else
                {
                    settings["DeletePrompt"].Value = DeletePromptSetting.ToString();
                }

                if (settings["RecycleBin"] == null)
                {
                    settings.Add("RecycleBin", RecycleBinSetting.ToString());
                }
                else
                {
                    settings["RecycleBin"].Value = RecycleBinSetting.ToString();
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error writing app settings", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool saveProcessedFolderPermanent(string pf)
        {
            //Zapisanie przekazanej konfiguracji do właściwości Modelu
            ProcessedFolderSetting = pf;

            // Zapisanie opcji ustawień do pliku konfiguracyjnego
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                if (settings["ProcessedFolder"] == null)
                {
                    settings.Add("ProcessedFolder", ProcessedFolderSetting);
                }
                else
                {
                    settings["ProcessedFolder"].Value = ProcessedFolderSetting;
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error writing app settings", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool saveRawFolderPermanent(string rf)
        {
            //Zapisanie przekazanej konfiguracji do właściwości Modelu
            RawFolderSetting = rf;

            // Zapisanie opcji ustawień do pliku konfiguracyjnego
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                if (settings["RawFolder"] == null)
                {
                    settings.Add("RawFolder", RawFolderSetting);
                }
                else
                {
                    settings["RawFolder"].Value = RawFolderSetting;
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error writing app settings", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

    }
}
