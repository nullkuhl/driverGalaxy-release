using System;
using WPFLocalizeExtension.Extensions;

namespace DriversGalaxy.OSMigrationTool.Backup.Infrastructure
{
	public class WPFLocalizeExtensionHelpers
	{
		public static string GetUIString(string key)
		{
			string uiString;
            LocTextExtension locExtension = new LocTextExtension(String.Format("DriversGalaxy.OSMigrationTool.Backup:Resources:{0}", key));
			locExtension.ResolveLocalizedValue(out uiString);
			return uiString;
		}
	}
}
