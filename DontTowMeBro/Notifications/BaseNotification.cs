//Author: Michael Stewart
//Date:1/6/2014
//-------------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DontTowMeBro.Helpers;


namespace DontTowMeBro.Notifications
{
        
    public class BaseNotification 
    {
       protected DateTime date, start, end;
       protected GpsLoc geoLocation;
       protected Date notifyDate;
       protected bool active { get; set; }
       protected int iWarningTime;

       
       public virtual void StartNotification(){}
       public virtual void StopNotification() {}

    }
}

