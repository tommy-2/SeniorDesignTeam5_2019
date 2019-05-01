using Microsoft.Extensions.DependencyInjection;
using Mirror.IO;
using Mirror.Networking;
using System;

namespace Mirror.Core
{
    public static class Services
    {
        static Lazy<IServiceProvider> Container 
            => new Lazy<IServiceProvider>(
                () => BuildAndConfigureServices());

        static IServiceProvider BuildAndConfigureServices()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton<IWeatherService, WeatherService>();
            collection.AddSingleton<ICalendarService, CalendarService>();
            collection.AddSingleton<ISerialService, SerialService>();

            return collection.BuildServiceProvider();
        }

        public static T Get<T>() => Container.Value.GetService<T>();
    }
}