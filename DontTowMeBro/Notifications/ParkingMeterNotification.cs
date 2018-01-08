//Date: 1/3/14
//Author: M.Stewart                  
//Class: ParkingMeterNotification    
//Modified: 1/6/14
//Notes:
//Add end time: used to stop meter operation
//---------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DontTowMeBro.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Phone.Scheduler;

namespace DontTowMeBro.Notifications
{
    public class ParkingMeterNotification : BaseNotification
    {
        //Members
        private DateTime MeterReturnTime { get; set; }
        private DateTime MeterWarningTime { get; set; }
        private DateTime newMeterStartTime { get; set; }
        private DateTime AlarmWarnTime { get; set; }
        private DateTime AlarmReturnTime { get; set; }

        private Reminder reminderB;
        private Reminder reminderA;

        public enum AlarmType { WARNING, TIMES_UP }


        public DateTime Return
        {
            get { return MeterReturnTime; }
            set { MeterReturnTime = value; }
        }

        public DateTime Warning
        {
            get { return MeterWarningTime; }
            set { MeterWarningTime = value; }
        }

        public DateTime Start
        {
            get { return newMeterStartTime; }
            set { newMeterStartTime = value; }
        }

       //Setup warning and meter arrays 
        public ObservableCollection<int> WarningTimeCollection= new ObservableCollection<int>();
        public ObservableCollection<int> MeterTimeCollection = new ObservableCollection<int>();

        public bool bActive
        {
            get { return this.active;}
            set { this.active = value; }
        }

        public GpsLoc loc
        {
            get { return geoLocation; }
            set { geoLocation = value; }
        }

        //Constructor
       public ParkingMeterNotification()
        {
            this.notifyDate = new Date();
            MeterReturnTime = new DateTime();
            MeterWarningTime = new DateTime();
            newMeterStartTime = new DateTime();
            
            AddMeterTimes();
            AddWarningTimes();
        }

        //Methods


       

       /// <summary>
       /// Sets the Windows Phone Scheduled Alarm
       /// May want to refactor and seperate add remove functions
       /// </summary>
       /// <param name="alarmType">(WARNING, TIMES_UP)</param>
       /// <param name="active">true adds timer / false any active timers are removed</param>
       public void AddRemoveAlarm(AlarmType alarmType, bool active)//add two string values to member fields  (Warning, TimesUp) add switch statement to body and try catch for errors
       {
           AlarmWarnTime = MeterWarningTime;
           AlarmReturnTime = MeterReturnTime;

            DateTime alarmWarnTime = AlarmWarnTime;
            DateTime alarmReturnTime = AlarmReturnTime;
           bool TimeCorrect = false;
          

           try
           {
               //Reminder a = new Reminder("a");
               //Reminder b = new Reminder("b");

               if (DateTime.Now < alarmWarnTime.AddSeconds(30))
               {
                   reminderA = new Reminder("PMWarnReminder")
               {
                   BeginTime = MeterWarningTime,
                   ExpirationTime = AlarmWarnTime.AddSeconds(30),
                   Content = "Parking Meter Warning",
                   RecurrenceType = RecurrenceInterval.None,
                   Title = "Parking Meter Reminder"
               };
                   TimeCorrect = true;
               }




               if (DateTime.Now < alarmReturnTime.AddSeconds(30))
               {
                   reminderB = new Reminder("PMReturnReminder") //change name to program name
                   {
                       BeginTime = MeterReturnTime,
                       ExpirationTime = AlarmReturnTime.AddSeconds(30),
                       Content = "Meter Time Has Run Out",
                       RecurrenceType = RecurrenceInterval.None,
                       Title = "Parking Meter Reminder"
                   };
                    TimeCorrect = true;
               }

               if (active && TimeCorrect)
               {
                   switch (alarmType)
                   {
                       case AlarmType.WARNING:
                           {
                               
                               if (ScheduledActionService.Find(reminderA.Name) != null)
                               {
                                   ScheduledActionService.Remove(reminderA.Name);
                               }

                               ScheduledActionService.Add(reminderA);
                           } break;
                       case AlarmType.TIMES_UP:
                           {
                               

                               if (ScheduledActionService.Find(reminderB.Name) != null)
                               {
                                   ScheduledActionService.Remove(reminderB.Name);
                               }

                               ScheduledActionService.Add(reminderB);
                           } break;
                       default:
                           {
                           } break;

                   }//end switch
               }//end if
               else
               {
                   switch (alarmType)
                   {
                       case AlarmType.WARNING:
                           {
                               if (reminderA != null)
                               {
                                   if (ScheduledActionService.Find(reminderA.Name) != null)
                                   {
                                       ScheduledActionService.Remove(reminderA.Name);
                                       reminderA = null;
                                   }
                               }
                           }break;
                       case AlarmType.TIMES_UP:
                           {
                               if (reminderB != null)
                               {
                                   if (ScheduledActionService.Find(reminderB.Name) != null)
                                   {
                                       ScheduledActionService.Remove(reminderB.Name);
                                       reminderB = null;
                                   }
                               }

                           }break;
                       default:
                           {
                           }break;

                   }//end switch

               }
               
       }//end try
           catch (Exception ex)
           {
               MessageBox.Show(ex.Message, "Alarm Error", MessageBoxButton.OK);
           }


       }



