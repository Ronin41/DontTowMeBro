using Microsoft.Phone.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontTowMeBro.Helpers
{
    public class Date
    {
        private DateTime newDate { get; set; }
        private DateTime BaseDate { get; set; }
        private DateTime LimitDate { get; set; }
        private DateTime WarningDate { get; set; }

        private string sMonth, sDay, sYear;
        private string sBaseTime { get; set; }
        public string sTimeLimit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Date()
        {
            newDate = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetMonth()
        {
            sMonth = newDate.Month.ToString();
            return sMonth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetDay()
        {
            sDay = newDate.Day.ToString();
            return sDay;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetYear()
        {
            sYear = newDate.Year.ToString();
            return sYear;
        }

        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime GetBaseTime()
        {
            BaseDate = DateTime.Now;
            return BaseDate;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dMins"></param>
        /// <returns></returns>
        public DateTime GetTimeChange(double dMins)
        {
            LimitDate = DateTime.Now;
            DateTime date = LimitDate.AddMinutes(dMins);
            return date;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateA"></param>
        /// <param name="dateB"></param>
        /// <returns></returns>
        public bool isDateALaterThanDateB(DateTime dateA, DateTime dateB)
        {
            if (dateA.CompareTo(dateB) > 0)
            {
                return true;
            }

            return false;

        }

      
    }//END DATE

    

}
