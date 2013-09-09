using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace DriversGalaxy.Utils
{
    /// <summary>
    /// Contains a collection of useful utitlites for dealing with allDevices and drivers.
    /// </summary>
    public class DriverUtils
    {
        #region Static Variables

        public static string SaveDir;
        /// <summary>
        /// Driver classes which are not included in a Freemium Driver Utilites scan results
        /// </summary>
        public static readonly String[] RestrictedClasses = { "COMPUTER", "VOLUME", "DISKDRIVE", "LEGACYDRIVER", "PROCESSOR" };

        #endregion

        #region Instance Variables

        private bool needsReboot;

        #endregion

        #region Events
        /// <summary>
        /// Occurs when the currently downloaded driver file finishes download.
        /// </summary>
        public event EventHandler<System.ComponentModel.AsyncCompletedEventArgs> FileDownloadCompleted;

        #endregion

        #region Methods

        private static bool Is64BitWindows()
        {
            return IntPtr.Size == 8;
        }


        /// <summary>
        /// Scans computer for plug-and-play allDevices.
        /// </summary>
        /// <returns></returns>
        public ManagementObjectCollection ScanDevices()
        {
            var devicesQuery = new ManagementObjectSearcher("root\\CIMV2", @"SELECT * FROM Win32_PnPSignedDriver WHERE DeviceName IS NOT NULL");

            return devicesQuery.Get();
        }


        /// <summary>
        /// Make sure to call this before any driver installations or restorations
        /// to get the correct decision of whether to reboot.
        /// </summary>
        public void WatchForReboot()
        {
            needsReboot = false;
        }

        /// <summary>
        /// Checks if Reboot is required (to be called after installations or restorations
        /// of drivers).
        /// </summary>
        /// <returns><code>true</code> if system reboot is required, <code>false</code> otherwise</returns>
        public bool IsRebootRequired()
        {
            return needsReboot;
        }

        /// <summary>
        /// Backs up a driver.
        /// </summary>
        /// <param name="deviceName">The device name whose driver is to be backed up</param>
        /// <param name="infFileName">The name of the driver inf file</param>
        /// <param name="backupDir">The output backup directory</param>               
        public bool BackupDriver(string deviceName, string infFileName, string backupDir)
        {
            bool result = false;

            try
            {
                backupDir = backupDir.EndsWith("\\") ? backupDir : backupDir + "\\";
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                deviceName = deviceName.Trim().Replace('/', ' ').Replace('\\', ' ');
                var deviceBackupDir = backupDir + deviceName + "\\";
                if (!Directory.Exists(deviceBackupDir))
                    Directory.CreateDirectory(deviceBackupDir);

                // Empty target device backup dir
                var oldFiles = new DirectoryInfo(deviceBackupDir).GetFiles();
                foreach (var oldFile in oldFiles)
                {
                    oldFile.Delete();
                }

                var oldDirs = new DirectoryInfo(deviceBackupDir).GetDirectories();
                foreach (var oldDir in oldDirs)
                {
                    oldDir.Delete(true);
                }



                /*********************************************************************************************************************/
                /* information can be found at : http://msdn.microsoft.com/en-us/library/windows/hardware/ff553973%28v=vs.85%29.aspx */
                /*********************************************************************************************************************/

                // Check if driver exists
                String windir = Environment.GetEnvironmentVariable("windir") + "\\";
                String driverStorePath = windir + "System32\\DriverStore\\FileRepository";

                if (Directory.Exists(driverStorePath))
                {
                    // Driver Store (Windows Vista and Windows Server 2008, Windows 7)

                    String[] possibleDriverDirsInStore = Directory.GetDirectories(driverStorePath, infFileName + "*");
                    if (possibleDriverDirsInStore != null && possibleDriverDirsInStore.Length == 1)
                    {
                        CopyFolder(possibleDriverDirsInStore[0], deviceBackupDir);
                    }
                }
                else
                {
                    // DevicePath (Windows Server 2003, Windows XP, and Windows 2000)

                    RegistryKey currentVersion = Registry.LocalMachine.OpenSubKey(@"SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion");
                    String[] devicePaths = currentVersion.GetValue("DevicePath").ToString().Split(';');
                    String devicePath = devicePaths[0];

                    // Backup inf file
                    String infFilePath = Path.Combine(devicePath, infFileName);
                    File.Copy(infFilePath, deviceBackupDir + infFileName);

                    // Backup PNF file
                    var pnfFileName = infFileName.Replace(".inf", ".PNF");
                    var pnfFilePath = Path.Combine(devicePath, pnfFileName);
                    File.Copy(pnfFilePath, deviceBackupDir + pnfFileName);

                    // Backup CAT file
                    string originalCATName = IniFileUtils.GetValue(infFilePath, "Version", "CatalogFile");
                    if (!String.IsNullOrEmpty(originalCATName))
                    {
                        var catName = infFileName.Replace(".inf", ".cat");
                        var catroot = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\catroot";
                        var catrootDirs = new DirectoryInfo(catroot).GetDirectories();

                        foreach (var dir in catrootDirs)
                        {
                            var catPath = dir.FullName + "\\" + catName;
                            if (File.Exists(catPath))
                            {
                                File.Copy(catPath, deviceBackupDir + originalCATName);
                                break;
                            }
                        }
                    }

                    // backup "layout" file
                    String layoutFile = IniFileUtils.GetValue(infFilePath, "Version", "LayoutFile");
                    if (!String.IsNullOrEmpty(layoutFile))
                    {
                        File.Copy(Path.Combine(devicePath, layoutFile), deviceBackupDir + layoutFile);
                    }


                    // Backup driver files from by parsing the inf file
                    if (Is64BitWindows())
                        BackupDriverFilesFromInf(".amd64", infFilePath, deviceBackupDir);
                    else
                        BackupDriverFilesFromInf(".x86", infFilePath, deviceBackupDir);
                }
                result = true;
            }
            catch { }

            return result;
        }

        /// <summary>
        /// Restores a backed up driver to the system dir.
        /// </summary>
        /// <param name="deviceName">The name of device whose driver was backed up</param>
        /// <param name="backupDir">The backup directory</param>
        /// <returns><code>True</code> if restoring was successfull, <code>False</code> otherwise</returns>
        public bool RestoreDriver(string deviceName, string backupDir)
        {
            bool result = false;

            // Format paths
            deviceName = deviceName.Trim().Replace('/', ' ').Replace('\\', ' ');
            backupDir = !backupDir.EndsWith("\\") ? backupDir + "\\" : backupDir;

            try
            {
                // Find inf file
                var deviceBackupDirPath = backupDir + deviceName;
                var deviceBackupDir = new DirectoryInfo(deviceBackupDirPath);
                var infFile = deviceBackupDir.GetFiles("*.inf")[0];
                bool driverNeedsReboot;

                // http://msdn.microsoft.com/en-us/library/windows/hardware/ff544813%28v=vs.85%29.aspx
                Int32 flags = DRIVER_PACKAGE_FORCE | DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT;
                if (Environment.OSVersion.Version.Major < 6) flags |= DRIVER_PACKAGE_LEGACY_MODE;

                //SetDifxLogCallback(new DIFLOGCALLBACK(DIFLogCallbackFunc), IntPtr.Zero);
                Int32 err = DriverPackageInstall(infFile.FullName, flags, IntPtr.Zero, out driverNeedsReboot);

                needsReboot = needsReboot || driverNeedsReboot;

                result = (err == ERROR_SUCCESS);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return result;
        }

        private void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }


        // http://msdn.microsoft.com/en-us/library/windows/hardware/ff547465%28v=vs.85%29.aspx
        private const String INF_SourceDisksFiles = "SourceDisksFiles";
        private const String INF_SourceDisksNames = "SourceDisksNames";
        private const String INF_DestinationDirs = "DestinationDirs";



        private static List<String> GetDriverFiles(string platform, string infFilePath, ref String sdf)
        {
            List<String> driverFiles = IniFileUtils.GetKeys(infFilePath, sdf);
            if (driverFiles.Count == 0)
            {
                sdf = INF_SourceDisksFiles;
                driverFiles.AddRange(IniFileUtils.GetKeys(infFilePath, sdf));
            }

            return driverFiles;
        }
        private static List<String> GetDiskNames(string platform, string infFilePath, ref String sdn)
        {
            List<String> sourceDisksNames = IniFileUtils.GetKeys(infFilePath, sdn);
            if (sourceDisksNames.Count == 0)
            {
                sdn = INF_SourceDisksNames;
                sourceDisksNames.AddRange(IniFileUtils.GetKeys(infFilePath, sdn));
            }

            return sourceDisksNames;
        }
        private static List<String> GetSearchDirs(string infFilePath)
        {
            List<String> destinationDirs = IniFileUtils.GetKeys(infFilePath, INF_DestinationDirs);

            List<String> searchDirs = new List<String>();
            foreach (String dir in destinationDirs)
            {
                var dirVal = IniFileUtils.GetValue(infFilePath, INF_DestinationDirs, dir).Split(',');
                var dirid = int.Parse(dirVal[0]);

                var searchDir = IniFileUtils.ResolveDirId(dirid);
                if (dirVal.Length > 1) searchDir += "\\" + dirVal[1].Trim();

                searchDirs.Add(searchDir);
            }

            return searchDirs;
        }

        private void BackupDriverFilesFromInf(string platform, string infFilePath, string deviceBackupDir)
        {
            String layoutFilePath = null;
            if (!String.IsNullOrEmpty(IniFileUtils.GetValue(infFilePath, "Version", "LayoutFile")))
            {
                // system-supplied INF
                layoutFilePath = Path.Combine(Path.GetDirectoryName(infFilePath), IniFileUtils.GetValue(infFilePath, "Version", "LayoutFile"));
            }

            String sdf = INF_SourceDisksFiles + platform;
            String sdn = INF_SourceDisksNames + platform;

            // Get driver files
            List<String> driverFiles = GetDriverFiles(platform, layoutFilePath ?? infFilePath, ref sdf);

            // Determine source disks names section
            List<String> sourceDisk = GetDiskNames(platform, layoutFilePath ?? infFilePath, ref sdn);

            // Get search dirs
            List<String> searchDirs = GetSearchDirs(infFilePath);


            //
            foreach (String driverFile in driverFiles)
            {
                /** seems to have a meaning only on windows Vista + **/
                String[] sourceDiskIds = IniFileUtils.GetValue(layoutFilePath ?? infFilePath, sdf, driverFile).Split(',');

                List<String> sourcePath = new List<String>();
                foreach (String sourceDiskId in (from d in sourceDiskIds where !String.IsNullOrEmpty(d) select d))
                {
                    String[] paths = IniFileUtils.GetValue(layoutFilePath ?? infFilePath, sdn, sourceDiskId).Split(',');
                    sourcePath.AddRange((from p in paths where !String.IsNullOrEmpty(p) select p));
                }
                sourcePath = sourcePath.Distinct().ToList();
                sourcePath.Add(""); // add local path


                var backupSubdir = deviceBackupDir;
                if (sourcePath.Count == 4)
                    backupSubdir = Path.Combine(deviceBackupDir, sourcePath[3]);

                if (!Directory.Exists(backupSubdir))
                    Directory.CreateDirectory(backupSubdir);
                /*********************************************************/


                foreach (String possibleDir in searchDirs)
                {
                    if (File.Exists(possibleDir + "\\" + driverFile))
                    {
                        File.Copy(possibleDir + "\\" + driverFile, backupSubdir + "\\" + driverFile);
                        break;
                    }
                }
            }
        }

        #endregion

        #region PInvokes

        const uint ERROR_SUCCESS = 0x00000000;

        private enum DIFXAPI_LOG
        {
            DIFXAPI_SUCCESS = 0, // Successes
            DIFXAPI_INFO = 1,    // Basic logging information that should always be shown
            DIFXAPI_WARNING = 2, // Warnings
            DIFXAPI_ERROR = 3    // Errors
        }

        private delegate void DIFLOGCALLBACK(DIFXAPI_LOG EventType, Int32 ErrorCode, [MarshalAs(UnmanagedType.LPTStr)] string EventDescription, IntPtr CallbackContext);

        private static void DIFLogCallbackFunc(DIFXAPI_LOG EventType, Int32 ErrorCode, string EventDescription, IntPtr CallbackContext)
        {
            switch (EventType)
            {
                case DIFXAPI_LOG.DIFXAPI_SUCCESS:
                    Debug.WriteLine(String.Format("SUCCESS: {0}. Error code: {1}", EventDescription, ErrorCode));
                    break;
                case DIFXAPI_LOG.DIFXAPI_INFO:
                    Debug.WriteLine(String.Format("INFO: {0}. Error code: {1}", EventDescription, ErrorCode));
                    break;
                case DIFXAPI_LOG.DIFXAPI_WARNING:
                    Debug.WriteLine(String.Format("WARNING: {0}. Error code: {1}", EventDescription, ErrorCode));
                    break;
                case DIFXAPI_LOG.DIFXAPI_ERROR:
                    Debug.WriteLine(String.Format("ERROR: {0}. Error code: {1}", EventDescription, ErrorCode));
                    break;
            }
        }

        [DllImport("DIFxAPI.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SetDifxLogCallback(DIFLOGCALLBACK LogCallback, IntPtr CallbackContext);




        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct INSTALLERINFO
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pApplicationId;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDisplayName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pProductName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pMfgName;
        }

        const uint SP_COPY_DELETESOURCE = 0x0000001; // delete source file on successful copy
        const uint SP_COPY_REPLACEONLY = 0x0000002; // copy only if target file already present
        const uint SP_COPY_NEWER_OR_SAME = 0x0000004; // copy only if source newer than or same as target
        const uint SP_COPY_NEWER_ONLY = 0x0010000; // copy only if source file newer than target
        const uint SP_COPY_NOOVERWRITE = 0x0000008; // copy only if target doesn't exist
        const uint SP_COPY_NODECOMP = 0x0000010; // don't decompress source file while copying
        const uint SP_COPY_LANGUAGEAWARE = 0x0000020; // don't overwrite file of different language
        const uint SP_COPY_SOURCE_ABSOLUTE = 0x0000040; // SourceFile is a full source path
        const uint SP_COPY_SOURCEPATH_ABSOLUTE = 0x0000080; // SourcePathRoot is the full path
        const uint SP_COPY_FORCE_IN_USE = 0x0000200; // Force target- in-use behavior
        const uint SP_COPY_FORCE_NOOVERWRITE = 0x0001000; // like NOOVERWRITE but no callback nofitication
        const uint SP_COPY_FORCE_NEWER = 0x0002000; // like NEWER but no callback nofitication

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupInstallFile(
            IntPtr InfHandle,
            IntPtr InfContext,
            string SourceFile,
            string SourcePathRoot,
            string DestinationName,
            uint CopyStyle,
            IntPtr CopyMsgHandler,
            IntPtr Context);

        const Int32 INF_STYLE_OLDNT = 0x00000001;
        const Int32 INF_STYLE_WIN4 = 0x00000002;

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupOpenInfFile(
            [MarshalAs(UnmanagedType.LPTStr)] string FileName,
            [MarshalAs(UnmanagedType.LPTStr)] string InfClass,
            Int32 InfStyle, out uint ErrorLine);

        private const Int32 DRIVER_PACKAGE_FORCE = 0x00000004;
        private const Int32 DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT = 0x00000008;
        private const Int32 DRIVER_PACKAGE_LEGACY_MODE = 0x00000010;
        private const Int32 DRIVER_PACKAGE_SILENT = 0x00000002;
        private const Int32 DRIVER_PACKAGE_REPAIR = 0x00000001;

        /// <summary>
        /// The DriverPackageInstall function preinstalls a driver package in the DIFx driver store and then installs the driver in the system.
        /// </summary>
        /// <param name="DriverPackageInfPath">A pointer to a NULL-terminated string that supplies the fully qualified path of the driver package's INF file of the driver package to install. The INF file cannot be in the system INF file directory.</param>
        /// <param name="Flags">A bitwise OR of the values in the following table that control the installation operation.</param>
        /// <param name="pInstallerInfo">A pointer to a constant INSTALLERINFO structure that supplies information about an application that is associated with the driver that is being installed. This pointer is optional and can be NULL. If the pointer is NULL, the driver package is not associated with an application.</param>
        /// <param name="pNeedReboot">A pointer to a BOOL-typed variable. On return, DriverPackageInstall sets this variable to TRUE if a system restart is required to complete the installation. Otherwise, the function sets this variable to FALSE.</param>
        /// <returns></returns>
        [DllImport("DIFxAPI.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DriverPackageInstall(
            [MarshalAs(UnmanagedType.LPTStr)] string DriverPackageInfPath,
            Int32 Flags,
            IntPtr pInstallerInfo,
            out bool pNeedReboot);


        [DllImport("DIFxAPI.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DriverPackageInstall(
            [MarshalAs(UnmanagedType.LPTStr)] string DriverPackageInfPath,
            Int32 Flags,
            INSTALLERINFO pInstallerInfo,
            out bool pNeedReboot);

        #endregion
    }

    #region Enums

    public enum DriverStatus
    {
        UpToDate,
        OutOfDate,
        NotInstalled
    }

    #endregion
}
