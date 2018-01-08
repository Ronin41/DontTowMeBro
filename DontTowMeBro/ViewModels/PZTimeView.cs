using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontTowMeBro.ViewModels
{
    class PZTimeView
    {

        private List<string> _startTimeHList;
        private List<string> _stopTimeHList;
        private List<string> _warnTimeHList;

        private List<string> _startTimeMList;
        private List<string> _stopTimeMList;
        private List<string> _warnTimeMList;

        private List<string> _startTimeSList;
        private List<string> _stopTimeSList;
        private List<string> _warnTimeSList;

            public List<string> startTimeHList
        {
            set { _startTimeHList = value; }
            get { return _startTimeHList; }
        }

        public List<string> stopTimeHList
        {
            set { _stopTimeHList = value; }
            get { return _stopTimeHList; }
        }

        public List<string> warnTimeHList
        {
            set { _warnTimeHList = value; }
            get { return _warnTimeHList; }
        }

        public List<string> startTimeMList
        {
            set { _startTimeMList = value; }
            get { return _startTimeMList; }
        }

        public List<string> stopTimeMList
        {
            set { _stopTimeMList = value; }
            get { return _stopTimeMList; }
        }

        public List<string> warnTimeMList
        {
            set { _warnTimeMList = value; }
            get { return _warnTimeMList; }
        }

        public List<string> startTimeSList
        {
            set { _startTimeSList = value; }
            get { return _startTimeSList; }
        }

        public List<string> stopTimeSList
        {
            set { _stopTimeSList = value; }
            get { return _stopTimeSList; }
        }

        public List<string> warnTimeSList
        {
            set { _warnTimeSList = value; }
            get { return _warnTimeSList; }
        }

        public PZTimeView()
        {
            startTimeHList = new List<string>();
            startTimeMList = new List<string>();
            startTimeSList = new List<string>();

            stopTimeHList = new List<string>();
            stopTimeMList = new List<string>();
            stopTimeSList = new List<string>();

            warnTimeHList = new List<string>();
            warnTimeMList = new List<string>();
            warnTimeSList = new List<string>();


            addStartTime();
            addStopTime();
            addWarnTime();
        }

        /// <summary>
        /// 
        /// </summary>
        private void addStartTime()
        {
            for (int i = 0; i < 60; i++ )
            {

                switch (i)
                {
                    case 0:
                        {
                            startTimeSList.Add("00");
                            startTimeMList.Add("00");
                        } break;
                    case 1:
                        {
                            startTimeSList.Add("01");
                            startTimeMList.Add("01");
                        } break;
                    case 2:
                        {
                            startTimeSList.Add("02");
                            startTimeMList.Add("02");
                        } break;
                    case 3:
                        {
                            startTimeSList.Add("03");
                            startTimeMList.Add("03");
                        } break;
                    case 4:
                        {
                            startTimeSList.Add("04");
                            startTimeMList.Add("04");
                        } break;
                    case 5:
                        {
                            startTimeSList.Add("05");
                            startTimeMList.Add("05");
                        } break;
                    case 6:
                        {
                            startTimeSList.Add("06");
                            startTimeMList.Add("06");
                        } break;
                    case 7:
                        {
                            startTimeSList.Add("07");
                            startTimeMList.Add("07");
                        } break;
                    case 8:
                        {
                            startTimeSList.Add("08");
                            startTimeMList.Add("08");
                        } break;
                    case 9:
                        {
                            startTimeSList.Add("09");
                            startTimeMList.Add("09");
                        } break;
                }

                if (i >= 10)
                {
                    startTimeSList.Add(i.ToString());
                    startTimeMList.Add(i.ToString());
                }
            }

            for (int i = 1; i <= 12; i++)
            {
                switch (i)
                {

                    case 1:
                        {
                            startTimeHList.Add("01");
                        }break;
                    case 2:
                        {
                            startTimeHList.Add("02");
                        } break;
                    case 3:
                        {
                            startTimeHList.Add("03");
                        } break;
                    case 4:
                        {
                            startTimeHList.Add("04");
                        } break;
                    case 5:
                        {
                            startTimeHList.Add("05");
                        } break;
                    case 6:
                        {
                            startTimeHList.Add("06");
                        } break;
                    case 7:
                        {
                            startTimeHList.Add("07");
                        } break;
                    case 8:
                        {
                            startTimeHList.Add("08");
                        } break;
                    case 9:
                        {
                            startTimeHList.Add("09");
                        } break;
                }

                if( i >= 10)
                startTimeHList.Add(i.ToString());
            }

        }//end start

        /// <summary>
        /// 
        /// </summary>
        private void addStopTime()
        {
            for (int i = 0; i < 60; i++)
            {

                switch (i)
                {
                    case 0:
                        {
                            stopTimeSList.Add("00");
                            stopTimeMList.Add("00");
                        } break;
                    case 1:
                        {
                            stopTimeSList.Add("01");
                            stopTimeMList.Add("01");
                        } break;
                    case 2:
                        {
                            stopTimeSList.Add("02");
                            stopTimeMList.Add("02");
                        } break;
                    case 3:
                        {
                            stopTimeSList.Add("03");
                            stopTimeMList.Add("03");
                        } break;
                    case 4:
                        {
                            stopTimeSList.Add("04");
                            stopTimeMList.Add("04");
                        } break;
                    case 5:
                        {
                            stopTimeSList.Add("05");
                            stopTimeMList.Add("05");
                        } break;
                    case 6:
                        {
                            stopTimeSList.Add("06");
                            stopTimeMList.Add("06");
                        } break;
                    case 7:
                        {
                            stopTimeSList.Add("07");
                            stopTimeMList.Add("07");
                        } break;
                    case 8:
                        {
                            stopTimeSList.Add("08");
                            stopTimeMList.Add("08");
                        } break;
                    case 9:
                        {
                            stopTimeSList.Add("09");
                            stopTimeMList.Add("09");
                        } break;
                }

                if (i >= 10)
                {
                    stopTimeSList.Add(i.ToString());
                    stopTimeMList.Add(i.ToString());
                }
            }

             for (int i = 1; i <= 12; i++)
            {
                switch (i)
                {
                    case 1:
                        {
                            stopTimeHList.Add("01");
                        }break;
                    case 2:
                        {
                            stopTimeHList.Add("02");
                        } break;
                    case 3:
                        {
                            stopTimeHList.Add("03");
                        } break;
                    case 4:
                        {
                            stopTimeHList.Add("04");
                        } break;
                    case 5:
                        {
                            stopTimeHList.Add("05");
                        } break;
                    case 6:
                        {
                            stopTimeHList.Add("06");
                        } break;
                    case 7:
                        {
                            stopTimeHList.Add("07");
                        } break;
                    case 8:
                        {
                            stopTimeHList.Add("08");
                        } break;
                    case 9:
                        {
                            stopTimeHList.Add("09");
                        } break;
                }

                if( i >= 10)
                stopTimeHList.Add(i.ToString());
             }
           
        }//end stop

        /// <summary>
        /// 
        /// </summary>
        private void addWarnTime()
        {

            for (int i = 0; i < 60; i++)
            {

                switch (i)
                {

                    case 0:
                        {
                            warnTimeSList.Add("00");
                            warnTimeMList.Add("00");
                        }break;
                    case 1:
                        {
                            warnTimeSList.Add("01");
                            warnTimeMList.Add("01");
                        } break;
                    case 2:
                        {
                            warnTimeSList.Add("02");
                            warnTimeMList.Add("02");
                        } break;
                    case 3:
                        {
                            warnTimeSList.Add("03");
                            warnTimeMList.Add("03");
                        } break;
                    case 4:
                        {
                            warnTimeSList.Add("04");
                            warnTimeMList.Add("04");
                        } break;
                    case 5:
                        {
                            warnTimeSList.Add("05");
                            warnTimeMList.Add("05");
                        } break;
                    case 6:
                        {
                            warnTimeSList.Add("06");
                            warnTimeMList.Add("06");
                        } break;
                    case 7:
                        {
                            warnTimeSList.Add("07");
                            warnTimeMList.Add("07");
                        } break;
                    case 8:
                        {
                            warnTimeSList.Add("08");
                            warnTimeMList.Add("08");
                        } break;
                    case 9:
                        {
                            warnTimeSList.Add("09");
                            warnTimeMList.Add("09");
                        } break;
                }

                if (i >= 10)
                {
                    warnTimeSList.Add(i.ToString());
                    warnTimeMList.Add(i.ToString());
                }
            }
            for (int i = 1; i <= 12; i++)
            {
                switch (i)
                {
                    case 1:
                        {
                            warnTimeHList.Add("01");
                        } break;
                    case 2:
                        {
                            warnTimeHList.Add("02");
                        } break;
                    case 3:
                        {
                            warnTimeHList.Add("03");
                        } break;
                    case 4:
                        {
                            warnTimeHList.Add("04");
                        } break;
                    case 5:
                        {
                            warnTimeHList.Add("05");
                        } break;
                    case 6:
                        {
                            warnTimeHList.Add("06");
                        } break;
                    case 7:
                        {
                            warnTimeHList.Add("07");
                        } break;
                    case 8:
                        {
                            warnTimeHList.Add("08");
                        } break;
                    case 9:
                        {
                            warnTimeHList.Add("09");
                        } break;
                }

                if (i >= 10)
                    warnTimeHList.Add(i.ToString());
            }

        }//end warn
    }
}
