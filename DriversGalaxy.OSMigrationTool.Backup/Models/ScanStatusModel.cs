
namespace DriversGalaxy.OSMigrationTool.Backup.Models
{
	public enum ScanStatus
	{
		NotStarted,
		ScanStarted,
		ScanFinishedNoDrivers,
		ScanFinishedDriversFound,
		DownloadStarted,
		DownloadFinished,
		ComposeStarted,
		ComposeFinished
	};
}
 