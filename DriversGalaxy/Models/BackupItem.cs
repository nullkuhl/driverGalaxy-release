using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace DriversGalaxy.Models
{
	public class BackupItem
	{
		public BackupItem()
		{
		}

		public BackupItem(string path, DateTime time, string type, ObservableCollection<DevicesGroup> groupedDrivers)
		{
			Path = path;
			Time = time;
			Type = type;
			GroupedDrivers = groupedDrivers;
		}

		public DateTime Time { get; set; }
		public string Type { get; set; }
		public string Path { get; set; }
		public ObservableCollection<DevicesGroup> GroupedDrivers { get; set; }
	};
}
