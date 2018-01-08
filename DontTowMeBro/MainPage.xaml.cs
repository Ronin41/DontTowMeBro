//Author: Michael Stewart
//Date: 1/30/2014
//Notes:
//Finish adding state items to save file <- complete 
//Implement scheduled phone Warning alarm <- complete
//Implement scheduled phone times up alarm <- complete
//Implement AppBar and user save option for repeatable configurations <- complete
//*Instead of numbered Long and Lat. Change text to read Position Set 
//Implement Parkingzone start button: module does not start until loc, warning, start, and stop are set
//Implement System settings save file (use for data ex: PZ start time bool) <- complete
//Implemented ParkingZone reset and start funcions: needs relook <- complete
//Implemented ParkingZone timing systems reqs testing and refinement <- complete
//                -Conneceted in MyTimer and added the clearing command to the PZReset method  
//Implement ParkingZone firsstSave: requires testing <- complete
//Load and Save presets to be done first patch after launch
//Known Bugs:
//    Unhandled exception from meter and warning selection list boxes *Fixed
//    Consolidate all of the packets into system packet
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DontTowMeBro.Resources;
using System.Windows.Media;
using System.Device.Location;
using DontTowMeBro.Helpers;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using DontTowMeBro.Notifications;
using Windows.Devices.Geolocation;
using System.Windows.Threading;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Shapes;
using System.Diagnostics;
using DontTowMeBro.ViewModels;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Maps.Services;


namespace DontTowMeBro
{

    public struct SystemPacket
    {
       public ParkingMeterNotification pm;
       public ParkingZoneNotification pz;
       public GpsLoc loc;
       public int pmMeterSelectState, pmWarnSelectState;
       public bool loaded;
       public pzStartTimePacket pzStartTP;
       public pzStopTimePacket pzStopTP;
       public pzWarningTimePacket pzWarnTP;
       
    }

   [DataContract]
    public struct pzStartTimePacket
    {
       [DataMember]
        public int hour;
       [DataMember]
        public int second;
       [DataMember]
        public int minute;
        [DataMember]
        public bool active;
        [DataMember]
        public bool pm;
    }

    [DataContract]
    public struct pzStopTimePacket
    {
         [DataMember]
        public int hour;
         [DataMember]
        public int second;
         [DataMember]
        public int minute;
         [DataMember]
        public bool active;
         [DataMember]
         public bool pm;
    }

    [DataContract]
    public struct pzWarningTimePacket
    {
         [DataMember]
        public int hour;
         [DataMember]
        public int second;
         [DataMember]
        public int minute;
         [DataMember]
        public bool active;
         [DataMember]
         public bool pm;
    }

    


    public partial class MainPage : PhoneApplicationPage, IDisposable
    {
        struct selectedItem
        {
            public int items;
        }

        //Members
        private object _WarnTimeSelected = new object(); 
        private object _MeterTimeSelected = new object();
        private GeoInfo newPMLocation;
        private GeoInfo newPZLocation;
        private GeoInfo userLocation = new GeoInfo();
        private ParkingMeterNotification PMNotification;
        private ParkingZoneNotification PZNotification;
        private GpsLoc newLoc;
        private GpsLoc userLoc;
        private DispatcherTimer newTimer;
        private Grid CarLocGrid, userLocGrid;
        private Rectangle userLocRec;
        private Rectangle carLocRec;
        private MapOverlay CarLocOverlay, userLocOverlay;
        private MapLayer CarLocLayer, userLocLayer;
        private bool gpsPMButtonActive = false;
        private bool gpsPZButtonActive = false;
        private MapRoute mapRoute;

        
        private List<GeoCoordinate> coordList = new List<GeoCoordinate>();
        private GeocodeQuery codeQuery = new GeocodeQuery();
        private RouteQuery routeQuery = new RouteQuery();
        private Route route;
        private FindMyCarFile newFile = new FindMyCarFile();

        

        private PMViewModel pmView = new PMViewModel();
        private List<selectedItem> pmListItemWarning = new List<selectedItem>();
        private List<selectedItem> pmListItemMeterTime = new List<selectedItem>();

        private MeterStateFile newMeterState = new MeterStateFile();//save file struct
        private ZoneStateFile newZoneState = new ZoneStateFile(); //save file struct
        private SettingsFile newSettingsState = new SettingsFile(); //save file struct

        private SystemActiveState newActiveStates = new SystemActiveState();

        private SaveState saveState = new SaveState();
        private LoadState loadState = new LoadState();
        private SystemPacket systemPacket = new SystemPacket();
        private SystemPacket loadedSystemPacket = new SystemPacket();
        private int loadPercent = 0;
        private bool newMainPageLoaded;
        private string sReturnTime, sWarnTime;
        private pzStartTimePacket pzStartPacket = new pzStartTimePacket();
        private pzStopTimePacket pzStopPacket = new pzStopTimePacket();
        private pzWarningTimePacket pzWarningPacket = new pzWarningTimePacket();
        private bool findMyCarActive = false;

        private Switch meterSwitch, warningSwitch;

        private string sHighLightMeterTime, sHighLightWarningTime;
        private bool skip = false;
        private Date newDate = new Date();

        private enum Switch { SELECTED, NOT_SELECTED }

        private ApplicationBarIconButton userLocBtn, userZoomBtn, carZoomBtn;

        private bool userToggle = false;

        private ImageBrush carImage, userImage;

        private FindRoute findRoute = new FindRoute();

        public ParkingMeterNotification pmNotification
        {
            get { return PMNotification; }
            set { }
        }

        public ParkingZoneNotification pzNotification
        {
            get { return PZNotification; }
            set { }
        }

