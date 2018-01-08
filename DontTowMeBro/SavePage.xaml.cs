/*Author: Michael Stewart
 *Date: 2/3/14
 *File:SavePage.xaml.cs
 *Notes: Connect filename input with SaveUserFile filename input
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DontTowMeBro.Helpers;
using DontTowMeBro.Notifications;

//public struct SystemPacket
//{
//    public ParkingMeterNotification pm;
//    public GpsLoc loc;
//    public int pmMeterSelectState, pmWarnSelectState;
//}


namespace DontTowMeBro
{
    public partial class SavePage : PhoneApplicationPage
    {
       
        private UserSaveFile newUserFile = new UserSaveFile();
        private FileStruct newSaveFile = new FileStruct();
        private SystemPacket systemPacket = new SystemPacket();

        public SavePage()
        {
            InitializeComponent();
            var myParameter = NavigationService.GetLastNavigationData();
            systemPacket = (SystemPacket)myParameter; //System packet contains information from main page
            
        }

        /// <summary>
        /// Save user file to storage
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {

            newSaveFile.meterFile.active = systemPacket.pm.bActive;
            newSaveFile.meterFile.firstSaved = true;
            newSaveFile.meterFile.loc = systemPacket.loc;
            newSaveFile.meterFile.MeterReturnTime = systemPacket.pm.Return;
            newSaveFile.meterFile.MeterStartTime = systemPacket.pm.Start;
            newSaveFile.meterFile.MeterWarningTime = systemPacket.pm.Warning;
            newSaveFile.meterFile.meterSelectedIndex = systemPacket.pmMeterSelectState;
            newSaveFile.meterFile.warnSelectedIndex = systemPacket.pmWarnSelectState;

            newSaveFile.zoneFile.active = systemPacket.pz.bActive;
            newSaveFile.zoneFile.firstSave = systemPacket.pz.firstSave;
            newSaveFile.zoneFile.loc = systemPacket.pz.loc;
            newSaveFile.zoneFile.parkingMeterReturn = systemPacket.pz.returnTime;
            newSaveFile.zoneFile.parkingMeterStart = systemPacket.pz.startTime;
            newSaveFile.zoneFile.parkingMeterWarning = systemPacket.pz.warningTime;
            newSaveFile.zoneFile.StartTimePacket = systemPacket.pzStartTP;
            newSaveFile.zoneFile.StopTimePacket = systemPacket.pzStopTP;
            newSaveFile.zoneFile.WarningTimePacket = systemPacket.pzWarnTP;

            newUserFile.SaveUserFile(newSaveFile,tbSaveFileName.Text);
            

            return true;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        


    }
}