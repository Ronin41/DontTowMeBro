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
using System.Runtime.Serialization;
using DontTowMeBro.Helpers;

namespace DontTowMeBro
{
    
    public partial class PZStopPage : PhoneApplicationPage
    {
        struct selectedItem
        {
            public int items;
        }

        private pzStopTimePacket t = new pzStopTimePacket();
        private SystemPacket packet = new SystemPacket();
        private int hour, min, sec;
        

        private PZTimeView pzTimeView = new PZTimeView();

        private enum Switch { SELECTED, NOT_SELECTED }

        private SettingsFile newSettingsState = new SettingsFile();
        private SaveState saveState = new SaveState();
        private LoadState loadState = new LoadState();

        private List<selectedItem> listItemDataH = new List<selectedItem>();
        private List<selectedItem> listItemDataM = new List<selectedItem>();
        private List<selectedItem> listItemDataS = new List<selectedItem>();

        public PZStopPage()
        {
            InitializeComponent();

            pzLBStopH.ItemsSource = pzTimeView.stopTimeHList;
            pzLBStopM.ItemsSource = pzTimeView.stopTimeMList;
            pzLBStopS.ItemsSource = pzTimeView.stopTimeSList;

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

                packet.pzStopTP = t;
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

            newSettingsState.StopTimePacket = t;
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
        private void pzLBStopH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Grab  Items
            List<UserControl> listItems = new List<UserControl>();
            Switch s = Switch.SELECTED;


            try
            {
                GetItemsRecursive<UserControl>(pzLBStopH, ref listItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetItemsRecursive Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }




            try
            {
                if (listItemDataM.Count > 0)
                {
                    foreach (selectedItem item in listItemDataM)
                    {
                        if (e.AddedItems[0].Equals(item.items.ToString()))
                        {
                            s = Switch.NOT_SELECTED;
                            listItemDataM.Remove(item);
                            break;
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
                    listItemDataM.Clear();
                    listItemDataM.Add(item);
                    hour = item.items;
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
        private void pzLBStopM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Grab  Items
            List<UserControl> listItems = new List<UserControl>();
            Switch s = Switch.SELECTED;


            try
            {
                GetItemsRecursive<UserControl>(pzLBStopM, ref listItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetItemsRecursive Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }




            try
            {
                if (listItemDataM.Count > 0)
                {
                    foreach (selectedItem item in listItemDataM)
                    {
                        if (e.AddedItems[0].Equals(item.items.ToString()))
                        {
                            s = Switch.NOT_SELECTED;
                            listItemDataM.Remove(item);
                            break;
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
                    listItemDataM.Clear();
                    listItemDataM.Add(item);
                    min = item.items;
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
        private void pzLBStopS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Grab  Items
            List<UserControl> listItems = new List<UserControl>();
            Switch s = Switch.SELECTED;


            try
            {
                GetItemsRecursive<UserControl>(pzLBStopS, ref listItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetItemsRecursive Error; PZStartPage.xaml.cs", MessageBoxButton.OK);
            }




            try
            {
                if (listItemDataM.Count > 0)
                {
                    foreach (selectedItem item in listItemDataM)
                    {
                        if (e.AddedItems[0].Equals(item.items.ToString()))
                        {
                            s = Switch.NOT_SELECTED;
                            listItemDataM.Remove(item);
                            break;
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
                    listItemDataM.Clear();
                    listItemDataM.Add(item);
                    sec = item.items;
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