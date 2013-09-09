using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DriversGalaxy.Utils
{
	/// <summary>
	/// Utilities for interacting with ini files in general.
	/// </summary>
	static class IniFileUtils
	{
		#region Variables

		private static Hashtable dirids = new Hashtable();

		#endregion

		#region Constructors

		static IniFileUtils()
		{
			dirids.Add(10, Environment.GetEnvironmentVariable("SystemRoot"));
			dirids.Add(11, Environment.GetEnvironmentVariable("SystemRoot") + "\\system32");
			dirids.Add(12, Environment.GetEnvironmentVariable("SystemRoot") + "\\system32\\drivers");
			dirids.Add(17, Environment.GetEnvironmentVariable("SystemRoot") + "\\inf");
			dirids.Add(18, Environment.GetEnvironmentVariable("SystemRoot") + "\\help");
			dirids.Add(20, Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts");
			dirids.Add(24, Environment.GetEnvironmentVariable("SystemDrive"));
			dirids.Add(50, Environment.GetEnvironmentVariable("SystemRoot") + "\\system");
			dirids.Add(53, Environment.GetEnvironmentVariable("UserProfile"));
			dirids.Add(16406, Environment.GetEnvironmentVariable("AllUsersProfile") + "\\Microsoft\\Windows\\Start Menu");
			dirids.Add(16407, Environment.GetEnvironmentVariable("AllUsersProfile") + "\\Microsoft\\Windows\\Start Menu\\Programs");
			dirids.Add(16408, Environment.GetEnvironmentVariable("AllUsersProfile") + "\\Microsoft\\Windows\\Start Menu\\Programs\\Startup");
			dirids.Add(16409, Environment.GetEnvironmentVariable("public") + "\\Desktop");
			dirids.Add(16415, Environment.GetEnvironmentVariable("public") + "\\Favorites");
			dirids.Add(16419, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
			dirids.Add(16422, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
			dirids.Add(16425, Environment.GetEnvironmentVariable("SystemRoot") + "\\SysWOW64");
			dirids.Add(16426, Environment.GetEnvironmentVariable("ProgramFiles(x86)"));
			dirids.Add(16427, Environment.GetEnvironmentVariable("CommonProgramFiles"));
			dirids.Add(16428, Environment.GetEnvironmentVariable("CommonProgramFiles(x86)"));
			//dirids.Add(16429, Environment.GetFolderPath(Environment.SpecialFolder.CommonTemplates));
			dirids.Add(16430, Environment.GetEnvironmentVariable("public") + "\\Public Documents");
		}

		#endregion

		#region Methods

		/// <summary>
		/// Lists the keys in the specified section in the ini file.
		/// </summary>
		/// <param name="iniFileName">The path to the ini file</param>
		/// <param name="section">The section name</param>
		/// <returns>A list of keys under the specified section</returns>
		public static List<string> GetKeys(string iniFileName, string section)
		{
			string returnString = new string(' ', 32768);
			GetPrivateProfileString(section, null, null, returnString, 32768, iniFileName);

			List<string> result = new List<string>(returnString.Split('\0'));
			result.RemoveRange(result.Count - 2, 2);
			return result;
		}

		/// <summary>
		/// Gets the value of a key in some section inside the ini file.
		/// </summary>
		/// <param name="iniFileName">The path to the ini file</param>
		/// <param name="section">The section name</param>
		/// <param name="key">The key name</param>
		/// <returns>The value of the key</returns>
		public static string GetValue(string iniFileName, string section, string key)
		{
			string returnString = new string(' ', 255);
			GetPrivateProfileString(section, key, "", returnString, 255, iniFileName);

			// Remove comment if exists
			if (returnString.Contains(';'))
				returnString = returnString.Remove(returnString.IndexOf(";"));

			returnString = returnString.Trim();

			if (returnString.EndsWith("\0"))
				returnString = returnString.Remove(returnString.Length - 1);

			return returnString;
		}

		/// <summary>
		/// Resolves dirid in DestinationDirs section to actual paths.
		/// </summary>
		/// <param name="dirid">The dirid</param>
		/// <returns>The absolute path mapped to the dirid</returns>
		public static string ResolveDirId(int dirid)
		{
			return dirids[dirid].ToString();
		}

		#endregion

		#region PInvokes

		[DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
			SetLastError = true,
			CharSet = CharSet.Unicode, ExactSpelling = true,
			CallingConvention = CallingConvention.StdCall)]

		private static extern int GetPrivateProfileString(
			string lpAppName,
			string lpKeyName,
			string lpDefault,
			string lpReturnString,
			int nSize,
			string lpFilename);

		#endregion
	}
}
