using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Storage;
using System.Threading.Tasks;


namespace GTVWinPhone8
{
    public partial class App : Application
    {
        /// <summary>
        /// Component used to raise a notification to the end users to rate the application on the marketplace.
        /// </summary>
        public static string APIUrl = "http://gunceltelevizyon.azurewebsites.net/api/";
        public static string StorageUrl = "https://gtvstorage.blob.core.windows.net/channels/";
        public static StorageFolder localPics = ApplicationData.Current.LocalFolder;
        public const int VERSION = 1;
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        /// 

        private static global::SQLite.SQLiteAsyncConnection databaseConnection;
        public static global::SQLite.SQLiteAsyncConnection DatabaseConnection
        {
            get
            {
                if (databaseConnection == null && !string.IsNullOrWhiteSpace(Database))
                    databaseConnection = new global::SQLite.SQLiteAsyncConnection(Database, true);

                return databaseConnection;
            }
        }
        public static void CloseDatabaseLink()
        {

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
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = false;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        
              //Creates a new instance of the RadRateApplicationReminder component.
    
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private async void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //Before using any of the ApplicationBuildingBlocks, this class should be initialized with the version of the application.
            await PrepareApplicationForFirstRun();
        
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {

        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            //RootFrame.Background = new SolidColorBrush(Color.FromArgb(255, 3, 166, 120));
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            
            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}
