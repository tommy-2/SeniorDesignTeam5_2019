using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Mirror.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirror
{
    public class ControlIcon
    {
        public string CItem
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CarouselControl : UserControl
    {
        //items in the carousel
        ObservableCollection<ControlIcon> items = new ObservableCollection<ControlIcon>();

        public enum CarouselItem
        {
            Calendar,
            Weather,
            Home,
            Traffic,
            Twitter
        }

        public CarouselControl()
        {
            this.InitializeComponent();
            items.Add(new ControlIcon() { CItem = string.Format("ms-appx:///Assets//Carousel//calendar.png") });
            items.Add(new ControlIcon() { CItem = string.Format("ms-appx:///Assets//Carousel//weather.png") });
            items.Add(new ControlIcon() { CItem = string.Format("ms-appx:///Assets//Carousel//home.png") });
            items.Add(new ControlIcon() { CItem = string.Format("ms-appx:///Assets//Carousel//traffic.png") });
            items.Add(new ControlIcon() { CItem = string.Format("ms-appx:///Assets//Carousel//twitter.png") });
            //items.Add(new ControlIcon() { CItem = string.Format("ms-appx:///Assets//Carousel//settings.png") });
            MirrorCarousel.ItemsSource = items;

            MirrorCarousel.SelectedIndex = 2; /* initialize index to home */

        }

        /// <summary>
        /// for motion sensor call
        /// </summary>
        public void ScrollLeft()
        {
            if (MirrorCarousel.SelectedIndex != (items.Count - 1))
            {
                MirrorCarousel.SelectedIndex++;
                //Transition Duration??
                //Set Selected item?
            }

        }

        /// <summary>
        /// for motion sensor call
        /// </summary>
        public void ScrollRight()
        {
            if (MirrorCarousel.SelectedIndex != 0)
            {
                MirrorCarousel.SelectedIndex--;
            }
        }

        public CarouselItem GetSelectedItem()
        {
            return (CarouselItem)MirrorCarousel.SelectedIndex;
        }

        public void SetIndex(int index)
        {
                MirrorCarousel.SelectedIndex = index;
        }


    }
}
