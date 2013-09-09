using System.Collections.Generic;
using System.ComponentModel;

namespace DriversGalaxy.Models
{
	public class DevicesGroup : INotifyPropertyChanged
	{
		/// <summary>
		/// Default constructor for the <c>XmlSerializer</c>
		/// </summary>
		public DevicesGroup() { }

		public DevicesGroup(string deviceClass, string deviceClassName, string deviceClassImageSmall, List<DeviceInfo> devices)
		{
			DeviceClass = deviceClass;
			DeviceClassName = deviceClassName;
			DeviceClassImageSmall = deviceClassImageSmall;
			Devices = devices;
		}

		bool groupChecked;
		public bool GroupChecked
		{
			get { return groupChecked; }
			set { groupChecked = value; OnPropertyChanged("GroupChecked"); }
		}
		string deviceClass;
		public string DeviceClass
		{
			get { return deviceClass; }
			set
			{
				deviceClass = value;
				switch (deviceClass)
				{
					case "DISPLAY":
						{
							Order = 0;
							break;
						}
					case "MONITOR":
						{
							Order = 1;
							break;
						}
					case "MEDIA":
						{
							Order = 2;
							break;
						}
					case "BATTERY":
						{
							Order = 4;
							break;
						}
					case "DISKDRIVE":
						{
							Order = 5;
							break;
						}
					case "CDROM":
						{
							Order = 5;
							break;
						}
					case "FDC":
						{
							Order = 5;
							break;
						}
					case "FLOPPYDISK":
						{
							Order = 5;
							break;
						}
					case "KEYBOARD":
						{
							Order = 7;
							break;
						}
					case "COMPUTER":
						{
							Order = 8;
							break;
						}
					case "VOLUME":
						{
							Order = 9;
							break;
						}
					case "PORTS":
						{
							Order = 9;
							break;
						}
					case "USB":
						{
							Order = 10;
							break;
						}
					case "NET":
						{
							Order = 11;
							break;
						}
					case "MOUSE":
						{
							Order = 12;
							break;
						}
					case "HIDCLASS":
						{
							Order = 12;
							break;
						}
					case "PROCESSOR":
						{
							Order = 13;
							break;
						}
					case "SYSTEM":
						{
							Order = 14;
							break;
						}
					default:
						{
							Order = 6;
							break;
						}
				}
			}
		}
		public int Order { get; set; }
		public string DeviceClassName { get; set; }
		public string DeviceClassImageSmall { get; set; }
		public List<DeviceInfo> Devices { get; set; }

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
