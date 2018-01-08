using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontTowMeBro.ViewModels
{
    class PMViewModel
    {
        private List<string> _meterTime;
        private List<string> _warningTime;

        public List<string> meterTime
        {
            set { _meterTime = value; }
            get{return _meterTime;}
         }

        public List<string> warningTime
        {
            set { _warningTime = value; }
            get { return _warningTime; }
        }

        public PMViewModel()
        {
            meterTime = new List<string>();
            warningTime = new List<string>();

            PopulateMeterTime(13);
            PopulateWarningTime();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mul">number of times to multiply by 5</param>
        private void PopulateMeterTime(int mul)
        {
            int interval = 5;

            for (int i = 1; i <= mul; i++)
            {
                meterTime.Add(Convert.ToString(interval));
                interval += 5;
            }
        }

        private void PopulateWarningTime()
        {
            for (int i = 1; i <= 15; i++)
                warningTime.Add(i.ToString());
        }

    }
}
