//Date: 1/3/14
//Author: M.Stewart                  //
//Class: ParkingMeterNotification    //
//-----------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontTowMeBro.Support
{
    class ParkingMeterNotification : BaseNotification
    {
       public ParkingMeterNotification()
        {
            this.bActive = false;
            this.date = DateTime.Now;
            this.dGpsLocation = 0.0d;
           //Reset to new start stop time
            this.start = DateTime.Now;
            this.end = DateTime.Now;

        }

       public override void ParkingNotification()
       {
           base.ParkingNotification();
       }
    }
}
