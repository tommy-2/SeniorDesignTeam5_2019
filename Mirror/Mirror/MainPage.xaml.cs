#region Using Statement(s)

using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetroLog;
using Microsoft.ProjectOxford.Emotion; //delete
using Mirror.Controls;
using Mirror.Core;
using Mirror.Extensions;
//using Mirror.Interfaces; //delete
//using Mirror.IO; //delete
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
using RawEmotion = Microsoft.ProjectOxford.Emotion.Contract.Emotion; //delete
using Windows.UI.Xaml.Controls.Maps;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Uwp.UI.Animations;

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

        

        public ObservableCollection<Tweet> Tweets { get; set; }
        private bool _sleepMode = false;

        //gesture variable fields
        GestureControl.GestureOutputFunctionDelegate gestureDel;
        GestureControl gc;
        DispatcherTimer _timer;
        #endregion

        public MainPage()
        {
            InitializeComponent();
            DataContext = new HudViewModel(this);
            Tweets = new ObservableCollection<Tweet>();

            gestureDel = gestureHandler;
            gc = new GestureControl(gestureDel);
            //start gesture control
            gc.ConnectAndListen_Arduino();
        }

        private void gestureHandler(GestureControl.GestureType gesture)
        {
            if (_sleepMode && gesture != GestureControl.GestureType.ZX_Up) { return; }
            switch (gesture)
            {
                case GestureControl.GestureType.ZX_Right:
                    _eventCarouselControl.ScrollLeft();
                    CenterPageDisplay((int)_eventCarouselControl.GetSelectedItem());
                    break;
                case GestureControl.GestureType.ZX_Left:
                    _eventCarouselControl.ScrollRight();
                    CenterPageDisplay((int)_eventCarouselControl.GetSelectedItem());
                    break;
                case GestureControl.GestureType.ZX_Up:
                    ToggleSleepMode();
                    break;
                case GestureControl.GestureType.No_Gesture:
                    break;
                default:
                    break;
            }
        }

        //TEST
        private void CenterPageDisplay(int pageCase)
        {
            switch (pageCase)
            {
                case 0:
                    //display calendar
                    fadeIn(_eventCalendarLarge);
                    fadeIn(_wvStreamSmall);
                    fadeOut(_eventCalendar);
                    //fadeOut(_trafficMap);
                    fadeOut(_forecastWeather);
                    fadeOut(_wvStreamBig);
                    break;
                case 1:
                    //display weather
                    fadeOut(_eventCalendarLarge);
                    fadeIn(_eventCalendar);
                    fadeIn(_trafficMap);
                    fadeIn(_forecastWeather);
                    fadeIn(_wvStreamSmall);
                    fadeOut(_wvStreamBig);
                    break;
                case 2:
                    //display home
                    _eventCalendarLarge.Opacity = 0;
                    _eventCalendar.Opacity = 1;
                    fadeIn(_trafficMap);
                    fadeIn(_wvStreamSmall);
                    fadeOut(_forecastWeather);
                    fadeOut(_trafficMapBig);
                    fadeOut(_wvStreamBig);
                    break;
                case 3:
                    //display traffic
                    _eventCalendarLarge.Opacity = 0;
                    _eventCalendar.Opacity = 1;
                    fadeIn(_trafficMapBig);
                    fadeIn(_wvStreamSmall);
                    fadeOut(_wvStreamBig);
                    fadeOut(_trafficMap);
                    _forecastWeather.Opacity = 0;
                    break;
                case 4:
                    //display twitter
                    fadeOut(_trafficMapBig);
                    fadeIn(_trafficMap);
                    fadeOut(_eventCalendarLarge);
                    fadeOut(_wvStreamSmall);
                    fadeIn(_wvStreamBig);
                    break;
                default:
                    _eventCalendarLarge.Opacity = 0;
                    _eventCalendar.Opacity = 1;
                    _trafficMap.Opacity = 1;
                    _forecastWeather.Opacity = 0;
                    _wvStreamBig.Opacity = 0;
                    _wvStreamSmall.Opacity = 0;
                    //all opacity set to 0, home displayed (index of 2)
                    break;
            }
        }

        /// <summary>
        /// fades out screen and only responds to wake gesture
        /// </summary>
        private void ToggleSleepMode()
        {
            if (_sleepMode == false)
            {   
                //fade out everything but the time
                _sleepMode = true;
                fadeOut(_quotes);
                fadeOut(_eventCarouselControl);
                fadeOut(_connectionImage);
                fadeOut(_ipAddress);
                fadeOut(_currentWeather);
                fadeOut(_forecastWeather);
                fadeOut(_eventCalendar);
                fadeOut(_eventCalendarLarge);
                fadeOut(_trafficMap);
                fadeOut(_trafficMapBig);
                fadeOut(_wvStreamSmall);
                fadeOut(_wvStreamBig);
            }
            else
            {
                //home page items
                _eventCarouselControl.SetIndex(2);
                fadeIn(_eventCarouselControl);
                fadeIn(_quotes);
                fadeIn(_eventCalendar);
                fadeIn(_currentWeather);
                fadeIn(_wvStreamSmall);
                fadeIn(_trafficMap);
                fadeIn(_ipAddress);
                fadeIn(_connectionImage);
                _sleepMode = false;
            }
            
           
        }

        /// <summary>
        /// opacity from 1 to 0 in 1 second
        /// </summary>
        /// <param name="displayItem"></param>
        private void fadeOut(UIElement displayItem)
        {
            displayItem.Fade(value: 0.0f, duration: 1000, delay: 0, easingType: EasingType.Cubic).Start();
        }

        /// <summary>
        /// opacity from 0 to 1 in 1 second
        /// </summary>
        /// <param name="displayItem"></param>
        private void fadeIn(UIElement displayItem)
        {
            displayItem.Fade(value: 1.0f, duration: 1000, delay: 500, easingType: EasingType.Cubic).Start();
        }

        private void Page_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.L)
            {
                _eventCarouselControl.ScrollLeft();
                CenterPageDisplay((int)_eventCarouselControl.GetSelectedItem());
            }
            if (e.Key == Windows.System.VirtualKey.R)
            {
                _eventCarouselControl.ScrollRight();
                CenterPageDisplay((int)_eventCarouselControl.GetSelectedItem());
            }
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

            //twitter stuff (timer to update twitta)
            await DoSearchAsync("@KState", 100);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3600) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        async void OnTimerTick(object sender, object e) => await DoSearchAsync("@KState", 100);

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

        #region Map

        async void setUpMap()
        {
            _trafficMap.MapServiceToken = Instance.TrafficAPIKey;
            _trafficMapBig.MapServiceToken = Instance.TrafficAPIKey;

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


                    // Set the map location. CHECK
                    _trafficMap.Center = AndersonHall;
                    _trafficMap.LandmarksVisible = true;
                    _trafficMapBig.Center = myLocation;

                    _trafficMapBig.LandmarksVisible = true;
                    _trafficMap.Height = 450;
                    _trafficMap.Width = 310;
                    break;

                case GeolocationAccessStatus.Denied:
                    // Handle the case  if access to location is denied.
                    break;

                case GeolocationAccessStatus.Unspecified:
                    // Handle the case if  an unspecified error occurs.
                    break;
            }

            _trafficMap.ZoomLevel = 13.55;
            _trafficMapBig.ZoomLevel = 14.65;
            _trafficMap.ZoomLevel = 14;
            //FIX, System.TypeLoadException: 'Requested Windows Runtime type 'Windows.UI.Xaml.Controls.Maps.MapStyleSheet' is not registered.'
            //_trafficMap.StyleSheet = MapStyleSheet.RoadDark();
            //_trafficMapBig.StyleSheet = MapStyleSheet.RoadDark();
        }
        #endregion

        /// <summary>
        /// Twitter
        /// </summary>
        private async Task DoSearchAsync(string query, byte count = 25)
        {
            //var response = await GetToken("[Get from twitter App area]", "[Get from twitter App area]");
            //var msg = response.Content;

            Tweets.Clear();
            _wvStreamSmall.NavigateToString("");
            _wvStreamBig.NavigateToString("");

            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = Instance.ConsumerKey,
                    ConsumerSecret = Instance.ConsumerSecret,
                    AccessToken = Instance.AccessToken,
                    AccessTokenSecret = Instance.AccessTokenSecret
                }
            };


            var twitterCtx = new TwitterContext(auth);


            var searchResponse =
                await
                (from search in twitterCtx.Search
                 where search.Type == SearchType.Search &&
                       search.Query == "\"" + query + "\"" && search.Count == count
                 select search)
                .SingleOrDefaultAsync();


            if (searchResponse != null && searchResponse.Statuses != null)
                foreach (var tweet in searchResponse.Statuses)
                {

                    var newTweet = new Tweet()
                    {
                        Name = tweet.User.Name,
                        NameAt = "@" + tweet.User.ScreenNameResponse,
                        TextRaw = tweet.Text,
                        TextHtml = ParseForHtml(tweet.Text),
                        TextDateTime = tweet.CreatedAt,
                        //AvatarUrl = tweet.User.ProfileImageUrl
                    };

                    if (!(newTweet.NameAt.Contains("@KState")))
                        continue;

                    Tweets.Add(newTweet);

                }


            //using binding to populate webview UI
            HtmlBindingHelper.SetTag(_wvStreamSmall, Tweets.ToList<Tweet>());
            HtmlBindingHelper.SetTag(_wvStreamBig, Tweets.ToList<Tweet>());
        }

        /// <summary>
        /// Twitter
        /// </summary>
        private string ParseForHtml(string message)
        {
            var ret = message;

            //urls
            Regex urlRx = new Regex(@"(http|ftp|https)://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?", RegexOptions.IgnoreCase);
            MatchCollection matches = urlRx.Matches(message);
            foreach (Match match in matches)
            {
                ret = ret.Replace(match.Value, string.Format("<span class='url1' onclick='window.external.notify(\"url|{0}\")'>{0}</span>", cleanString(match.Value, "|")));
            }


            //@
            Regex atRx = new Regex(@"(@)([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?", RegexOptions.IgnoreCase);
            MatchCollection matchesAt = atRx.Matches(ret);
            foreach (Match match in matchesAt)
            {
                ret = ret.Replace(match.Value, string.Format("<span class='at1' onclick='window.external.notify(\"at|{0}\")'>{0}</span>", cleanString(match.Value, "|")));
            }

            //#
            Regex hashRx = new Regex(@"(#)([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?", RegexOptions.IgnoreCase);
            MatchCollection matchesHash = hashRx.Matches(ret);
            foreach (Match match in matchesHash)
            {
                ret = ret.Replace(match.Value, string.Format("<span class='hsh1' onclick='window.external.notify(\"hash|{0}\")'>{0}</span>", cleanString(match.Value, "|")));
            }


            return ret;
        }

        /// <summary>
        /// Twitter
        /// </summary>
        private string cleanString(string raw, string charToRemove)
        {
            return raw.Replace(charToRemove, "'");
        }

        /// <summary>
        /// Twitter
        /// </summary>
        private void wvStream_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var yes = e.Value;
            var parts = yes.Split("|".ToCharArray());

            if (parts[0] == "at")
            {

            }
            else if (parts[0] == "hash")
            {

            }
            else if (parts[0] == "url")
            {

            }

        }

        /// <summary>
        /// Twitter
        /// </summary>
        private void WvStream_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {

        }

        
    }

    /// <summary>
    /// Twitter -- Get this out of main
    /// </summary>
    public class HtmlBindingHelper
    {
        //note: if this were ObservableCollection<tweet> it would not recieve updates as the collection is 
        //the same guid between setTags .. hence why i made it a "List"
        public static List<Tweet> GetTag(DependencyObject obj)
        {
            return (List<Tweet>)obj.GetValue(TagProperty);
        }

        public static void SetTag(DependencyObject obj, List<Tweet> value)
        {
            obj.SetValue(TagProperty, value);
        }

        public static readonly DependencyProperty TagProperty =
            DependencyProperty.RegisterAttached("Tag", typeof(string), typeof(HtmlBindingHelper),
                new PropertyMetadata(new List<Tweet>(), OnTagChanged));

        private static void OnTagChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var wv = (WebView)sender;
            if (e != null && e.NewValue != null && e.NewValue is List<Tweet>)
            {
                var tweets = (List<Tweet>)e.NewValue;

                var htmlStream = "";
                var htmlStyle = "";
                if (wv.Name == "_wvStreamBig")
                {
                    htmlStyle = "<style text='text/css' > "
                     + "body{ color:white;font-family:segoe ui, arial; font-size:13px;} "
                     + ".tw{ min-height:10px;float:left; margin-bottom:10px;} "
                     + ".tw .av{float:left;width:10px;margin-top:5px;} "
                     + ".tw .twtxt{float:right; width:620px;margin-left:10px;} "
                     + ".tw .u1{color:white;font-weight:bold;} "
                     + ".tw .u2{color:grey;margin-left:10px;} "
                     + ".tw .url1{color:blue;text-decoration:underline;cursor:pointer;} "
                     + ".tw .at1{color:green;text-decoration:italic;cursor:pointer;} "
                     + ".tw .hsh1{color:orange;text-decoration:italic;cursor:pointer;} "
                     + ".annoying{opacity:0.3;} "
                     + "</style>";
                }
                else
                {
                    htmlStyle = "<style text='text/css' > "
                     + "body{ color:white;font-family:segoe ui, arial; font-size:11px;} "
                     + ".tw{ min-height:10px;float:left; margin-bottom:10px;} "
                     + ".tw .av{float:left;width:10px;margin-top:5px;} "
                     + ".tw .twtxt{float:right; width:300px;margin-left:10px;} "
                     + ".tw .u1{color:white;font-weight:bold;} "
                     + ".tw .u2{color:grey;margin-left:10px;} "
                     + ".tw .url1{color:blue;text-decoration:underline;cursor:pointer;} "
                     + ".tw .at1{color:green;text-decoration:italic;cursor:pointer;} "
                     + ".tw .hsh1{color:orange;text-decoration:italic;cursor:pointer;} "
                     + ".annoying{opacity:0.3;} "
                     + "</style>";
                }
                foreach (var tweet in tweets)
                {
                    var specialCssClasses = "";
                    if (tweet.NameAt.ToLower() == "@virtuame") specialCssClasses += " annoying";

                    var htmlTemplate = ""
                        + "<div class='tw {4}'>"
                        + " <img src='{0}' class='av'  />"
                        + " <div class='twtxt'>"
                        + "     <span class='u1'>{1}</span>"
                        + "     <span class='u2'>{2}</span>"
                        + "     <div>{3}</div>"
                        + " </div>"
                        + "</div>"
                        ;
                    htmlStream += string.Format(htmlTemplate, tweet.AvatarUrl, tweet.Name, tweet.NameAt,
                        tweet.TextHtml, specialCssClasses);

                }

                wv.NavigateToString(htmlStyle + htmlStream);
            }

        }
    }

    /// <summary>
    /// Twitter -- get out of main
    /// </summary>
    public class Tweet
    {
        public string Name { get; set; }
        public string TextHtml { get; set; }
        public string TextRaw { get; set; }
        public DateTime TextDateTime { get; set; }
        public string AvatarUrl { get; set; }
        public string NameAt { get; set; }

    }



}