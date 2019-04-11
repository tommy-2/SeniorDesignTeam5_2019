using Mirror.Extensions;
using System;
using Windows.UI.Xaml;

namespace Mirror.ViewModels
{
    public class QuotesViewModel : BaseViewModel
    {

		string _quote;
		
		public string Quote => $"{_quote}";

        public QuotesViewModel(DependencyObject dependency, string quote) : base(dependency)
        {
            _quote = quote;
        }

        public override string ToFormattedString(DateTime? dateContext) => $"Some quote.";
    }
}