        public enum ParkingType { PMeter, PZone };
        
        
        // Constructor
        public MainPage()
        {
           

            InitializeComponent();       

            //Add Main_Page to Loaded stack
            Loaded += MainPage_Loaded;

            tbPMCurrentLocation.Visibility  = Visibility.Collapsed;


            //Initialize GpsLoc
            newPMLocation = new GeoInfo();
            newPZLocation = new GeoInfo();
            
            //Initialize Parking Meter Notification
            PMNotification = new ParkingMeterNotification();
            //Initialize Parking Zone Notification
            PZNotification = new ParkingZoneNotification();

            tbPMCurrentLocation.IsEnabled = false;

            /////Location Marker///////////////////////////////////////////////

            CarLocGrid = new Grid();
            CarLocGrid.RowDefinitions.Add(new RowDefinition());
            CarLocGrid.ColumnDefinitions.Add(new ColumnDefinition());
            CarLocGrid.Background = new SolidColorBrush(Colors.Transparent);

            userLocGrid = new Grid();
            userLocGrid.RowDefinitions.Add(new RowDefinition());
            userLocGrid.ColumnDefinitions.Add(new ColumnDefinition());
            userLocGrid.Background = new SolidColorBrush(Colors.Transparent);
            

            //Create userLocRect
            userImage = new ImageBrush();
            userImage.ImageSource = new BitmapImage(new Uri("/Assets/UserLoc.png", UriKind.Relative));

            userLocRec = new Rectangle();
            userLocRec.Fill = userImage;
            userLocRec.Height = 78;
            userLocRec.Width = 78;
            userLocRec.SetValue(Grid.RowProperty, 0);
            userLocRec.SetValue(Grid.ColumnProperty, 0);

            //Create CarLocRect//////////////////////////////////////////////////
            carImage = new ImageBrush();
            carImage.ImageSource = new BitmapImage(new Uri("/Assets/CarLoc.png", UriKind.Relative));

            carLocRec = new Rectangle();
            carLocRec.Height = 78;
            carLocRec.Width = 78;
            carLocRec.Fill = carImage;
            carLocRec.SetValue(Grid.RowProperty, 0);
            carLocRec.SetValue(Grid.ColumnProperty, 0);
  

            //Initialize MapOverlay and Layers//////////////////////////////////////
            CarLocOverlay = new MapOverlay();
            CarLocOverlay.Content = CarLocGrid;

            CarLocLayer = new MapLayer();
            CarLocLayer.Add(CarLocOverlay);
            AppMap.Layers.Add(CarLocLayer);

            userLocOverlay = new MapOverlay();
            userLocOverlay.Content = userLocGrid;

            userLocLayer = new MapLayer();
            userLocLayer.Add(userLocOverlay);
            AppMap.Layers.Add(userLocLayer);
            /////////////////////////////////////////////////////////////////////////
           
            pmLBMeterTime.ItemsSource = pmView.meterTime;
            pmLBWarningTime.ItemsSource = pmView.warningTime;

            EventHandler e = new EventHandler(MyTimer);

            newTimer = new DispatcherTimer();
            newTimer.Tick += e;

            newMainPageLoaded = false;


            //capture loadedSystemPacket///////////////////////////////////////////////////////////////////////

            try
            {
                var myParameter = NavigationService.GetLastNavigationData();

                if (myParameter != null)
                {
                    loadedSystemPacket = (SystemPacket)myParameter; //System packet contains information from main page
                    if (loadedSystemPacket.loaded == true)
                    {
                        LoadNewUserFile();
                        loadPercent += 50;

                        if (loadPercent == 100)
                        {
                            loadedSystemPacket.loaded = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MainPage() Error", MessageBoxButton.OK);
            }
            
            

            //load saved settings
            LoadSettings();

            
            
            findMyCarActive = newSettingsState.findMyCarActive;
               
            
            gpsPMButtonActive = newSettingsState.gpsPMButtonActive;
            gpsPZButtonActive = newSettingsState.gpsPZButtonActive;

            pzWarningPacket = newSettingsState.WarningTimePacket;
            pzStartPacket   = newSettingsState.StartTimePacket;
            pzStopPacket    = newSettingsState.StopTimePacket;

            userToggle = newSettingsState.userToggle;

            if(newSettingsState.pzStartText != null)
            btnPZSetStart.Content = newSettingsState.pzStartText;

            if (newSettingsState.pzStopText != null)
            btnPZSetStop.Content = newSettingsState.pzStopText;

            if (newSettingsState.pzWarnText != null)
            btnPZSetWarn.Content = newSettingsState.pzWarnText;

            sHighLightMeterTime = newSettingsState.meterTime;
            sHighLightWarningTime = newSettingsState.warnigTime;

            LoadMeterListLastPos(sHighLightMeterTime, sHighLightWarningTime);
            

            if (pzWarningPacket.active == true)
            {
                warnSetIndicator.Fill = new SolidColorBrush(Colors.Green);
                PZNotification.SetWarningTime(pzWarningPacket.hour, pzWarningPacket.minute, pzWarningPacket.pm);
            }
            else
            {
                warnSetIndicator.Fill = new SolidColorBrush(Colors.Yellow);
                PZNotification.SetWarningTime(pzWarningPacket.hour, pzWarningPacket.minute, pzWarningPacket.pm);
            }

            if (pzStartPacket.active == true)
            {
                startSetIndicator.Fill = new SolidColorBrush(Colors.Green);
                PZNotification.SetStartTime(pzStartPacket.hour, pzStartPacket.minute, pzStartPacket.pm);
            }
            else
            {
                startSetIndicator.Fill = new SolidColorBrush(Colors.Yellow);
                PZNotification.SetStartTime(pzStartPacket.hour, pzStartPacket.minute, pzStartPacket.pm);
            }

            if (pzStopPacket.active == true)
            {
                stopSetIndicator.Fill = new SolidColorBrush(Colors.Green);
                PZNotification.SetReturnTime(pzStopPacket.hour, pzStopPacket.minute, pzStopPacket.pm);
            }
            else
            {
                stopSetIndicator.Fill = new SolidColorBrush(Colors.Yellow);
                PZNotification.SetReturnTime(pzStopPacket.hour, pzStopPacket.minute, pzStopPacket.pm);
            }


            //turn button into toggle
            if (newSettingsState.userToggle == true)
            {
                if (userLocGrid.Children.Contains(userLocRec) == false)
                    userLocGrid.Children.Add(userLocRec);

                userLocation.UserLocation();

                if (userLocation.userLoc != null)
                {
                    userLocation.userLoc.PositionChanged += userLoc_PositionChanged;
                }

            }
            
            
            ///////////////////////////////////////////////////////////////////////////////////////////////////

            // Sample code to localize the ApplicationBar
               BuildLocalizedApplicationBar();

               Debug.WriteLine("Creating new instance of main page");
            

        }//end


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                codeQuery.Dispose();
                routeQuery.Dispose();
            }
        }

        ~MainPage()
        {
            Dispose(false);
        }

        /// <summary>
        /// Add Map Updates to loaded 
        /// </summary>object 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            Update_Map();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
           //ActiveSystemStats will be loaded. Which ever system is true will then get its state loaded and activated//
           //Load prev state
            if (newMainPageLoaded == false)
            {
                SetProgressIndicator(true);
                SystemTray.ProgressIndicator.Text = "Loading Settings";
                loadState.LoadActiveSystemStates(out newActiveStates);
                LoadNewState();
                if (gpsPMButtonActive == true)
                {
                    ImageBrush setLocImage = new ImageBrush();
                    Uri uri = new Uri("/Assets/Start_Button.png", UriKind.RelativeOrAbsolute);
                    setLocImage.ImageSource = new BitmapImage(uri);
                    setLocImage.Stretch = Stretch.UniformToFill;

                    btnPMSetLoc.Background = setLocImage;
                }
                if ( gpsPZButtonActive == true)
                {
                    ImageBrush setLocImage = new ImageBrush();
                    Uri uri = new Uri("/Assets/Start_Button.png", UriKind.RelativeOrAbsolute);
                    setLocImage.ImageSource = new BitmapImage(uri);
                    setLocImage.Stretch = Stretch.UniformToFill;

                    pzBtnLoc.Background = setLocImage;
                }

                SystemTray.ProgressIndicator.Text = "Complete";
                SetProgressIndicator(false);

                if (findMyCarActive == true)
                {
                    FindMyCar();
                }

                if (sHighLightMeterTime != null)
                {
                    MeterHighlight(true);
                    skip = true;
                }
                else
                {
                    meterSwitch = Switch.SELECTED;
                    
                }

                if (sHighLightWarningTime != null)
                {
                    WarningHighlight(true);
                }
                else
                {
                    warningSwitch = Switch.SELECTED;
                }

                newMainPageLoaded = true;
            }
        }

       

        /// <summary>
        /// Update the map
        /// </summary>
        private void Update_Map()
        {
            //Updated from notification modules
            if (PMNotification.bActive == true)
            {
                try
                {
                    //Add Items to grid
                    if (CarLocGrid.Children.Contains(carLocRec) == false)
                    CarLocGrid.Children.Add(carLocRec);
                    AppMap.SetView(new GeoCoordinate((double)newLoc.GetLatitude(), (double)newLoc.GetLongitude()), 17D);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Update Map error; line 87 MainPage.xaml.cs", MessageBoxButton.OK);
                }
            }
            else if (PZNotification.bActive == true)
            {
                try
                {
                    //Add Items to grid
                    if (CarLocGrid.Children.Contains(carLocRec) == false)
                        CarLocGrid.Children.Add(carLocRec);
                    AppMap.SetView(new GeoCoordinate((double)newLoc.GetLatitude(), (double)newLoc.GetLongitude()), 17D);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Update Map error; line 87 MainPage.xaml.cs", MessageBoxButton.OK);
                }
            }

        }//end

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

           
            
            //Navigate to individual Pivot Items
            string strItemIndex = null;
            if (NavigationContext.QueryString.TryGetValue("goto", out strItemIndex))
            {
                parkingPivot.SelectedIndex = Convert.ToInt32(strItemIndex);
                NavigationContext.QueryString.Clear();
            }
            
            
            /************************************************************************
             *Checks if the user has allowed the app to use the phones location
             ************************************************************************/
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                //Load Settings
                //LoadSettings();
                //return;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location. Is that ok?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            //Load Settings
            LoadSettings();

