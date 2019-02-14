﻿using Microsoft.Extensions.DependencyInjection;
using Mirror.IO;
using Mirror.Networking;
using Mirror.Speech;
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

            collection.AddSingleton<IAudioService, AudioService>();
            collection.AddSingleton<IBluetoothService, BluetoothService>();
            collection.AddSingleton<IPhotoService, PhotoService>();
            collection.AddSingleton<IWeatherService, WeatherService>();
            collection.AddSingleton<ICalendarService, CalendarService>();
            collection.AddSingleton<ISpeechEngine, SpeechEngine>();

            return collection.BuildServiceProvider();
        }

        public static T Get<T>() => Container.Value.GetService<T>();
    }
}