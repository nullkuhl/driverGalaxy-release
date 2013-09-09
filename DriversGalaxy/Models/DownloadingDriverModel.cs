using System.ComponentModel;

namespace DriversGalaxy.Models
{
	public class DownloadingDriverModel : INotifyPropertyChanged
	{
		public DownloadingDriverModel() { }

		public DownloadingDriverModel(string operation, DeviceInfo device)
		{
			Operation = operation;
			Device = device;
		}

		public string Operation { get; set; }
		public DeviceInfo Device { get; set; }

		#region INotifyPropertyChanged

		void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	};
}
