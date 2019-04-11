using Mirror.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.IO;
using Windows.Storage;


namespace Mirror
{
    public sealed partial class Quotes : UserControl
    {
		public const int MAXFILELENGTH = 2048;
        DispatcherTimer _timer;
        string[] quotes = new string[MAXFILELENGTH];
		int quoteCount = 0;
		Random rnd = new Random();

        public Quotes()
        {
            InitializeComponent();

            _content.Opacity = 0;
			LoadQuotesAsync();
            UpdateQuote();			
            _fadeIn.Begin();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        void OnTimerTick(object sender, object e) => UpdateQuote();

        void UpdateQuote()
        {
            //_fadeOut.Begin();
            //_scroll.Stop();
            DataContext = new QuotesViewModel(this, GetQuote());
            _scroll.Begin();
            //_fadeIn.Begin();
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
        }
		
		string GetQuote()
		{
			return quotes[rnd.Next(0, quoteCount)];
		}
		
		async void LoadQuotesAsync ()
		{
            /*
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile quotesFile = await storageFolder.GetFileAsync("quotes.txt");
            string text = await Windows.Storage.FileIO.ReadTextAsync(quotesFile);
            quotes = text.Split('\n');
            */

            // inspired by: https://stackoverflow.com/questions/34583303/how-to-read-a-text-file-in-windows-universal-app

            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///quotes.txt"));
            using (var inputStream = await file.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                while (streamReader.Peek() >= 0 && quoteCount < MAXFILELENGTH)
                {
                    quotes[quoteCount++] = streamReader.ReadLine();
                }
            }
        }
		
    }
}