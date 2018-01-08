using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontTowMeBro.Support
{
    public class BaseNotification
    {
       public DateTime date, start, end;
       public double dGpsLocation;
       public int iTimeLimit; //used for ParkingSpotNotification
       public  enum ALERT_TIME{AlertA=15,AlertB=30,AlertC=60}
       public bool bActive;



       public virtual void ParkingNotification();

    }
}
