using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DontTowMeBro.Resources;
using DontTowMeBro.Helpers;

using System.IO.IsolatedStorage;


namespace DontTowMeBro
{
    public partial class App : Application
    {
        //class members
       
        enum SessionType { None, Home, DeepLink }

        //Set to Home when the app is launched from Primary tile
        //Set to DeepLink when appi is launched from Deep Link
        private SessionType sessionType = SessionType.None;

        //Set to true when the page navigation is being reset
        bool wasRelaunched = false;

        //set to true when 3 min has passed since the app was first relaunched
        bool mustClearPagestack = false;

        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        private static AccessIsoStore store = null;
        public static AccessIsoStore Store
        {
            get
            {
                if (store == null)
                    store = new AccessIsoStore();

                return store;
            }

        }


        

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
           RemoveCurrentDeactivationSettings();
           
        }

       

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!App.Store.areFilesLoaded)
            {
                App.Store.returnFiles();
            }

            //// If some interval has passed since the app was deactivated (30 seconds in this example), 
            //// then remember to clear the back stack of pages 
            mustClearPagestack = CheckDeactivationTimeStamp();

            // // If IsApplicationInstancePreserved is not true, then set the session type to the value 
            // // saved in isolated storage. This will make sure the session type is correct for an 
            // // app that is being resumed after being tombstoned. 

            if (!e.IsApplicationInstancePreserved)
            {
                RestoreSessionType();
            }

             
        }

       

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // When the applicaiton is deactivated, save the current deactivation settings to isolated storage 
            SaveCurrentDeactivationSettings();
            

        }

        

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            // Ensure that required application state is persisted here.
            //Implement Save state for tombstoned app

            //when the application closes, delete any deactivation settings from isolated storage
            RemoveCurrentDeactivationSettings();
            
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Helper method to determine if the interval since the app was deactivated is 
        /// greater than 30 seconds 
        /// </summary>
        /// <returns>TimeSpan 3 mins</returns>
        private bool CheckDeactivationTimeStamp()
        {
            DateTimeOffset lastDeactivated;

            if (settings.Contains("DeactivateTime"))
            {
                lastDeactivated = (DateTimeOffset)settings["DeactivateTime"];
            }

            var currentDuration = DateTimeOffset.Now.Subtract(lastDeactivated);

            return TimeSpan.FromSeconds(currentDuration.TotalSeconds) > TimeSpan.FromSeconds(180); 

        }

        /// <summary>
        /// Called when the app is deactivating. Saves the time of the deactivation and the  
        /// session type of the app instance to isolated storage. 
        /// </summary>
        private void SaveCurrentDeactivationSettings()
        {
            if (AddOrUpdateValue("DeactivateTime", DateTimeOffset.Now))
            {
                settings.Save();
            }

            if (AddOrUpdateValue("SessionType", sessionType))
            {
                settings.Save();
            } 

        }

        // Helper method for adding or updating a key/value pair in isolated storage 
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists 
            if (settings.Contains(Key))
            {
                // If the value has changed 
                if (settings[Key] != value)
                {
                    // Store the new value 
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key. 
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        } 
 


        // Helper method for removing a key/value pair from isolated storage 
        public void RemoveValue(string Key)
        {
            // If the key exists 
            if (settings.Contains(Key))
            {
                settings.Remove(Key);
            }
        } 



        /// <summary>
        /// Called when the app is launched or closed. Removes all deactivation settings from 
        /// isolated storage 
        /// </summary>
        private void RemoveCurrentDeactivationSettings()
        {
            RemoveValue("DeactivateTime");
            RemoveValue("SessionType");
            settings.Save(); 
        }

        /// <summary>
        /// Helper method to restore the session type from isolated storage. 
        /// </summary>
        private void RestoreSessionType()
        {
            if (settings.Contains("SessionType"))
            {
                sessionType = (SessionType)settings["SessionType"];
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
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            try
            {
                //if session is None or New check if navigation is deep link  or if it points to main page
                if (sessionType == SessionType.None && e.NavigationMode == NavigationMode.New)
                {

                    //this block will run if the current navigation is part of part of the apps initial launch

                    //keep track of session type
                    if (e.Uri.ToString().Contains("DeepLink=true"))
                    {
                        sessionType = SessionType.DeepLink;
                    }//end if
                    else if (e.Uri.ToString().Contains("/MainPage.xaml"))
                    {
                        sessionType = SessionType.Home;
                    }//end else if
                }//end if

                    if (e.NavigationMode == NavigationMode.Reset)
                    {
                        // This block will execute if the current navigation is a relaunch. 
                        // If so, another navigation will be coming, so this records that a relaunch just happened 
                        // so that the next navigation can use this info. 

                        wasRelaunched = true;
                    }//end if
                    else if (e.NavigationMode == NavigationMode.New && wasRelaunched)
                    {
                        // This block will run if the previous navigation was a relaunch 
                        wasRelaunched = false;


                        if (e.Uri.ToString().Contains("DeepLink=true"))
                        {
                            sessionType = SessionType.DeepLink;
                            //the app was relaunchned via deep link
                            //the page stack will be cleared

                        }//end if
                        else if (e.Uri.ToString().Contains("/MainPage.xaml"))
                        {
                            if (sessionType == SessionType.DeepLink)
                            {
                                sessionType = SessionType.Home;
                            }//end if
                            else
                            {
                                if (!mustClearPagestack)
                                {
                                    e.Cancel = true;
                                    RootFrame.Navigated -= ClearBackStackAfterReset;

                                }//end if
                            }//end else
                        }//end else if

                        mustClearPagestack = false;
                    }//end elseif
                  
                
            }//end try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error App.Xaml.CS Line 237", MessageBoxButton.OK);
            }//end catch
        }//end

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}