using System.Windows;
using RawSync.Models;
using RawSync.ViewModels;

namespace RawSync
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            // This is required (just before the main window starts), in order to be able to display any dialog before the main window,
            // without closing the Application on this dialog close.
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // If there is any splash screen or login screen we could display it here.

            // Here we should instantiate SettingsModel singleton (and therefore read settings from file).
            RSSettingsModel ProgramSettings = RSSettingsModel.Instance;

            // Here we can instantiate the rest of the models which needs to stay on during App lifetime.
            // Instantiate FolderListingModels for Processed and Raw files folders
            FolderListingModel MyProcessedFolderListing = new FolderListingModel(ProgramSettings.ProcessedFolderSetting, ProgramSettings.ProcessedExtensionSetting);
            FolderListingModel MyRawFolderListing = new FolderListingModel(ProgramSettings.RawFolderSetting, ProgramSettings.RawExtensionSetting);

            // Here we sdould instantiate the ShellViewModel, which will stay on during App lifetime.
            ShellViewModel ShellVM = new ShellViewModel(MyProcessedFolderListing, MyRawFolderListing);


            // Create the ShellViewModel's View - i.e. MainWindow
            MainWindow wnd = new MainWindow();

            // Here we may optionally configure the Main Window or do some other stuff...
            wnd.DataContext = ShellVM;
            Application.Current.MainWindow = wnd;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            //Show the Main Window and proceed...
            wnd.Show();
        }

    }
}
