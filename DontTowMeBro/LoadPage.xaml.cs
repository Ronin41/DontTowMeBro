using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections;
using DontTowMeBro.Helpers;
using System.Collections.ObjectModel;
using DontTowMeBro.Notifications;


//public struct SystemPacket
//{
//    public ParkingMeterNotification pm;
//    public GpsLoc loc;
//    public int pmMeterSelectState, pmWarnSelectState;
//}


namespace DontTowMeBro
{

    public partial class LoadPage : PhoneApplicationPage
    {
        //AccessIsoStore store = new AccessIsoStore();
       // public ObservableCollection<UserLoadedFile> userLoadedFile = new ObservableCollection<UserLoadedFile>();

        private FileHandler fh = new FileHandler();
        private object loadedFile;
        FileStruct fs;
        SystemPacket systemPacket = new SystemPacket(); 

        public LoadPage()
        {
            InitializeComponent();
            DataContext = App.Store;
            
            App.Store.returnFiles();
                   
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
                App.Store.returnFiles();
            
        }

       
        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ///Next step:
            ///Implement Navigation routine to send the ne file data to mainPage 
            ///fix saveFile issue missing PM Notification switch

            TextBlock filename = (sender as TextBlock);

            systemPacket.pm = new ParkingMeterNotification();

            if (MessageBox.Show("Are You Sure?", "Load " + filename, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                
                fh.LoadUserFile(out loadedFile, filename.Text);
                fs = (FileStruct)loadedFile;

                systemPacket.loc = fs.meterFile.loc;
                systemPacket.pm.bActive = fs.meterFile.active;
                systemPacket.pm.Return = fs.meterFile.MeterReturnTime;
                systemPacket.pm.Warning = fs.meterFile.MeterWarningTime;
                systemPacket.pmMeterSelectState = fs.meterFile.meterSelectedIndex;
                systemPacket.pmWarnSelectState = fs.meterFile.warnSelectedIndex;
                
                systemPacket.pz.bActive = fs.zoneFile.active;
                systemPacket.pz.loc = fs.zoneFile.loc;
                systemPacket.pz.firstSave = fs.zoneFile.firstSave;
                systemPacket.pz.returnTime = fs.zoneFile.parkingMeterReturn;
                systemPacket.pz.startTime = fs.zoneFile.parkingMeterStart;
                systemPacket.pz.warningTime = fs.zoneFile.parkingMeterWarning;


                systemPacket.loaded = true;
                Uri uri = new Uri("/MainPage.xaml", UriKind.Relative);
                NavigationService.Navigate(uri.ToString(), systemPacket);
                
            }
        }


    }

}