        /// <summary>
        /// MeterTime is added to current time to give parking time end. ex 5mins added to 12:00 (start) is 12:05 (stop) <-- meter time
        /// Return Time
        /// </summary>
        /// <param name="dMins">sets amount of meter time</param>
       public void SetNewMeterTime(double dMins)
       {
           if (dMins != 0)
           {
               try
               {
                   MeterReturnTime = this.notifyDate.GetTimeChange(dMins);
               }
               catch (Exception ex)
               {
                   MessageBox.Show(ex.Message, "Error Line 53, ParkingMeterrNotification", MessageBoxButton.OK);
               }
           }
           else
           {
               MessageBox.Show("Meter Time Not Selected", "Error", MessageBoxButton.OK);
           }
       }

        /// <summary>
        /// Warning takes current time, adds dMins to get new warning time
        /// </summary>
        /// <param name="dMins">sets how long before warning</param>
        public void SetNewMeterWarningTime(double dMins)
        {
           if (dMins != 0)
           { 
            try
            {
                MeterWarningTime = this.notifyDate.GetTimeChange(dMins);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Line 69, ParkingMeterrNotification", MessageBoxButton.OK);
            }
               }
           else
           {
               MessageBox.Show("Warning Time Not Selected", "Error", MessageBoxButton.OK);
           }
        }

        /// <summary>
        /// Gets Start Time
        /// </summary>
        /// <returns></returns>
       public bool MeterStartTime()
       {
           try
           {
               newMeterStartTime = this.notifyDate.GetBaseTime();
               return true;
           }
           catch (Exception ex)
           {
               MessageBox.Show(ex.Message, "Error Line 85, ParkingMeterrNotification", MessageBoxButton.OK);
           }
           return false;
       }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
       public string RemainingMeterTime()
       {  
           TimeSpan duration = MeterReturnTime - DateTime.Now;
           string sReturn = "00:" + duration.Minutes.ToString() + ":" + duration.Seconds.ToString();
           return sReturn;
       }

        /// <summary>
        /// Tracks the warning time
        /// </summary>
        /// <returns>true if warning goes off, false if safe</returns>
       public bool MeterWarning()
       {
           try
           {
               if (this.notifyDate.GetBaseTime().CompareTo(MeterWarningTime) == 0 || this.notifyDate.GetBaseTime().CompareTo(MeterWarningTime) > 0)
               {
                   return true;
               }
           }
           catch (Exception ex)
           {
               MessageBox.Show(ex.Message, "Error Line 102, ParkingMeterrNotification", MessageBoxButton.OK);
           }

           return false;
       }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
       public bool MeterEndTime()
    {
        if (this.notifyDate.GetBaseTime().CompareTo(MeterReturnTime) >= 0)
        {
            return true;
        }

        return false;
    }

        /// <summary>
        /// 
        /// </summary>
       private void AddWarningTimes()
       {
           WarningTimeCollection.Add(1);
           WarningTimeCollection.Add(5);
           WarningTimeCollection.Add(10);
           WarningTimeCollection.Add(15);

       }

        /// <summary>
        /// 
        /// </summary>
       private void AddMeterTimes()
       {
           MeterTimeCollection.Add(5);
           MeterTimeCollection.Add(10);
           MeterTimeCollection.Add(15);
           MeterTimeCollection.Add(20);
           MeterTimeCollection.Add(25);
           MeterTimeCollection.Add(30);
           MeterTimeCollection.Add(35);
           MeterTimeCollection.Add(40);
           MeterTimeCollection.Add(45);
           MeterTimeCollection.Add(50);
           MeterTimeCollection.Add(55);
           MeterTimeCollection.Add(60);
           MeterTimeCollection.Add(65);
           MeterTimeCollection.Add(70);
           MeterTimeCollection.Add(75);
           MeterTimeCollection.Add(80);
           MeterTimeCollection.Add(85);
           MeterTimeCollection.Add(90);
       }

        /// <summary>
        /// Activates Parking Meter Notification
        /// </summary>
       public override void StartNotification()
       {
           bActive = true;
           
       }

        /// <summary>
        /// Future class reset method
        /// </summary>
       public void Reset()
       {
       }
    }
}
