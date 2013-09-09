using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace DriversGalaxy.Models
{
	public class DestinationOSDeviceInfo : DeviceInfoBase, INotifyPropertyChanged
	{
		/// <summary>
		/// Default constructor for the <c>XmlSerializer</c>
		/// </summary>
		public DestinationOSDeviceInfo()
		{

		}

		public DestinationOSDeviceInfo(string deviceClass, string deviceClassName, string deviceName, string infName, string version, string id, string hardwareID, string compatID)
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

		[XmlIgnore]
		bool selectedForInstall = true;
		[XmlIgnore]
		public bool SelectedForInstall
		{
			get { return selectedForInstall; }
			set { selectedForInstall = value; OnPropertyChanged("SelectedForInstall"); }
		}
	};
}
