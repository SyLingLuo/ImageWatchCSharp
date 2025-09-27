using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace  ImageWatchCSharp.Bind
{
	 
	public class BindableBase : INotifyPropertyChanged
	{
		 
		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
		{
			if (!object.Equals(storage, value))
			{
				storage = value;
				this.NotifyPropertyChanged(propertyName);
				return true;
			}
			return false;
		}

		 
		public void NotifyAllPropertiesChanged()
		{
			this.NotifyPropertyChanged("");
		}

		 
		 
		 
		public event PropertyChangedEventHandler PropertyChanged;

		 
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
