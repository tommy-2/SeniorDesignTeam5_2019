#region Using Statement(s)

using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Text.RegularExpressions;

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

        public ObservableCollection<Tweet> Tweets { get; set; }
        public MainPage()
        {
            InitializeComponent();
            DataContext = new HudViewModel(this);
            Tweets = new ObservableCollection<Tweet>();
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
            await DoSearchAsync("@KState", 100);
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

                    // Set the map location.
                    _trafficMap.Center = myLocation;
                    _trafficMap.LandmarksVisible = true;
                    _trafficMapBig.Center = myLocation;
                    _trafficMapBig.LandmarksVisible = true;
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
            //FIX, System.TypeLoadException: 'Requested Windows Runtime type 'Windows.UI.Xaml.Controls.Maps.MapStyleSheet' is not registered.'
            _trafficMap.StyleSheet = MapStyleSheet.RoadDark();
            _trafficMapBig.StyleSheet = MapStyleSheet.RoadDark();
        }
        #endregion

        #region Twitter
        private async Task DoSearchAsync(string query, byte count = 25)
        {
            //var response = await GetToken("[Get from twitter App area]", "[Get from twitter App area]");
            //var msg = response.Content;

            Tweets.Clear();
            wvStream.NavigateToString("");

            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = "",
                    ConsumerSecret = "",
                    AccessToken = "",
                    AccessTokenSecret = ""
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
            HtmlBindingHelper.SetTag(wvStream, Tweets.ToList<Tweet>());
        }


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


        private string cleanString(string raw, string charToRemove)
        {
            return raw.Replace(charToRemove, "'");
        }


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

        private void WvStream_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {

        }
    }

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
                var htmlStyle = "<style text='text/css' > "
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

    public class Tweet
    {
        public string Name { get; set; }
        public string TextHtml { get; set; }
        public string TextRaw { get; set; }
        public DateTime TextDateTime { get; set; }
        public string AvatarUrl { get; set; }
        public string NameAt { get; set; }

    }
    #endregion

}