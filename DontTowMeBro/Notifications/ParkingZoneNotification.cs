//Date: 2/26/14
//Author: M.Stewart                  
//Class: ParkingZoneNotification    
//Notes:
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

    struct Time
    {
       public int hour, minute, second;
    }

    public class ParkingZoneNotification : BaseNotification
    {
        //Members
        private DateTime _returnTime { get; set; }
        private DateTime _warningTime { get; set; }
        private DateTime _startTime { get; set; }
        private DateTime _alarmWarnTime { get; set; }
        private DateTime _alarmReturnTime { get; set; }

        private Time returnTimeStruct = new Time();
        private Time startTimeStruct = new Time();
        private Time warningTimeStruct = new Time();

        private Date newDate = new Date();

        private Reminder reminderA;
        private Reminder reminderB;

        private bool _firstSave { get; set; }

        

        public enum AlarmType { WARNING, TIMES_UP }

        public bool firstSave
        {
            get { return _firstSave; }
            set { _firstSave = value; }
        }

        public DateTime returnTime
        {
        get {return _returnTime;}
        set { _returnTime = value; }
        }

        public DateTime warningTime
        {
            get { return _warningTime; }
            set { _warningTime = value; }    
        }

        public DateTime startTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public DateTime alarmWarnTime
        {
            get { return _alarmWarnTime; }
            set { _alarmWarnTime = value; }
        }

        public DateTime alarmReturnTime
        {
            get { return _alarmReturnTime; }
            set { _alarmReturnTime = value; }
        }

        public bool bActive
        {
            get { return this.active; }
            set { this.active = value; }
        }

        public GpsLoc loc
        {
            get { return geoLocation; }
            set { geoLocation = value; }
        }

        public ParkingZoneNotification()
        {
            this.notifyDate = new Date();
            returnTime = new DateTime();
            warningTime = new DateTime();
            startTime = new DateTime();
        }

        public string RemainingZoneTime()
        {
            TimeSpan duration = returnTime - DateTime.Now;
            string sReturn = duration.Hours.ToString() + ":" + duration.Minutes.ToString() + ":" + duration.Seconds.ToString();

            return sReturn;
        }

        /// <summary>
        /// Sets the Windows Phone Scheduled Alarm
        /// May want to refactor and seperate add remove functions
        /// </summary>
        /// <param name="alarmType">(WARNING, TIMES_UP)</param>
        /// <param name="active">true adds timer / false any active timers are removed</param>
        public void AddRemoveAlarm(AlarmType alarmtype, bool active)
        {
            alarmWarnTime = warningTime;
            alarmReturnTime = returnTime;
            bool bTimeCorrect = false;


            try
            {
                if(DateTime.Now < alarmWarnTime)
                {
                    reminderA = new Reminder("PZWarnReminder")
                    {
                        BeginTime = warningTime,
                        ExpirationTime = alarmWarnTime,
                        Content = "Parking Zone Warning",
                        RecurrenceType = RecurrenceInterval.None,
                        Title = "ParkingZone Reminder"
                    };
                    bTimeCorrect = true;
                }//end if

                if(DateTime.Now < alarmReturnTime)
                {
                    reminderB = new Reminder("PZReturnReminder")
                    {
                        BeginTime = returnTime,
                        ExpirationTime = alarmReturnTime,
                        Content = "Parking Zone Closed",
                        RecurrenceType = RecurrenceInterval.None,
                        Title = "ParkingZone Reminder"
                    };
                    bTimeCorrect = true;
                }//end if

                if (active && bTimeCorrect)
                {
                    switch (alarmtype)
                    {
                        case AlarmType.WARNING:
                            {
                                if (ScheduledActionService.Find(reminderA.Name) != null)
                                {
                                    ScheduledActionService.Remove(reminderA.Name);
                                }//end if

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
                    switch (alarmtype)
                    {
                        case AlarmType.WARNING:
                            {
                                if (reminderA != null)
                                {
                                    if (ScheduledActionService.Find(reminderA.Name) != null)
                                    {
                                        ScheduledActionService.Remove(reminderA.Name);
                                        reminderA = null;
                                    }//end if
                                }//end if
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
                }//end else
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Alarm Error", MessageBoxButton.OK);

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the current time is later than return time</returns>
        public bool PZReturn(DateTime currentTime)
        {
            //if (currentTime.Hour >= returnTimeStruct.hour && currentTime.Minute >= returnTimeStruct.minute)
            //{
            //    return true;
            //}
            if (currentTime >= returnTime)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the current time is later than return time</returns>
        public bool PZWarning(DateTime currentTime)
        {
            //if (currentTime.Hour >= warningTimeStruct.hour && currentTime.Minute >= warningTimeStruct.minute)
            //{
            //    return true;
            //}

            if (currentTime >= warningTime)
            {
                return true;
            }

            return false;
        }



        /// <summary>
        /// Sets Warning Time
        /// </summary>
        /// <param name="dMins">Number of minutes subtracted from return time</param>
        public void SetWarning()
        {
            TimeSpan t = new TimeSpan(warningTime.Hour, warningTime.Minute, 0);
            DateTime d = new DateTime();

            try
            {
                d = returnTime;
                warningTime = d.Subtract(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error, ParkingZoneNotification, PZSetWarning", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Sets Warning Time
        /// Add AM/PM Toggle
        /// </summary>
        
        public void SetWarningTime(int hour, int minute, bool pm)
        {
            
            try
            {
                warningTimeStruct.minute = minute;
                warningTimeStruct.hour = hour;
                warningTimeStruct.second = 0;

                DateTime w = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    warningTimeStruct.hour, warningTimeStruct.minute, warningTimeStruct.second);

                if (pm == true)
                {
                    w = w.AddHours(12);
                    warningTime = w;
                }
                else
                {
                    warningTime = w;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error, ParkingZoneNotification, SetWarningTime", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Sets the return time
        /// ADD AM/PM toggle in XAML
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        public void SetReturnTime(int hour, int minute, bool pm)
        {
            try
            {
                returnTimeStruct.minute = minute;
                returnTimeStruct.hour = hour;
                returnTimeStruct.second = 0;

                DateTime r = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    returnTimeStruct.hour, returnTimeStruct.minute, returnTimeStruct.second);

                //Set switch
                //ex. if toggle is PM AddHours 12
                if (pm == true && hour != 12)
                {
                    r = r.AddHours(12);
                    returnTime = r;
                }
                else if(hour != 12)
                {                  
                    returnTime = r;
                }

                if (pm == true && hour == 12)
                {
                    returnTime = r;
                }
                else if (hour == 12)
                {
                    r = r.AddHours(12);
                    returnTime = r;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error, ParkingZoneNotification, SetReturnTime", MessageBoxButton.OK);
            }

           
        }

        /// <summary>
        /// Sets the return time
        /// add AM/PM toggle in XAML
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        public void SetStartTime(int hour, int minute, bool pm)
        {
            try
            {
                startTimeStruct.minute = minute;
                startTimeStruct.hour = hour;
                startTimeStruct.second = 0;

                DateTime s = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    (startTimeStruct.hour), startTimeStruct.minute, startTimeStruct.second);

                if (pm == true)
                {
                    s = s.AddHours(12);
                    startTime = s;
                }
                else
                {
                    startTime = s;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error, ParkingZoneNotification, SetStartTime", MessageBoxButton.OK);
            }
        }


        override public void StartNotification()
        {
            bActive = true;
        }

        public override void StopNotification()
        {
            bActive = false;
        }
        
    }//end class
}
