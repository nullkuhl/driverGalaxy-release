using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DriversGalaxy.Models
{
	public enum BackupStatus
	{
		NotStarted,
        Started,
		BackupTargetsSelection,
		BackupFinished,
		RestoreTargetsSelection,
		RestoreFinished
	};
}
 