            base.OnNavigatedTo(e);
        }//end

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SaveNewState();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isVisble"></param>
        private static void SetProgressIndicator(bool isVisble)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisble;
            SystemTray.ProgressIndicator.IsVisible = isVisble;
        }

        /// <summary>
        /// Async method; waits for the gps coords to upload before displaying
        /// </summary>
        public async void GetPos(ParkingType type)
        {

            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            try
            {
               
                    SetProgressIndicator(true);
                    SystemTray.ProgressIndicator.Text = "Getting GPS Location";

                    bool x = false;

                    if (type == ParkingType.PMeter)
                    {
                        x = await this.newPMLocation.GetPos();
                    }

                    if (type == ParkingType.PZone)
                    {
                        x = await this.newPZLocation.GetPos();
                    }


                    SystemTray.ProgressIndicator.Text = "Aquired";
                    SetProgressIndicator(false);

                    if (x == true)
                    {
                        /////////////////////////////////////////////
                        //newLoc Set Here
                        /////////////////////////////////////////////
                        if (type == ParkingType.PMeter)
                        {
                            newLoc = this.newPMLocation.GetGeolocation();
                            PMNotification.loc = newLoc;

                            findRoute.carCoord.Latitude = newLoc.GetLatitude();
                            findRoute.carCoord.Longitude = newLoc.GetLongitude();

                            tbPMCurrentLocation.IsEnabled = true;
                            tbPMCurrentLocation.Text = "Latitude:" + newLoc.GetLatitude().ToString() + "\n" + "Longitude:" + newLoc.GetLongitude().ToString();
                            tbPMCurrentLocation.IsEnabled = false;
                        }

                        if (type == ParkingType.PZone)
                        {
                            newLoc = this.newPZLocation.GetGeolocation();
                            PZNotification.loc = newLoc;
                            findRoute.carCoord.Latitude = newLoc.GetLatitude();
                            findRoute.carCoord.Longitude = newLoc.GetLongitude();
                        }
                        

                        //Save newLoc Information
                        systemPacket.loc.SetLatitude(newLoc.GetLatitude());
                        systemPacket.loc.SetLongitude(newLoc.GetLongitude());
                          
                    }
                
            }//end try
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    MessageBox.Show("location  is disabled in phone settings.", "Location Setting Error", MessageBoxButton.OK); 

                }//end if
             }//end catch

        }//end GetPos
            
              
        

        /// <summary>
        /// Sets Location variables
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPMSetLoc_Click(object sender, RoutedEventArgs e)
        {

            ImageBrush setLocImage = new ImageBrush();
            Uri uri = new Uri("/Assets/Start_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage.ImageSource = new BitmapImage(uri);
            setLocImage.Stretch = Stretch.UniformToFill;

            newPMLocation = new GeoInfo();
            GetPos(ParkingType.PMeter);
            btnPMSetLoc.Background = setLocImage;

            gpsPMButtonActive = true;

            ImageBrush setLocImage2 = new ImageBrush();
            Uri uri2 = new Uri("/Assets/Red_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage2.ImageSource = new BitmapImage(uri2);
            setLocImage2.Stretch = Stretch.UniformToFill;

            pzBtnLoc.Background = setLocImage2;
            gpsPZButtonActive = false;
            
        }

        /// <summary>
        /// Parkikng Meter Activation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPMStart_Click(object sender, RoutedEventArgs e)
        {
            if (newActiveStates.ParkingZone == false)
            {
                if (PMNotification.bActive == false)
                {

                    if (sReturnTime != null && sWarnTime != null && gpsPMButtonActive == true)
                    {
                        //Data should be collected from here and sent to storage object
                        PMNotification.SetNewMeterTime(Convert.ToDouble(sReturnTime));//needs error checking for empty condition
                        //Data should be collected from here and sent to storage object
                        PMNotification.SetNewMeterWarningTime(Convert.ToDouble(sWarnTime));

                        PMNotification.StartNotification();

                        //bPMWarning = true;
                        //bPMMeter = true;

                        //save activation state
                        newMeterState.active = PMNotification.bActive;
                        newActiveStates.ParkingMeter = true;
                        saveState.SaveActiveSystemStates(newActiveStates);

                        Update_Map();

                        try
                        {

                            CarLocOverlay.GeoCoordinate = new GeoCoordinate(newLoc.GetLatitude(), newLoc.GetLongitude());
                            CarLocOverlay.PositionOrigin = new Point(0, 0.5);

                            PMNotification.MeterStartTime();
                            newTimer.Start();
                            if (newTimer.IsEnabled)
                            {
                                PMNotification.AddRemoveAlarm(ParkingMeterNotification.AlarmType.WARNING, true);
                                PMNotification.AddRemoveAlarm(ParkingMeterNotification.AlarmType.TIMES_UP, true);
                            }

                            parkingPivot.SelectedIndex = 0;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                        }
                    }//end if 2
                    else
                    {
                        MessageBox.Show("Return, Warning, or Gps is not set", "Warning", MessageBoxButton.OK);
                    }
                }//end if 1
            }//end if
            else
            {
                MessageBox.Show("Parking Zone is active!", "Warning", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Parking Meter Reset Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPMReset_Click(object sender, RoutedEventArgs e)
        {
            
            SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
            SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);
            SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

            ImageBrush setLocImage = new ImageBrush();
            Uri uri = new Uri("/Assets/Red_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage.ImageSource = new BitmapImage(uri);
            setLocImage.Stretch = Stretch.UniformToFill;

            gpsPMButtonActive = false;

                sWarnTime = null;
                sReturnTime = null;
               
                PMNotification.bActive = false;

                tbPMCurrentLocation.IsEnabled = true;
                tbPMCurrentLocation.Text = "Latitude:" + "0" + "\n" + "Longitude:" + "0";
                tbPMCurrentLocation.IsEnabled = false;

                newMeterState.firstSaved = false;
                newMeterState.warnSelectedIndex = -1;
                newMeterState.meterSelectedIndex = -1;

                btnPMSetLoc.Background = setLocImage;
                tbCountDown.Text = "00:00:00";
                //newTimer.Stop();

                btnSafe.Background = greenBrush;

               

                List<UserControl> listItems1 = new List<UserControl>();
                List<UserControl> listItems2 = new List<UserControl>();
                try
                {
                    GetItemsRecursive<UserControl>(pmLBMeterTime, ref listItems1);
                    GetItemsRecursive<UserControl>(pmLBWarningTime, ref listItems2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "GetItemsRecursive Error; MainPage.xaml.cs", MessageBoxButton.OK);
                }

                if (listItems1.Count > 0)
                {
                    foreach (UserControl userControl in listItems1)
                    {
                        VisualStateManager.GoToState(userControl, "Normal", true);
                        pmListItemMeterTime.Clear();
                    }
                }

                if (listItems2.Count > 0)
                {
                    foreach (UserControl userControl in listItems2)
                    {
                        VisualStateManager.GoToState(userControl, "Normal", true);
                        pmListItemWarning.Clear();
                    }
                }

                pmNotification.loc = new GpsLoc();
                pmNotification.StopNotification();
                
                PMNotification.AddRemoveAlarm(ParkingMeterNotification.AlarmType.WARNING, false);
                PMNotification.AddRemoveAlarm(ParkingMeterNotification.AlarmType.TIMES_UP, false);

                sHighLightMeterTime = null;
                sHighLightWarningTime = null;

                if (CarLocGrid.Children.Contains(carLocRec))
                    CarLocGrid.Children.Remove(carLocRec);

                SaveNewState();

                newActiveStates.ParkingMeter = false;
                saveState.SaveActiveSystemStates(newActiveStates);
            

        }

        /// <summary>
        /// Sets ParkingZone GPS Location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pzBtnLoc_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush setLocImage = new ImageBrush();
            Uri uri = new Uri("/Assets/Start_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage.ImageSource = new BitmapImage(uri);
            setLocImage.Stretch = Stretch.UniformToFill;

            PZNotification.firstSave = false;
            newPZLocation = new GeoInfo();
            GetPos(ParkingType.PZone);
            pzBtnLoc.Background = setLocImage;

            gpsPZButtonActive = true;

            ImageBrush setLocImage2 = new ImageBrush();
            Uri uri2 = new Uri("/Assets/Red_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage2.ImageSource = new BitmapImage(uri2);
            setLocImage2.Stretch = Stretch.UniformToFill;

            btnPMSetLoc.Background = setLocImage2;
            gpsPMButtonActive = false;

        }//end

        /// <summary>
        /// Start Parkingzone module, updatemap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pzBtnStart_Click(object sender, RoutedEventArgs e)
        {
            
            if (newActiveStates.ParkingMeter == false)
            {
                if (pzStartPacket.active == true && pzStopPacket.active == true && 
                    pzWarningPacket.active == true && gpsPZButtonActive == true)
                {
                    if (pzNotification.bActive == false)
                    {
                        PZNotification.firstSave = false;

                        PZNotification.SetReturnTime(newSettingsState.StopTimePacket.hour,
                            newSettingsState.StopTimePacket.minute, newSettingsState.StopTimePacket.pm);
                        PZNotification.SetStartTime(newSettingsState.StartTimePacket.hour,
                            newSettingsState.StartTimePacket.minute, newSettingsState.StartTimePacket.pm);
                        PZNotification.SetWarningTime(newSettingsState.WarningTimePacket.hour,
                            newSettingsState.WarningTimePacket.minute, newSettingsState.WarningTimePacket.pm);

                        PZNotification.StartNotification();
                        Update_Map();

                        //Save States
                        newZoneState.active = pzNotification.bActive;
                        newActiveStates.ParkingZone = true;
                        saveState.SaveActiveSystemStates(newActiveStates);

                        try
                        {
                            CarLocOverlay.GeoCoordinate = new GeoCoordinate(newLoc.GetLatitude(), newLoc.GetLongitude());
                            CarLocOverlay.PositionOrigin = new Point(0, 0.5);

                            newTimer.Start();

                            if (newTimer.IsEnabled)
                            {
                                PZNotification.AddRemoveAlarm(ParkingZoneNotification.AlarmType.WARNING, true);
                                PZNotification.AddRemoveAlarm(ParkingZoneNotification.AlarmType.TIMES_UP, true);
                            }

                            parkingPivot.SelectedIndex = 0;

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error: MainPage.xaml.cs", MessageBoxButton.OK);
                        }
                    }//end if
                }//end if
                else
                {
                    MessageBox.Show("Start, Stop, Warning, or Gps are not set", "Warning", MessageBoxButton.OK);
                }
            }//end if
            else
            {
                MessageBox.Show("Parking Meter is active!", "Warning", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Resets the Parking Zone Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pzBtnReset_Click(object sender, RoutedEventArgs e)
        {
			SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
            SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

            ImageBrush setLocImage = new ImageBrush();
            Uri uri = new Uri("/Assets/Red_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage.ImageSource = new BitmapImage(uri);
            setLocImage.Stretch = Stretch.UniformToFill;

            gpsPZButtonActive = false;

            pzStartTimePacket start;
            pzStopTimePacket stop;
            pzWarningTimePacket warning;

            start.active = false;
            start.hour   = 0;
            start.minute = 0;
            start.second = 0;
            start.pm = false;

            stop.active = false;
            stop.hour   = 0;
            stop.minute = 0;
            stop.second = 0;
            stop.pm = false;

            warning.active = false;
            warning.hour   = 0;
            warning.minute = 0;
            warning.second = 0;
            warning.pm = false;

            loadedSystemPacket = new SystemPacket();

			btnSafe.Background = greenBrush;

            newSettingsState.StartTimePacket = start;
            newSettingsState.StopTimePacket = stop;
            newSettingsState.WarningTimePacket = warning;

            pzStartPacket = newSettingsState.StartTimePacket;
            pzWarningPacket = newSettingsState.WarningTimePacket;
            pzStopPacket = newSettingsState.StopTimePacket; 

            warnSetIndicator.Fill = new SolidColorBrush(Colors.Yellow);
            PZNotification.SetWarningTime(pzWarningPacket.hour, pzWarningPacket.minute, pzWarningPacket.pm);
                       
            startSetIndicator.Fill = new SolidColorBrush(Colors.Yellow);
            PZNotification.SetStartTime(pzStartPacket.hour, pzStartPacket.minute, pzStartPacket.pm);       
            
            stopSetIndicator.Fill = new SolidColorBrush(Colors.Yellow);
            PZNotification.SetReturnTime(pzStopPacket.hour, pzStopPacket.minute, pzStopPacket.pm);

            PZNotification.loc = new GpsLoc();

            PZNotification.AddRemoveAlarm(ParkingZoneNotification.AlarmType.TIMES_UP, false);
            PZNotification.AddRemoveAlarm(ParkingZoneNotification.AlarmType.WARNING, false);

            PZNotification.StopNotification();

            PZNotification.firstSave = false;

            btnPZSetStart.Content = "Set Start";
            btnPZSetStop.Content = "Set Stop";
            btnPZSetWarn.Content = "Set Warning";

            tbCountDown.Text = "00:00:00";
            pzBtnLoc.Background = setLocImage;

            if (CarLocGrid.Children.Contains(carLocRec))
                CarLocGrid.Children.Remove(carLocRec);

            SaveNewState();

            newActiveStates.ParkingZone = false;
            saveState.SaveActiveSystemStates(newActiveStates);

           

        }




        

        /// <summary>
        /// Timer Package
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyTimer(object sender, EventArgs e)
        {
            SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
            SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
            SolidColorBrush orangeBrush = new SolidColorBrush(Colors.Orange);

           
            //Paring Meter Notification////////////////////////////////////////////////////////////
            /*
             * Controls green/red/orange warning indicators
             * 
             */ 



            if (PMNotification.bActive == true)
            {
                if (PMNotification.MeterWarning() == true)
                {
                  Dispatcher.BeginInvoke(delegate() { btnSafe.Background = orangeBrush; });

                  if (PMNotification.MeterEndTime() == true)
                  {
                      Dispatcher.BeginInvoke(delegate() { btnSafe.Background = redBrush; });

                      tbPMCurrentLocation.IsEnabled = true;
                      tbPMCurrentLocation.Text = "Latitude:" + "0" + "\n" + "Longitude:" + "0";
                      tbPMCurrentLocation.IsEnabled = false;

                      PMNotification.bActive = false;
                      newActiveStates.ParkingMeter = false;

                      newTimer.Stop();

                      tbCountDown.Text = "00:00:00";

                      //gpsPMButtonActive = false;

                      ImageBrush setLocImage = new ImageBrush();
                      Uri uri2 = new Uri("/Assets/Red_Button.png", UriKind.RelativeOrAbsolute);
                      setLocImage.ImageSource = new BitmapImage(uri2);
                      setLocImage.Stretch = Stretch.UniformToFill;

                      //btnPMSetLoc.Background = setLocImage;

                      //MeterHighlight(false);
                      //WarningHighlight(false);

                      saveState.SaveActiveSystemStates(newActiveStates);

                  }//end if
                }//end if 
                        else
                        {
                            Dispatcher.BeginInvoke(delegate() { btnSafe.Background = greenBrush; });                   
                        }

                //stops time, resets settings when meter time has run out
                //if (PMNotification.MeterEndTime()
                //    == true)
                //{

                //}
                    
                tbCountDown.Text = PMNotification.RemainingMeterTime();
            }//end if
            //////////////////////////////////////////////////////////////////////////////////////
            if (PZNotification.bActive == true)
            {
                //set items that need to be updated here
                if (PZNotification.PZWarning(newDate.GetBaseTime()) == true)
                {
                    Dispatcher.BeginInvoke(delegate() { btnSafe.Background = orangeBrush; });
                    if (PZNotification.PZReturn(newDate.GetBaseTime()) == true)
                    {
                        Dispatcher.BeginInvoke(delegate() { btnSafe.Background = redBrush; });
                        PZNotification.StopNotification();

                        newTimer.Stop();
                        tbCountDown.Text = "00:00:00";

                        ImageBrush setLocImage2 = new ImageBrush();
                        Uri uri2 = new Uri("/Assets/Red_Button.png", UriKind.RelativeOrAbsolute);
                        setLocImage2.ImageSource = new BitmapImage(uri2);
                        setLocImage2.Stretch = Stretch.UniformToFill;

                        //pzBtnLoc.Background = setLocImage2;
                        //gpsPZButtonActive = false;
            

       
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(delegate() { btnSafe.Background = greenBrush; });
                }
                tbCountDown.Text = PZNotification.RemainingZoneTime();
            }

            

        }//endMyTimer

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            //tPm.Dispose();
        }


        

        /// <summary>
        /// Check which system was active first then load the proper state file
        /// </summary>
        private void LoadNewState()
        {
            //Load states here

            if (newActiveStates.ParkingMeter == true)
            {
                newMeterState = (MeterStateFile)loadState.LoadNewState(StateType.PARKINGMETER);


                this.PMNotification.bActive = newMeterState.active;
                newLoc = newMeterState.loc;
                this.PMNotification.Return = newMeterState.MeterReturnTime;
                this.PMNotification.Start = newMeterState.MeterStartTime;
                this.PMNotification.Warning = newMeterState.MeterWarningTime;
                this.gpsPMButtonActive = newMeterState.gpsButtonActive;

                LoadedPMStart();

                newPMLocation = new GeoInfo();
                GetPos(ParkingType.PMeter);
            }
            else if (newActiveStates.ParkingZone == true)
            {
                
                newZoneState = (ZoneStateFile)loadState.LoadNewState(StateType.PARKINGZONE);

                this.PZNotification.bActive = newZoneState.active;
                newLoc = newZoneState.loc;
                this.PZNotification.returnTime = newZoneState.parkingMeterReturn;
                this.PZNotification.warningTime = newZoneState.parkingMeterWarning;
                this.PZNotification.startTime = newZoneState.parkingMeterStart;
                this.PZNotification.firstSave = newZoneState.firstSave;
                this.gpsPZButtonActive = newZoneState.gpsButtonActive;
               

                LoadedPZStart();

                newPZLocation = new GeoInfo();
                GetPos(ParkingType.PZone);
            }

            LoadSettings();

        }

        /// <summary>
        /// Loads new user file captured from LoadPage class
        /// </summary>
        private void LoadNewUserFile()
        {
            this.PMNotification = loadedSystemPacket.pm;
            newLoc = loadedSystemPacket.loc;

            tbPMCurrentLocation.IsEnabled = true;
            tbPMCurrentLocation.Text = "Latitude:" + newLoc.GetLatitude().ToString() + "\n" + "Longitude:" + newLoc.GetLongitude().ToString();
            tbPMCurrentLocation.IsEnabled = false;
        }

        /// <summary>
        /// Initiate Parking Meter System
        /// </summary>
        private void LoadedPMStart()
        {

            PMNotification.StartNotification();

            //save activation state
            newMeterState.active = PMNotification.bActive;
            newActiveStates.ParkingMeter = true;

            saveState.SaveActiveSystemStates(newActiveStates);

            //Update Meter State Struct here


            Update_Map();

            try
            {
                if (PMNotification.bActive == true)
                {
                    CarLocOverlay.GeoCoordinate = new GeoCoordinate(newLoc.GetLatitude(), newLoc.GetLongitude());
                    CarLocOverlay.PositionOrigin = new Point(0, 0.5);
                    
                }
                newTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void LoadedPZStart()
        {
            //SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);


            PZNotification.StartNotification();

            //save activation State
            newZoneState.active = PZNotification.bActive;
            newActiveStates.ParkingZone = true;

            saveState.SaveActiveSystemStates(newActiveStates);

            Update_Map();

            try
            {
                if (PZNotification.bActive == true)
                {
                    CarLocOverlay.GeoCoordinate = new GeoCoordinate(newLoc.GetLatitude(), newLoc.GetLongitude());
                    CarLocOverlay.PositionOrigin = new Point(0, 0.5);
                    //pzBtnLoc.Background = greenBrush;
                }
                newTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Saves current active system state
        /// </summary>
        private void SaveNewState()
        {
            //if (newActiveStates.ParkingMeter == true)
            //{
                newMeterState.active = PMNotification.bActive;
                newMeterState.loc = newLoc;
                newMeterState.MeterReturnTime = PMNotification.Return;
                newMeterState.MeterStartTime = PMNotification.Start;
                newMeterState.MeterWarningTime = PMNotification.Warning;                
                newMeterState.firstSaved = true;
                newMeterState.gpsButtonActive = gpsPMButtonActive;
                saveState.SaveSystemState(StateType.PARKINGMETER, newMeterState);
            //}
            //else if (newActiveStates.ParkingZone == true)
            //{
                if (PZNotification.firstSave == false)
                {
                    newZoneState.active = PZNotification.bActive;
                    newZoneState.loc = newLoc;
                    newZoneState.parkingMeterReturn = PZNotification.returnTime;
                    newZoneState.parkingMeterStart = PZNotification.startTime;
                    newZoneState.parkingMeterWarning = PZNotification.warningTime;
                    newZoneState.firstSave = true;
                    newZoneState.gpsButtonActive = gpsPZButtonActive;
                    
                    saveState.SaveSystemState(StateType.PARKINGZONE, newZoneState);
                    
                }
            //}

            SaveSettings("full");


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">
        /// quick
        /// full
        /// </param>
        private void SaveSettings(string type)
        {
           string _type = type.ToLower();
            switch(_type)
            {
                case "full":
                    {
                        newSettingsState.StartTimePacket = pzStartPacket;
                        newSettingsState.StopTimePacket = pzStopPacket;
                        newSettingsState.WarningTimePacket = pzWarningPacket;
                        newSettingsState.pzStartText = (string)btnPZSetStart.Content;
                        newSettingsState.pzStopText = (string)btnPZSetStop.Content;
                        newSettingsState.pzWarnText = (string)btnPZSetWarn.Content;
                        newSettingsState.gpsPMButtonActive = gpsPMButtonActive;
                        newSettingsState.gpsPZButtonActive = gpsPZButtonActive;
                        newSettingsState.findMyCarActive = this.findMyCarActive;
                        newSettingsState.meterTime = this.sHighLightMeterTime;
                        newSettingsState.warnigTime = this.sHighLightWarningTime;
                        newSettingsState.userToggle = this.userToggle;
                        

                        saveState.SaveSystemState(StateType.SETTINGS, newSettingsState);
                    }break;
                case "quick":
                    {
                        saveState.SaveSystemState(StateType.SETTINGS, newSettingsState);
                    }break;
            }
            
        }


        /// <summary>
        /// Loads all app settings
        /// </summary>
        private void LoadSettings()
        {
          
          newSettingsState = new SettingsFile();
          newSettingsState = (SettingsFile)loadState.LoadNewState(StateType.SETTINGS);
        }

        /// <summary>
        /// Override back button
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            SaveNewState();        
        }

       

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            //ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
            //appBarButton.Text = AppResources.AppBarButtonText;
            //ApplicationBar.Buttons.Add(appBarButton); 

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem Save = new ApplicationBarMenuItem(AppResources.AppBarMenuItemSave);
            //ApplicationBar.MenuItems.Add(Save);
            Save.Click += Save_Click;

            ApplicationBarMenuItem Load = new ApplicationBarMenuItem(AppResources.AppBarMenuItemLoad);
            //ApplicationBar.MenuItems.Add(Load);
            Load.Click += Load_Click;

            ApplicationBarMenuItem About = new ApplicationBarMenuItem(AppResources.AppBarMenuItemAbout);
            ApplicationBar.MenuItems.Add(About);
            About.Click += About_Click;

            ApplicationBarMenuItem Instructions = new ApplicationBarMenuItem(AppResources.AppBarInstructions);
            ApplicationBar.MenuItems.Add(Instructions);
            Instructions.Click += Instructions_Click;

            //ApplicationBarMenuItem UserLoc = new ApplicationBarMenuItem(AppResources.UserSetLoc);
           
            userLocBtn = new ApplicationBarIconButton(new Uri("/Assets/UserLocOn.png", UriKind.RelativeOrAbsolute));
            userLocBtn.Text = "Track Me";
            userLocBtn.Click += userLocBtn_Click;
            ApplicationBar.Buttons.Add(userLocBtn);

            userZoomBtn = new ApplicationBarIconButton(new Uri("/Assets/ZoomUser.png", UriKind.Relative));
            userZoomBtn.Text = "User Zoom";
            userZoomBtn.Click += userZoomBtn_Click;
            ApplicationBar.Buttons.Add(userZoomBtn);

            carZoomBtn = new ApplicationBarIconButton(new Uri("/Assets/ZoomCar.png", UriKind.Relative));
            carZoomBtn.Text ="Car Zoom";
            carZoomBtn.Click+=carZoomBtn_Click;
            ApplicationBar.Buttons.Add(carZoomBtn);
        }

        void carZoomBtn_Click(object sender, EventArgs e)
        {
            AppMap.SetView(new GeoCoordinate((double)newLoc.GetLatitude(), (double)newLoc.GetLongitude()), 17D);
            if (trackMe == true)
                trackMe = false;
        }

        void userZoomBtn_Click(object sender, EventArgs e)
        {
            if (trackMe == false)
            {
                trackMe = true;
                AppMap.SetView(new GeoCoordinate(userLoc.GetLatitude(), userLoc.GetLongitude()), 17D);
            }
            else
                trackMe = false;

           
        }

        void userLocBtn_Click(object sender, EventArgs e)
        {
            //turn button into toggle
            if (userToggle == false)
            {
                if (userLocGrid.Children.Contains(userLocRec) == false)
                    userLocGrid.Children.Add(userLocRec);

                userLocation.UserLocation();

                if (userLocation.userLoc != null)
                {
                    userLocation.userLoc.PositionChanged += userLoc_PositionChanged;
                }

                userToggle = true;
                newSettingsState.userToggle = userToggle;

                
            }
            else
            {
                if (userToggle == true)
                {
                    if (userLocGrid.Children.Contains(userLocRec))
                        userLocGrid.Children.Remove(userLocRec);


                    if (userLocation.userLoc != null)
                    {
                        userLocation.userLoc.PositionChanged -= userLoc_PositionChanged;
                    }

                    if (trackMe == true)
                        trackMe = false;

                    userToggle = false;
                    newSettingsState.userToggle = userToggle;
                }
            }

        }

    
        void userLoc_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(() => 
            {
                userLocation.SetGeoloction(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                userLoc = userLocation.GetGeolocation();

                userLocOverlay.GeoCoordinate = new GeoCoordinate(userLoc.GetLatitude(), userLoc.GetLongitude());
                userLocOverlay.PositionOrigin = new Point(0, 0.5);

                if (trackMe == true)
                    AppMap.SetView(new GeoCoordinate(userLoc.GetLatitude(), userLoc.GetLongitude()), 17D);

            });
        } 

        void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Park-Watch" + "\n" +"Steel Bengal Software" + "\n" + "8 June 2014"
                + "\n"+"steelbengalsw@gmail.com" + "\n" + "https://twitter.com/SteelBengal" + "\n" + "Version: 1.1.1.0" ,  "About", MessageBoxButton.OK);
            
        }


        void Instructions_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("/Instructions.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
            SaveSettings("Full");
        }

        private void Save_Click(object sender, EventArgs e)
        {
            
            systemPacket.pm = PMNotification;
            systemPacket.pz = PZNotification;
            systemPacket.pzStartTP = pzStartPacket;
            systemPacket.pzStopTP = pzStopPacket;
            systemPacket.pzWarnTP = pzWarningPacket;
            
            Uri uri = new Uri("/SavePage.xaml", UriKind.Relative);

            NavigationService.Navigate(uri.ToString(), systemPacket);
        }//end



        private void Load_Click(object sender, EventArgs e)
        {
               
            Uri uri = new Uri("/LoadPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }//end


        private void btnPZSetStart_Click(object sender, RoutedEventArgs e)
        {
            PZNotification.firstSave = false;
            SaveSettings("full");
            Uri uri = new Uri("/PZStartPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }//end

        private void btnPZSetStop_Click(object sender, RoutedEventArgs e)
        {
            PZNotification.firstSave = false;
            SaveSettings("full");
            Uri uri = new Uri("/PZStopPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }//end

        private void btnPZSetWarn_Click(object sender, RoutedEventArgs e)
        {
            PZNotification.firstSave = false;
            SaveSettings("full");
            Uri uri = new Uri("/PZWarnPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }//end




        private void startSetIndicator_Loaded(object sender, RoutedEventArgs e)
        {
            //check parking zone warning setting to see is active and set the proper indecator colors

            string h = null;
            string m = null;
            string s = null; 

            if (loadedSystemPacket.pzStartTP.active == true)
            {
                startSetIndicator.Fill = new SolidColorBrush(Colors.Green);

                if (loadedSystemPacket.pzStartTP.hour < 10)
                    h = "0" + loadedSystemPacket.pzStartTP.hour.ToString();
                else
                    h = loadedSystemPacket.pzStartTP.hour.ToString();

                if (loadedSystemPacket.pzStartTP.minute < 10)
                    m = "0" + loadedSystemPacket.pzStartTP.minute.ToString();
                else
                    m = loadedSystemPacket.pzStartTP.minute.ToString();

                if (loadedSystemPacket.pzStartTP.second < 10)
                    s = "0" + loadedSystemPacket.pzStartTP.second.ToString();
                else
                    s = loadedSystemPacket.pzStartTP.second.ToString();

                string start = h + ":" + m + ":" + s;

                if (loadedSystemPacket.pzStartTP.pm == true)
                {
                    
                    btnPZSetStart.Content = "Start: " + start + " PM";
                }
                else
                {
                    btnPZSetStart.Content = "Start: " + start + " AM";
                }

                PZNotification.SetStartTime(loadedSystemPacket.pzStartTP.hour, loadedSystemPacket.pzStartTP.minute, loadedSystemPacket.pzStartTP.pm);
            }
        }//end

        private void stopSetIndicator_Loaded(object sender, RoutedEventArgs e)
        {
            string h = null;
            string m = null;
            string s = null; 

            if (loadedSystemPacket.pzStopTP.active == true)
            {

                if (loadedSystemPacket.pzStopTP.hour < 10)
                    h = "0" + loadedSystemPacket.pzStopTP.hour.ToString();
                else
                    h = loadedSystemPacket.pzStopTP.hour.ToString();

                if (loadedSystemPacket.pzStopTP.minute < 10)
                    m = "0" + loadedSystemPacket.pzStopTP.minute.ToString();
                else
                    m = loadedSystemPacket.pzStopTP.minute.ToString();

                if (loadedSystemPacket.pzStopTP.second < 10)
                    s = "0" + loadedSystemPacket.pzStopTP.second.ToString();
                else
                    s = loadedSystemPacket.pzStopTP.second.ToString();

                string stop = h + ":" + m + ":" + s;

                stopSetIndicator.Fill = new SolidColorBrush(Colors.Green);

                if (loadedSystemPacket.pzStopTP.pm == true)
                    btnPZSetStop.Content = "Stop: " + stop + " PM";
                else
                    btnPZSetStop.Content = "Stop: " + stop + " AM";

                PZNotification.SetReturnTime(loadedSystemPacket.pzStopTP.hour, loadedSystemPacket.pzStopTP.minute, loadedSystemPacket.pzStopTP.pm);
            }
        }//end

        private void warnSetIndicator_Loaded(object sender, RoutedEventArgs e)
        {
            string h = null;
            string m = null;
            string s = null; 

            if (loadedSystemPacket.pzWarnTP.active == true)
            {

                if (loadedSystemPacket.pzWarnTP.hour < 10)
                    h = "0" + loadedSystemPacket.pzWarnTP.hour.ToString();
                else
                    h = loadedSystemPacket.pzWarnTP.hour.ToString();

                if (loadedSystemPacket.pzWarnTP.minute < 10)
                    m = "0" + loadedSystemPacket.pzWarnTP.minute.ToString();
                else
                    m = loadedSystemPacket.pzWarnTP.minute.ToString();

                if (loadedSystemPacket.pzWarnTP.second < 10)
                    s = "0" + loadedSystemPacket.pzWarnTP.second.ToString();
                else
                    s = loadedSystemPacket.pzWarnTP.second.ToString();

                string warning = h + ":" + m + ":" + s;

                warnSetIndicator.Fill = new SolidColorBrush(Colors.Green);

                if (loadedSystemPacket.pzWarnTP.pm == true)
                    btnPZSetWarn.Content = "Warning: " + warning + " PM";
                else
                    btnPZSetWarn.Content = "Warning: " + warning + " AM";

                PZNotification.SetWarningTime(loadedSystemPacket.pzWarnTP.hour, loadedSystemPacket.pzWarnTP.minute, loadedSystemPacket.pzWarnTP.pm);
            }
        }


        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pmLBMeterTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          //Grab Items
            List<UserControl> listItems = new List<UserControl>();
            
            
           
                meterSwitch = Switch.SELECTED;

                if (skip == true)
                {
                    ((object[])(e.RemovedItems))[0] = sHighLightMeterTime;
                }

                try
                {
                    GetItemsRecursive<UserControl>(pmLBMeterTime, ref listItems);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "GetItemsRecursive Error; MainPage.xaml.cs", MessageBoxButton.OK);
                }

                try
                {
                    if (pmListItemMeterTime.Count > 0)
                    {
                        foreach (selectedItem item in pmListItemMeterTime)
                        {
                            if (e.AddedItems[0].Equals(item.items.ToString()))
                            {
                                meterSwitch = Switch.NOT_SELECTED;
                                pmListItemMeterTime.Remove(item);
                                break;
                            }//end
                        }//end foreach
                    }//end if
                    else
                    {
                        meterSwitch = Switch.SELECTED;
                    }//end else

                    if (meterSwitch == Switch.SELECTED)
                    {
                        selectedItem item = new selectedItem();
                        item.items = Convert.ToInt16(e.AddedItems[0].ToString());
                        pmListItemMeterTime.Clear();
                        pmListItemMeterTime.Add(item);
                        sReturnTime = item.items.ToString();
                    }//end if
                }//end try
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error; MainPage.xaml.cs / pmLBMeterTime_SelectionChanged", MessageBoxButton.OK);
                }
            
               


                    //controls selected highlight state
                    switch (meterSwitch)
                    {
                        case Switch.SELECTED:
                            {
                                if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
                                {
                                    foreach (UserControl userControl in listItems)
                                    {
                                        VisualStateManager.GoToState(userControl, "Normal", true);
                                    }//end foreach

                                    foreach (UserControl userControl in listItems)
                                    {
                                        if (e.AddedItems[0].Equals(userControl.DataContext))
                                        {
                                            sHighLightMeterTime = (string)userControl.DataContext;
                                            VisualStateManager.GoToState(userControl, "Selected", true);
                                        }
                                    }
                                }//end if
                            } break;
                        case Switch.NOT_SELECTED:
                            {
                                if (e.RemovedItems.Count > 0 && e.RemovedItems[0] != null)
                                {
                                    foreach (UserControl userControl in listItems)
                                    {
                                        if (e.RemovedItems[0].Equals(userControl.DataContext))
                                        {
                                            VisualStateManager.GoToState(userControl, "Normal", true);
                                            sHighLightMeterTime = null;
                                        }//end if
                                    }//end foreach
                                }//end if
                            } break;

                    }//end switch

               
        }


        private void MeterHighlight(bool selected)
        {
            List<UserControl> listItems = new List<UserControl>();
            GetItemsRecursive<UserControl>(pmLBMeterTime, ref listItems);

            try
            {
                if (selected == true)
                {
                    foreach (UserControl userControl in listItems)
                    {
                        if (userControl.DataContext.ToString().Equals(sHighLightMeterTime))
                        {
                            VisualStateManager.GoToState(userControl, "Selected", true);
                            selectedItem item = new selectedItem();
                            item.items = Convert.ToInt16(sHighLightMeterTime);
                            pmListItemMeterTime.Add(item);
                            skip = true;
                        }
                    }
                }
                else
                {
                    foreach (UserControl userControl in listItems)
                    {
                        if (userControl.DataContext.ToString().Equals(sHighLightMeterTime))
                        {
                            VisualStateManager.GoToState(userControl, "Normal", true);
                            selectedItem item = new selectedItem();
                            item.items = Convert.ToInt16(sHighLightMeterTime);
                            pmListItemMeterTime.Add(item);
                            skip = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: MeterHighlight, MainPage" + ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void pmLBMeterTime_Loaded(object sender, RoutedEventArgs e)
        {
            if (sHighLightMeterTime != null)
                MeterHighlight(true);
        }
        private void pmLBWarningTime_Loaded(object sender, RoutedEventArgs e)
        {
            if (sHighLightWarningTime != null)
                WarningHighlight(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pmLBWarningTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Grab Items
            List<UserControl> listItems = new List<UserControl>();
            warningSwitch = Switch.SELECTED;

            try
            {
                GetItemsRecursive<UserControl>(pmLBWarningTime, ref listItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetItemsRecursive Error; MainPage.xaml.cs", MessageBoxButton.OK);
            }

            try
            {
                if (pmListItemWarning.Count > 0)
                {
                    foreach (selectedItem item in pmListItemWarning)
                    {
                        if (e.AddedItems[0].Equals(item.items.ToString()))
                        {
                            warningSwitch = Switch.NOT_SELECTED;
                            pmListItemWarning.Remove(item);
                            break;
                        }//end
                    }//end foreach
                }//end if
                else
                {
                    warningSwitch = Switch.SELECTED;
                }//end else

                if (warningSwitch == Switch.SELECTED)
                {
                    selectedItem item = new selectedItem();
                    item.items = Convert.ToInt16(e.AddedItems[0].ToString());
                    pmListItemWarning.Clear();
                    pmListItemWarning.Add(item);
                    sWarnTime = item.items.ToString();
                }//end if

                //controls selected highlight state
                switch (warningSwitch)
                {
                    case Switch.SELECTED:
                        {
                            if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
                            {
                                foreach (UserControl userControl in listItems)
                                {
                                    VisualStateManager.GoToState(userControl, "Normal", true);
                                }//end foreach

                                foreach (UserControl userControl in listItems)
                                {
                                    if (e.AddedItems[0].Equals(userControl.DataContext))
                                    {
                                        sHighLightWarningTime = (string)userControl.DataContext;
                                        VisualStateManager.GoToState(userControl, "Selected", true);
                                    }
                                }
                            }//end if
                        } break;
                    case Switch.NOT_SELECTED:
                        {
                            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] != null)
                            {
                                foreach (UserControl userControl in listItems)
                                {
                                    if (e.RemovedItems[0].Equals(userControl.DataContext))
                                    {
                                        VisualStateManager.GoToState(userControl, "Normal", true);
                                    }//end if
                                }//end foreach
                            }//end if
                        } break;

                }//end switch

            }//end try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error; MainPage.xaml.cs / pmLBWarningTime_SelectionChanged", MessageBoxButton.OK);
            }  
        }//end


        private void WarningHighlight(bool selected)
        {
            List<UserControl> listItems = new List<UserControl>();

            GetItemsRecursive<UserControl>(pmLBWarningTime, ref listItems);

            try
            {
                if (selected == true)
                {
                    foreach (UserControl userControl in listItems)
                    {
                        if (userControl.DataContext.ToString().Equals(sHighLightWarningTime))
                        {
                            VisualStateManager.GoToState(userControl, "Selected", true);
                            warningSwitch = Switch.SELECTED;
                        }
                    }
                }
                else
                {
                    foreach (UserControl userControl in listItems)
                    {
                        if (userControl.DataContext.ToString().Equals(sHighLightWarningTime))
                        {
                            VisualStateManager.GoToState(userControl, "Normal", true);
                            warningSwitch = Switch.SELECTED;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: WarningHighlight, MainPage" + ex.Message, "Error", MessageBoxButton.OK);
            }

        }



        /// <summary>
        /// Recursive get the item.
        /// </summary>
        /// <typeparam name="T">The item to get.</typeparam>
        /// <param name="parents">Parent container.</param>
        /// <param name="objectList">Item list</param>
        public static void GetItemsRecursive<T>(DependencyObject parents, ref List<T> objectList) where T : DependencyObject
        {
            try
            {
                var childrenCount = VisualTreeHelper.GetChildrenCount(parents);


                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parents, i);


                    if (child is T)
                    {
                        objectList.Add(child as T);
                    }


                    GetItemsRecursive<T>(child, ref objectList);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: GetItemRecursive, MainPage.xaml.cs" + e.Message, "Error", MessageBoxButton.OK);
            }


            return;
        }

        private void AppMap_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "5e27e672-0f08-4fa0-bc3a-8a70af512f98";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "dtKQzzde1T-85VgMe2EiZA";
        }

        private void FindMyCar()
        {
            ImageBrush setLocImage = new ImageBrush();
            Uri uri = new Uri("/Assets/Start_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage.ImageSource = new BitmapImage(uri);
            setLocImage.Stretch = Stretch.UniformToFill;

            ImageBrush setLocImage2 = new ImageBrush();
            Uri uri2 = new Uri("/Assets/Red_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage2.ImageSource = new BitmapImage(uri2);
            setLocImage2.Stretch = Stretch.UniformToFill;

            try
            {
                if (findRoute.carCoord.IsUnknown == false)
                {
                    AppMap.Center.Latitude = findRoute.carCoord.Latitude;
                    AppMap.Center.Longitude = findRoute.carCoord.Longitude;
                    btnFindMyCar.Background = setLocImage;
                    GetRoute();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex.Message, "Warning", MessageBoxButton.OK);
            }
           
        }


        private void btnFindMyCar_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush setLocImage = new ImageBrush();
            Uri uri = new Uri("/Assets/Start_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage.ImageSource = new BitmapImage(uri);
            setLocImage.Stretch = Stretch.UniformToFill;

            ImageBrush setLocImage2 = new ImageBrush();
            Uri uri2 = new Uri("/Assets/Red_Button.png", UriKind.RelativeOrAbsolute);
            setLocImage2.ImageSource = new BitmapImage(uri2);
            setLocImage2.Stretch = Stretch.UniformToFill;

            try
            {
                if (findMyCarActive == false)
                {
                    if (findRoute.carCoord.IsUnknown == false)
                    {
                        AppMap.Center.Latitude = findRoute.carCoord.Latitude;
                        AppMap.Center.Longitude = findRoute.carCoord.Longitude;
                        btnFindMyCar.Background = setLocImage;
                        findMyCarActive = true;
                        GetRoute();
                    }
                    else
                    {
                        MessageBox.Show("GPS Has Not Been Set","Warning",MessageBoxButton.OK);
                    }
                    
                }
                else
                {
                    findMyCarActive = false;
                    btnFindMyCar.Background = setLocImage2;

                    if(mapRoute != null)
                    AppMap.RemoveRoute(mapRoute);

                    if(newFile.file != null)
                    newFile.file.dirList.Clear();

                    saveState.SaveSystemState(StateType.FINDMYCAR, newFile);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("MainPage.xaml.cs, btnFindMyCar_Click" + ex.Message, "Warning", MessageBoxButton.OK);
            }
           
        }


        public async void GetRoute()
        {
           
            try
            {
                if (findRoute.carCoord.IsUnknown == false)
                {
                    SetProgressIndicator(true);
                    SystemTray.ProgressIndicator.Text = "Getting Route Directions";

                    //Get user position
                    Geolocator locator = new Geolocator();
                    locator.DesiredAccuracy = PositionAccuracy.High;
                    Geoposition position = await locator.GetGeopositionAsync();
                    GeoCoordinate currentLoc = new GeoCoordinate(position.Coordinate.Latitude, position.Coordinate.Longitude);


                    if (this.coordList.Count > 0)
                    {
                        this.coordList.Clear();
                        this.coordList.Add(currentLoc);
                    }
                    else
                        this.coordList.Add(currentLoc);


                    //Get car location
                    this.codeQuery = new GeocodeQuery();
                    this.codeQuery.SearchTerm = "San Antonio, tx";
                    this.codeQuery.GeoCoordinate = currentLoc;
                    this.codeQuery.QueryCompleted += codeQuery_QueryCompleted;
                    this.codeQuery.QueryAsync();

                    SystemTray.ProgressIndicator.Text = "Aquired";
                    SetProgressIndicator(false);
                }//enf if
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: GeoInfo.cs, MapRoute, GetRoute:" + e.Message, "Error", MessageBoxButton.OK);
            }
            

           
        }

        void codeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error != null)
            {
                return;
            }



            this.routeQuery = new RouteQuery();
            //this.coordList.Add(e.Result[0].GeoCoordinate); //uses search term
            this.coordList.Add(findRoute.carCoord);
            this.routeQuery.Waypoints = coordList;
            this.routeQuery.TravelMode = TravelMode.Walking;
            this.routeQuery.QueryCompleted += routeQuery_QueryCompleted;
            this.routeQuery.QueryAsync();


        }

        void routeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<Route> e)
        {


            try
            {
                newFile.file = new FindMyCarViewModel();

                route = e.Result;

                mapRoute = new MapRoute(route);

                mapRoute.DisplayOutline = true;

                AppMap.AddRoute(mapRoute);
                

                foreach (RouteLeg leg in route.Legs)
                {
                    for (int i = 0; i < leg.Maneuvers.Count; i++)
                    {
                        RouteManeuver maneuver = leg.Maneuvers[i];
                       
                        newFile.file.dirList.Add(String.Format("{0}.{1}", i + 1, maneuver.InstructionText));
                    }
                }
                //Save route list here
                saveState.SaveSystemState(StateType.FINDMYCAR, newFile);
            }
            catch (Exception ex)
            { 
                if(e.Error.InnerException != null)
                MessageBox.Show("Error, MainPage.xaml.cs, routeQuery_QueryCompleted: " + e.Error.InnerException.Message,"Error", MessageBoxButton.OK);
                else if(e.Error != null)
                MessageBox.Show("Error, MainPage.xaml.cs, routeQuery_QueryCompleted: " + e.Error.Message,"Error", MessageBoxButton.OK);
                else
                MessageBox.Show("Error, MainPage.xaml.cs, routeQuery_QueryCompleted: " + ex.Message, "Error", MessageBoxButton.OK);

            }
            
        }

        private void btnDirections_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings("Full");
            Uri uri = new Uri("/MapDirection.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void LoadMeterListLastPos(string meter_mins, string warning_mins)
        {
            List<UserControl> meterListItems = new List<UserControl>();
            List<UserControl> warningListItems = new List<UserControl>();

                GetItemsRecursive<UserControl>(pmLBMeterTime, ref meterListItems);
                GetItemsRecursive<UserControl>(pmLBWarningTime, ref warningListItems);

                try
                {


                if(meter_mins != null)
                foreach (UserControl x in meterListItems)
                {
                    if (x.DataContext.ToString().Equals(meter_mins))
                    {
                        VisualStateManager.GoToState(x, "Selected", true);
                    }
                }

                if(warning_mins != null)
                foreach (UserControl x in warningListItems)
                {
                    if (x.DataContext.ToString().Equals(warning_mins))
                    {
                        VisualStateManager.GoToState(x, "Selected", true);
                    }
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("MainPage, LoadMetetListLastPos, " + ex.Message, "Error", MessageBoxButton.OK);
            }

           
        }







        public bool trackMe { get; set; }
    }//end class


    /************************************************************************************************************************************************************/

    /// <summary>
    /// Navigation Service extension for sending data between classes
    /// </summary>
    public static class NavigationExtensions
    {
        private static object _navigationData = null;

        public static void Navigate(this NavigationService service, string page, object data)
        {
            _navigationData = data;
            service.Navigate(new Uri(page, UriKind.Relative));
        }

        public static object GetLastNavigationData(this NavigationService service)
        {
            object data = _navigationData;
            _navigationData = null;
            return data;
        }

        
    }//end class



}