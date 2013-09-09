using System.Collections.Generic;
using System.ComponentModel;

namespace DriversGalaxy.Models
{
	public class MigrationDeviceInfo : DeviceInfoBase, INotifyPropertyChanged
	{
		/// <summary>
		/// Default constructor for the <c>XmlSerializer</c>
		/// </summary>
		public MigrationDeviceInfo()
		{ }

		public MigrationDeviceInfo(string deviceClass, string deviceClassName, string deviceName, string infName, string version, string id, string hardwareID, string compatID)
		{
			DeviceClass = deviceClass != null ? deviceClass : "null";
			DeviceClassName = deviceClassName != null ? deviceClassName : "null";
			DeviceName = deviceName != null ? deviceName : "null";
			InfName = infName != null ? infName : "null";
			Version = version != null ? version : "null";
			Id = id != null ? id : "null";
			HardwareID = hardwareID != null ? hardwareID : "null";
			CompatID = compatID != null ? compatID : "null";
		}

		bool selectedForDestOSDriverDownload;
		public bool SelectedForDestOSDriverDownload
		{
			get { return selectedForDestOSDriverDownload; }
			set { selectedForDestOSDriverDownload = value; OnPropertyChanged("SelectedForDestOSDriverDownload"); }
		}

		public bool IsDestOSDriverAvailable { get; set; }
	};
}
