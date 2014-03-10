using DictionarySuggestDemo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Observable.Aliases;
using System.Threading.Tasks;

namespace DictionarySuggestDemo.ViewModels
{
    public class SearchViewModel : INotifyPropertyChanged, IDisposable
    {
        private string _searchText;
        private IList<string> _suggestions;
        private bool _hasSuggestions;

        #region Subscription

        private IDisposable _subscription;

        #endregion

        public SearchViewModel(ISuggestService service)
        {
            #region SearchTerms

            var searchTerms = Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "SearchText")
                .Select(_ => SearchText ?? string.Empty)
                .Throttle(TimeSpan.FromSeconds(0.5))
                .DistinctUntilChanged();

            #endregion
            #region SuggestionUpdates

            var suggestionUpdates = searchTerms.Select(
                    term => term.Length < 3 ?
                        Task.FromResult(new string[0]) :
                        service.GetSuggestions(term))
                .Switch();

            #endregion
            #region Subscription

            _subscription = suggestionUpdates
                .Subscribe(suggestions =>
                    {
                        Suggestions = suggestions;
                        HasSuggestions = suggestions != null && suggestions.Any();
                    });

            #endregion
        }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        public IList<string> Suggestions
        {
            get
            {
                return _suggestions;
            }
            private set
            {
                _suggestions = value;
                OnPropertyChanged("Suggestions");
            }
        }

        public bool HasSuggestions
        {
            get
            {
                return _hasSuggestions;
            }
            private set
            {
                _hasSuggestions = value;
                OnPropertyChanged("HasSuggestions");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region IDisposable

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_subscription != null)
                    _subscription.Dispose();
            }
        }

        #endregion
    }
}
