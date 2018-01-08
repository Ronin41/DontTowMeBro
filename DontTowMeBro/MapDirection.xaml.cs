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
using DontTowMeBro.Helpers;

namespace DontTowMeBro
{
    public partial class MapDirection : PhoneApplicationPage
    {
        //FindMyCarViewModel view = new FindMyCarViewModel();
        FindMyCarFile file = new FindMyCarFile();
        LoadState loadState = new LoadState();

        public MapDirection()
        {
            InitializeComponent();

            //Load List from file replace NavigationService
            try
            {
                file = (FindMyCarFile)loadState.LoadNewState(StateType.FINDMYCAR);
                

                //var myParameter = NavigationService.GetLastNavigationData();
                //view = (FindMyCarViewModel)myParameter;

                if (file.file != null)
                    lbDirections.ItemsSource = file.file.dirList;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error, MapDirection: " + e.Message, "Error", MessageBoxButton.OK);
            }

        }

        
    }
}