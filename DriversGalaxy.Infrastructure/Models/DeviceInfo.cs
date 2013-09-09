using System.Collections.Generic;
using System.ComponentModel;

namespace DriversGalaxy.Models
{
	public class DeviceInfo : DeviceInfoBase, INotifyPropertyChanged
	{

		/// <summary>
		/// Default constructor for the <c>XmlSerializer</c>
		/// </summary>
		public DeviceInfo()
		{           
        }

		public DeviceInfo(string deviceClass, string deviceClassName, string deviceName, string infName, string version, string id, string hardwareID, string compatID)
        {
            if (string.IsNullOrEmpty(deviceClass) && !string.IsNullOrEmpty(deviceClassName))
            {
                if (deviceClassNames.ContainsKey(deviceClassName))
                {
                    deviceClass = deviceClassNames[deviceClassName];
                }
            }

			DeviceClass = deviceClass != null ? deviceClass : "null";
			DeviceClassName = deviceClassName != null ? deviceClassName : "null";
			DeviceName = deviceName != null ? deviceName : "null";
			InfName = infName != null ? infName : "null";
			Version = version != null ? version : "null";
			Id = id != null ? id : "null";
			HardwareID = hardwareID != null ? hardwareID : "null";
			CompatID = compatID != null ? compatID : "null";
		}

		bool selectedForUpdate;
		public bool SelectedForUpdate
		{
			get { return selectedForUpdate; }
			set { selectedForUpdate = value; OnPropertyChanged("SelectedForUpdate"); }
		}
		bool selectedForExclude;
		public bool SelectedForExclude
		{
			get { return selectedForExclude; }
			set { selectedForExclude = value; OnPropertyChanged("SelectedForExclude"); }
		}
		bool selectedForBackup;
		public bool SelectedForBackup
		{
			get { return selectedForBackup; }
			set { selectedForBackup = value; OnPropertyChanged("SelectedForBackup"); }
		}
		bool selectedForRestore;
		public bool SelectedForRestore
		{
			get { return selectedForRestore; }
			set { selectedForRestore = value; OnPropertyChanged("SelectedForRestore"); }
		}

		public string InstalledDriverDate { get; set; }
		public string NewDriverDate { get; set; }
		public bool IsExcluded { get; set; }
		public bool NeedsUpdate { get; set; }
	};
}
