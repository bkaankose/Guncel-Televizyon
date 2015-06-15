using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Networking.PushNotifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GTVWin8.Database;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.Messaging;
using System.Diagnostics;
// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace GTVWin8
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        
        public static string APIUrl = "http://gunceltelevizyon.azurewebsites.net/api/";
        public static string StorageUrl = "https://gtvstorage.blob.core.windows.net/channels/";
        private string NotificationsHubName = "gunceltelevizyon";
        private string NotificationsHubListenKey = "Endpoint=sb://gunceltelevizyon.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=dT9K/XJhgKVPTjebEzY6wHUOR7RjLjMnlnEKWmfWuQs=";
        public const int VERSION = 1;
        public static StorageFolder localPics = ApplicationData.Current.LocalFolder;
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static string database;
        public static string Database
        {
            get
            {
                return database;
            }
            set
            {
                if (database != null && database.Equals(value)) return;

                CloseDatabaseLink();
                database = value;
            }
        }

        private static global::GTVWin8.Database.SQLiteAsyncConnection databaseConnection;
        public static global::GTVWin8.Database.SQLiteAsyncConnection DatabaseConnection
        {
            get
            {
                if (databaseConnection == null && !string.IsNullOrWhiteSpace(Database))
                    databaseConnection = new global::GTVWin8.Database.SQLiteAsyncConnection(Database, true);

                return databaseConnection;
            }
        }

        private static SQLiteAsyncConnection asyncDatabaseConnection;

        public static SQLiteAsyncConnection AsyncDatabaseConnection
        {
            get
            {
                if (asyncDatabaseConnection == null && !string.IsNullOrWhiteSpace(Database) && DatabaseConnection != null)
                    asyncDatabaseConnection = new global::GTVWin8.Database.SQLiteAsyncConnection(Database, true);

                return asyncDatabaseConnection;
            }
            set
            {
                asyncDatabaseConnection = value;
            }
        }
        public static void CloseDatabaseLink()
        {
            if (databaseConnection != null)
                databaseConnection.Dispose();

            if (asyncDatabaseConnection != null)
                asyncDatabaseConnection.Dispose();

            databaseConnection = null;
            asyncDatabaseConnection = null;
        }
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

        }
        private async Task PrepareApplicationForFirstRun(bool overrideExistingDatabase = false)
        {
            var result = false;
            var sf = ApplicationData.Current.LocalFolder;

            var name = "gtv.db";
            var ddd = await sf.GetFilesAsync();
            var database = (await sf.GetFilesAsync()).SingleOrDefault(i => i.Name == name);

            if (overrideExistingDatabase && database != null)
            {
                App.CloseDatabaseLink();
                await database.DeleteAsync();
                database = null;
            }

            if (database == null)
            {
                database = (await sf.GetFilesAsync()).SingleOrDefault(i => i.Name == name);
                result = true;

                if (database == null)
                {
                    var data = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Database/gtv.db", UriKind.Absolute));
                    await data.CopyAsync(sf, name);
                    database = (await sf.GetFilesAsync()).SingleOrDefault(i => i.Name == name);
                }
            }

            App.Database = database.Path;
        }
        private async Task InitNotificationsAsync()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            var hub = new NotificationHub(NotificationsHubName, NotificationsHubListenKey);
            await hub.RegisterNativeAsync(channel.Uri);
        }
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await PrepareApplicationForFirstRun();
            //await InitNotificationsAsync();
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                


                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
