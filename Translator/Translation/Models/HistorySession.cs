using MvvmHelpers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Translation.DataService.Models;

namespace Translation.Models
{
    public class HistorySession : ObservableRangeCollection<Session>
    {
        private DateTime _sessionDate;
        public DateTime SessionDate
        {
            get { return _sessionDate; }
            set
            {
                _sessionDate = value;
                OnPropertyChanged();
            }
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
