using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DontTowMeBro.ViewModels;
using System.Windows.Media;
using DontTowMeBro.Helpers;
using System.Runtime.Serialization;

namespace DontTowMeBro
{
    public partial class PZWarnPage : PhoneApplicationPage
    {
        struct selectedItem
        {
            public int items;
        }

        private PZTimeView pzTimeView = new PZTimeView();
        private pzWarningTimePacket t = new pzWarningTimePacket();
        private SystemPacket packet = new SystemPacket();

        private int hour, min, sec;
        

        private SettingsFile newSettingsState = new SettingsFile();
        private SaveState saveState = new SaveState();
        private LoadState loadState = new LoadState();

        private enum Switch { SELECTED, NOT_SELECTED }


        private List<selectedItem> listItemDataH = new List<selectedItem>();
        private List<selectedItem> listItemDataM = new List<selectedItem>();
        private List<selectedItem> listItemDataS = new List<selectedItem>();

        public PZWarnPage()
        {
            InitializeComponent();

            pzLBWarnH.ItemsSource = pzTimeView.warnTimeHList;
            pzLBWarnM.ItemsSource = pzTimeView.warnTimeMList;
            pzLBWarnS.ItemsSource = pzTimeView.warnTimeSList;

            hour = min = sec = -1;

            LoadSettings();
        }
        
       

        private void btnPZDone_Click(object sender, RoutedEventArgs e)
        {
            if (hour != -1 && min != -1 && sec != -1)
            {
                t.hour = hour;
                t.minute = min;
                t.second = sec;
                t.active = true;
                t.pm = (bool)cbPM.IsChecked;

                packet.pzWarnTP = t;
                SaveSettings();
            }

            Uri uri = new Uri("/MainPage.xaml?goto=2", UriKind.Relative);
            NavigationService.Navigate(uri.ToString(), packet);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveSettings()
        {
            newSettingsState.WarningTimePacket = t;
            saveState.SaveSystemState(StateType.SETTINGS, newSettingsState);
        }

        /// <summary>
        /// Loads all app settings
        /// </summary>
        private void LoadSettings()
        {
            //ToDo: set parking zone current light settings
            newSettingsState = (SettingsFile)loadState.LoadNewState(StateType.SETTINGS);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pzLBWarnH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Grab  Items
            List<UserControl> listItems = new List<UserControl>();
            Switch s = Switch.NOT_SELECTED;


            try
            {
                GetItemsRecursive<UserControl>(pzLBWarnH, ref listItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetItemsRecursive Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }




            try
            {
                if (listItemDataH.Count > 0)
                {
                    foreach (selectedItem item in listItemDataH)
                    {
                        if (e.AddedItems[0].Equals(item.items.ToString()))
                        {
                            s = Switch.NOT_SELECTED;
                            listItemDataH.Remove(item);
                            break;
                        }
                        else
                        {
                            s = Switch.SELECTED;
                        }
                    }//end foreach
                }//end if
                else
                {
                    s = Switch.SELECTED;
                }//end else

                if (s == Switch.SELECTED)
                {
                    selectedItem item = new selectedItem();
                    switch (e.AddedItems[0].ToString())
                    {
                        case "01":
                            {
                                item.items = 1;
                            } break;
                        case "02":
                            {
                                item.items = 2;
                            } break;
                        case "03":
                            {
                                item.items = 3;
                            } break;
                        case "04":
                            {
                                item.items = 4;
                            } break;
                        case "05":
                            {
                                item.items = 5;
                            } break;
                        case "06":
                            {
                                item.items = 6;
                            } break;
                        case "07":
                            {
                                item.items = 7;
                            } break;
                        case "08":
                            {
                                item.items = 8;
                            } break;
                        case "09":
                            {
                                item.items = 9;
                            } break;
                        case "10":
                            {
                                item.items = 10;
                            } break;
                        case "11":
                            {
                                item.items = 11;
                            } break;
                        case "12":
                            {
                                item.items = 12;
                            } break;

                    }
                    //item.items = Convert.ToInt16(e.AddedItems[0].ToString());
                    listItemDataH.Clear();
                    listItemDataH.Add(item);

                    hour = item.items; // set hour for time packets
                }//end if


                ///Switch statement for control the selection highlight state
                switch (s)
                {
                    case Switch.SELECTED:
                        {
                            if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
                            {
                                foreach (UserControl userControl in listItems)
                                {
                                    VisualStateManager.GoToState(userControl, "Normal", true);
                                }//end foreach

                                foreach (UserControl userControl in listItems)
                                {
                                    if (e.AddedItems[0].Equals(userControl.DataContext))
                                        VisualStateManager.GoToState(userControl, "Selected", true);
                                }
                            }//end if
                        } break;
                    case Switch.NOT_SELECTED:
                        {
                            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] != null)
                            {
                                foreach (UserControl userControl in listItems)
                                {
                                    if (e.RemovedItems[0].Equals(userControl.DataContext))
                                    {
                                        VisualStateManager.GoToState(userControl, "Normal", true);
                                    }//end if
                                }//end foreach
                            }//end if
                        } break;
                }//end switch

            }//end try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }

        }//end

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pzLBWarnM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Grab  Items
            List<UserControl> listItems = new List<UserControl>();
            Switch s = Switch.NOT_SELECTED;


