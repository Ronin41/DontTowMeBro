//Author: Michael Stewart
//Date:1/6/2014
//-------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using DontTowMeBro.ViewModels;
using System.Windows;
using System.Device.Location;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Maps.Controls;
using System.Threading;


namespace DontTowMeBro.Helpers
{
    [DataContract]
    public struct GpsLoc
    {
        [DataMember]
        private double dLatitude;
        [DataMember]
        private double dLongitude;
        //private bool reset;

        //public bool bReset
        //{
        //get{return reset;}
        //set{reset = value;}
        //}

        public void SetLatitude(double latitude)
        {
            dLatitude = latitude;
        }

        public void SetLongitude(double longitude)
        {
            dLongitude = longitude;
        }

        public double GetLongitude() { return dLongitude; }
        public double GetLatitude() { return dLatitude; }


        public GpsLoc(double Latitude, double Longitude)
        {
            dLatitude = Latitude;
            dLongitude = Longitude;
            //reset = false;
        }

        //public void Reset() 
        //{
        //    dLatitude = 0;
        //    dLongitude = 0;
        //    reset = false;
        //}
        
    }

    /// <summary>
    /// GPS Location Store
    /// </summary>
    public class GeoInfo
    {
        private GpsLoc geoLocation;
        private Geolocator _userLoc;

        public Geolocator userLoc
        {
            get { return _userLoc; }
            set { _userLoc = value; }
        }

        
  
        public GeoInfo()
        {
            geoLocation = new GpsLoc(0.0D, 0.0D);

        }

        /// <summary>
        /// Gets the current location through the device
        /// </summary>
        public async Task<bool> GetPos()
        {
                Geolocator geolocator = new Geolocator();
                geolocator.DesiredAccuracyInMeters = 50;

                Geoposition position = await geolocator.GetGeopositionAsync
                    (TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30));
                

                geoLocation.SetLatitude(position.Coordinate.Latitude);
                geoLocation.SetLongitude(position.Coordinate.Longitude);

                return true;
                
        }

        /// <summary>
        /// return Location
        /// </summary>
        /// <returns></returns>
        public GpsLoc GetGeolocation()
        {
            return geoLocation;
        }


        /// <summary>
        /// Manually set the location
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetGeoloction(double latitude, double longitude)
        {
            geoLocation.SetLatitude(latitude);
            geoLocation.SetLongitude(longitude);
        }

        public void UserLocation()
        {
            if (userLoc == null)
            {
                userLoc = new Geolocator();
                userLoc.DesiredAccuracy = PositionAccuracy.High;
                userLoc.MovementThreshold = 2; //~8ft
            }
        }
    }//end



    public class FindRoute
    {

        //private FindMyCarViewModel findMyCarList = new FindMyCarViewModel();
        //private List<GeoCoordinate> coordList = new List<GeoCoordinate>();
        //private GeocodeQuery codeQuery = new GeocodeQuery();
        //private RouteQuery routeQuery = new RouteQuery();

        private GeoCoordinate _carCoord = new GeoCoordinate();

        public GeoCoordinate carCoord
        {
            get { return _carCoord; }
            set { _carCoord = value;}
        }

       

    }//end
}
