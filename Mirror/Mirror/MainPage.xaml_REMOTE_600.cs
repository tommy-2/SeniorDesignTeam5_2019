#region Using Statement(s)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetroLog;
using Microsoft.ProjectOxford.Emotion;
using Mirror.Controls;
using Mirror.Core;
using Mirror.Extensions;
//using Mirror.Interfaces;
//using Mirror.IO;
using Mirror.Logging;
using Mirror.Models;
using Mirror.Threading;
using Mirror.ViewModels;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;
using static Mirror.Core.Settings;
using RawEmotion = Microsoft.ProjectOxford.Emotion.Contract.Emotion;
using Windows.UI.Xaml.Controls.Maps;

#endregion


namespace Mirror
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Field(s)

        ILogger _logger = LoggerFactory.Get<MainPage>();

        #endregion

        //gesture variable fields
        GestureControl.GestureOutputFunctionDelegate gestureDel;
        GestureControl gc;

        public MainPage()
        {
            InitializeComponent();
            DataContext = new HudViewModel(this);

        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _messageLabel.Text = GetTimeOfDayGreeting();

            setUpMap();

            // I want these to be serialized.
            foreach (var loader in new IAsyncLoader[]
                                   {
                                       _currentWeather,
                                       _forecastWeather,
                                       _eventCalendar,
                                       _eventCalendarLarge
                                   }.Where(loader => loader != null))
            {
                await loader.LoadAsync();
            }

            //setup gesture control variable fields
            gestureDel = gestureHandler;
            gc = new GestureControl(gestureDel);
            //start gesture control
            gc.ConnectAndListen_Arduino();

        }

        private static string GetTimeOfDayGreeting()
        {
            var hour = DateTime.Now.Hour;
            return hour < 12
                ? "Good morning"
                : hour < 17
                    ? "Good afternoon"
                    : "Good evening";
        }

        async void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //none
        }

        async void setUpMap()
        {
            _trafficMap.MapServiceToken = Instance.TrafficAPIKey;

            // Set your current location.
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    // Get the current location.
                    Geolocator geolocator = new Geolocator();
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    Geopoint myLocation = pos.Coordinate.Point;

                    //K-State Campus Location
                    BasicGeoposition bgp = new BasicGeoposition();
                    bgp.Latitude = 39.1930;
                    bgp.Longitude = -96.5740;
                    Geopoint AndersonHall = new Geopoint(bgp);


                    // Set the map location.
                    _trafficMap.Center = AndersonHall;
                    _trafficMap.LandmarksVisible = true;
                    _trafficMap.Height = 450;
                    _trafficMap.Width = 350;
                    break;

                case GeolocationAccessStatus.Denied:
                    // Handle the case  if access to location is denied.
                    break;

                case GeolocationAccessStatus.Unspecified:
                    // Handle the case if  an unspecified error occurs.
                    break;
            }

            _trafficMap.ZoomLevel = 14;
            _trafficMap.StyleSheet = MapStyleSheet.RoadDark();
        }

        //this function is set up to run whenever a gesture is received from the gesture sensors
        //more specifically it runs every time a serial message is received form the arduino
        //implements a state machine that responds to input gestures
        private void gestureHandler(GestureControl.GestureType gesture)
        {
            switch (gesture)
            {
                case GestureControl.GestureType.ZX_Right:
                    _eventCarouselControl.ScrollRight();
                    break;
                case GestureControl.GestureType.ZX_Left:
                    _eventCarouselControl.ScrollLeft();
                    break;
                case GestureControl.GestureType.ZX_Up:
                    break;
                case GestureControl.GestureType.DFR_Up:
                    break;
                case GestureControl.GestureType.DFR_Down:
                    break;
                case GestureControl.GestureType.DFR_Left:
                    break;
                case GestureControl.GestureType.DFR_Right:
                    break;
                case GestureControl.GestureType.DFR_CW:
                    break;
                case GestureControl.GestureType.DFR_CCW:
                    break;
                case GestureControl.GestureType.No_Gesture:
                    break;
                default:
                    break;
            }
        }
    }
}