using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Translation.DataService.Models
{
	public abstract class BaseModel : INotifyPropertyChanged
	{
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