            try
            {
                GetItemsRecursive<UserControl>(pzLBWarnM, ref listItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetItemsRecursive Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }




            try
            {
                if (listItemDataH.Count > 0)
                {
                    foreach (selectedItem item in listItemDataH)
                    {
                        if (e.AddedItems[0].Equals(item.items.ToString()))
                        {
                            s = Switch.NOT_SELECTED;
                            listItemDataH.Remove(item);
                            break;
                        }
                        else
                        {
                            s = Switch.SELECTED;
                        }
                    }//end foreach
                }//end if
                else
                {
                    s = Switch.SELECTED;
                }//end else

                if (s == Switch.SELECTED)
                {
                    selectedItem item = new selectedItem();
                    item.items = Convert.ToInt16(e.AddedItems[0].ToString());
                    listItemDataH.Clear();
                    listItemDataH.Add(item);

                    min = item.items; // set min for time packets
                }//end if


                ///Switch statement for control the selection highlight state
                switch (s)
                {
                    case Switch.SELECTED:
                        {
                            if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
                            {
                                foreach (UserControl userControl in listItems)
                                {
                                    VisualStateManager.GoToState(userControl, "Normal", true);
                                }//end foreach

                                foreach (UserControl userControl in listItems)
                                {
                                    if (e.AddedItems[0].Equals(userControl.DataContext))
                                        VisualStateManager.GoToState(userControl, "Selected", true);
                                }
                            }//end if
                        } break;
                    case Switch.NOT_SELECTED:
                        {
                            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] != null)
                            {
                                foreach (UserControl userControl in listItems)
                                {
                                    if (e.RemovedItems[0].Equals(userControl.DataContext))
                                    {
                                        VisualStateManager.GoToState(userControl, "Normal", true);
                                    }//end if
                                }//end foreach
                            }//end if
                        } break;
                }//end switch

            }//end try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }

        }//end

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pzLBWarnS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Grab  Items
            List<UserControl> listItems = new List<UserControl>();
            Switch s = Switch.NOT_SELECTED;


            try
            {
                GetItemsRecursive<UserControl>(pzLBWarnS, ref listItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetItemsRecursive Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }




            try
            {
                if (listItemDataH.Count > 0)
                {
                    foreach (selectedItem item in listItemDataH)
                    {
                        if (e.AddedItems[0].Equals(item.items.ToString()))
                        {
                            s = Switch.NOT_SELECTED;
                            listItemDataH.Remove(item);
                            break;
                        }
                        else
                        {
                            s = Switch.SELECTED;
                        }
                    }//end foreach
                }//end if
                else
                {
                    s = Switch.SELECTED;
                }//end else

                if (s == Switch.SELECTED)
                {
                    selectedItem item = new selectedItem();
                    item.items = Convert.ToInt16(e.AddedItems[0].ToString());
                    listItemDataH.Clear();
                    listItemDataH.Add(item);

                    sec = item.items; // set sec for time packets
                }//end if


                ///Switch statement for control the selection highlight state
                switch (s)
                {
                    case Switch.SELECTED:
                        {
                            if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
                            {
                                foreach (UserControl userControl in listItems)
                                {
                                    VisualStateManager.GoToState(userControl, "Normal", true);
                                }//end foreach

                                foreach (UserControl userControl in listItems)
                                {
                                    if (e.AddedItems[0].Equals(userControl.DataContext))
                                        VisualStateManager.GoToState(userControl, "Selected", true);
                                }
                            }//end if
                        } break;
                    case Switch.NOT_SELECTED:
                        {
                            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] != null)
                            {
                                foreach (UserControl userControl in listItems)
                                {
                                    if (e.RemovedItems[0].Equals(userControl.DataContext))
                                    {
                                        VisualStateManager.GoToState(userControl, "Normal", true);
                                    }//end if
                                }//end foreach
                            }//end if
                        } break;
                }//end switch

            }//end try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }

        }//end


        /// <summary>
        /// Recursive get the item.
        /// </summary>
        /// <typeparam name="T">The item to get.</typeparam>
        /// <param name="parents">Parent container.</param>
        /// <param name="objectList">Item list</param>
        public static void GetItemsRecursive<T>(DependencyObject parents, ref List<T> objectList) where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(parents);


            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parents, i);


                if (child is T)
                {
                    objectList.Add(child as T);
                }


                GetItemsRecursive<T>(child, ref objectList);
            }


            return;
        }
    }
}