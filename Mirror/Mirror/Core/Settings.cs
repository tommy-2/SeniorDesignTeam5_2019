using Mirror.Extensions;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;


namespace Mirror.Core
{
    public class Settings
    {
        const string Configuration = nameof(Configuration);
        static readonly Lazy<Settings> _settings = new Lazy<Settings>(() => new Settings());

        public static Settings Instance { get; } = _settings.Value;

        public string City { get; private set; }
        public string OpenWeatherApiKey { get; private set; }
        public string WeatherUom { get; private set; }
        public string TrafficAPIKey { get; private set; }
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }
        public string AccessToken { get; private set; }
        public string AccessTokenSecret { get; private set; }

        public List<CalendarConfig> Calendars { get; private set; }

        Settings()
        {
            var resourceLoader = ResourceLoader.GetForViewIndependentUse(Configuration);
            City = resourceLoader.GetString(nameof(City));
            OpenWeatherApiKey = resourceLoader.GetString(nameof(OpenWeatherApiKey));
            WeatherUom = resourceLoader.GetString(nameof(WeatherUom));
            Calendars = resourceLoader.GetString(nameof(Calendars)).Deserialize<List<CalendarConfig>>();
            TrafficAPIKey = resourceLoader.GetString(nameof(TrafficAPIKey));
            ConsumerKey = resourceLoader.GetString(nameof(ConsumerKey));
            ConsumerSecret = resourceLoader.GetString(nameof(ConsumerSecret));
            AccessToken = resourceLoader.GetString(nameof(AccessToken));
            AccessTokenSecret = resourceLoader.GetString(nameof(AccessTokenSecret));
        }
    }
    
    public class CalendarConfig
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsUsingCredentials 
            => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }
}