﻿#region Using Statement(s)

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
using RawEmotion = Microsoft.ProjectOxford.Emotion.Contract.Emotion;

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

        public MainPage()
        {
            InitializeComponent();
            DataContext = new HudViewModel(this);
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _messageLabel.Text = GetTimeOfDayGreeting();

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

    }
}