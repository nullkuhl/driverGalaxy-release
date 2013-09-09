using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace DUSDK_for.NET
{
    public class DUSDKHandler
    {

        /// <summary>
        /// To receive the progress while scan and driver update process
        /// PUT IT ANYWHERE IN CLASS WHERE YOU WANT TO RECEIVES THE PROGRESS
        /// </summary>
        /// <param name="progressType">Value of progress enum - PROGRESS_TYPE</param>-
        /// <param name="data"></param>
        /// <param name="currentItemPos"></param>
        /// <param name="structSize"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public delegate bool progressCallback(PROGRESS_TYPE progressType, IntPtr data, int currentItemPos, int nTotalDriversToScan, int progress);



        /// <summary>
        /// Receives Download progres
        /// </summary>
        /// <param name="progressType"></param>
        /// <param name="iTotalDownloaded"></param>
        /// <param name="iTotalSize"></param>
        /// <param name="iRetCode"></param>
        /// <param name="iPercentageCompleted"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public delegate bool downloadProgressCallback(
            PROGRESS_TYPE progressType,
            long iTotalDownloaded,
            long iTotalSize,
            int iRetCode,
            int iPercentageCompleted,
            int progress
            );


        /// <summary>
        /// Call to scan and receive the driver updates of user system.
        /// </summary>
        /// <param name="pFunc"></param>
        /// <param name="szProductKey"></param>
        /// <param name="szAppDataLoc"></param>
        /// <param name="szTempLoc"></param>
        /// <param name="szRegistryLoc"></param>
        /// <returns></returns>
        [DllImport(@"stduhelper.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int scanDeviceDriversForUpdates(progressCallback pFunc,
            StringBuilder szProductKey,
            StringBuilder szAppDataLoc,
            StringBuilder szTempLoc,
            StringBuilder szRegistryLoc,
            uint dwFlags
            );





        [DllImport(@"stduhelper.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern bool cancelOperation(int operation);

        [DllImport(@"stduhelper.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int updateDeviceDriversEx(
            [In, Out] progressCallback pFunc,
            [In] StringBuilder szProductKey,
            [In] StringBuilder szAppDataLoc,
            [In] StringBuilder szTempLoc,
            [In] StringBuilder szRegistryLoc,
            [In] downloadProgressCallback pDownloadCallbackFunc,
            [In, Out] IntPtr Scandata,
            [In, Out] IntPtr DevicesToUpdates,
            [In] int driversSize,
            [In] StringBuilder szRestorePointName
            );



        /************************************************************************
         * OS Migration tool functions
        /************************************************************************/
        [DllImport(@"stduhelper.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int OSMT_scanDeviceDriversForUpdates(
            progressCallback pFunc,
            StringBuilder szProductKey,
            StringBuilder szAppDataLoc,
            StringBuilder szTempLoc,
            StringBuilder szRegistryLoc,
            uint dwFlags,
            int nOsId
            );







        [DllImport(@"stduhelper.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int OSMT_updateDeviceDriversEx(
            [In, Out] progressCallback pFunc,
            [In] StringBuilder szProductKey,
            [In] StringBuilder szAppDataLoc,
            [In] StringBuilder szTempLoc,
            [In] StringBuilder szRegistryLoc,
            [In] downloadProgressCallback pDownloadCallbackFunc,
            [In, Out] IntPtr Scandata,
            [In, Out] IntPtr DevicesToUpdates,
            [In] int driversSize,
            [In] StringBuilder szRestorePointName,
            [In] int nOsId,
            [In] uint nUpdateFlag,
            [In] StringBuilder szArchiveLoc /* value to pass if want to store downloads or if want restore from archive */
            );



        [DllImport(@"stduhelper.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int TestParamPassed([In] IntPtr pUserDrivers, [In] IntPtr pDriversToUpdate);


        public static int DUSDK_TestParamPassed([In] IntPtr pUserDrivers, [In] IntPtr pDriversToUpdate)
        {
            return TestParamPassed(pUserDrivers, pDriversToUpdate);
        }


        public static int DUSDK_scanDeviceDriversForUpdates(progressCallback pFunc, StringBuilder szProductKey, StringBuilder szAppDataLoc, StringBuilder szTempLoc, StringBuilder szRegistryLoc, uint dwFlags)
        {
            return scanDeviceDriversForUpdates(pFunc, szProductKey, szAppDataLoc, szTempLoc, szRegistryLoc, dwFlags);
        }

        public static int DUSDK_scanDeviceDriversForUpdatesForOS(progressCallback pFunc, StringBuilder szProductKey, StringBuilder szAppDataLoc, StringBuilder szTempLoc, StringBuilder szRegistryLoc, uint dwFlags, int nOsId)
        {
            return OSMT_scanDeviceDriversForUpdates(pFunc, szProductKey, szAppDataLoc, szTempLoc, szRegistryLoc, dwFlags, nOsId);
        }

        public static bool DUSDK_cancelOperation(int operation)
        {
            return cancelOperation(operation);
        }

        #region Enum and Structures

        //
        // Input for scan function
        //
        public enum SCAN_FLAGS
        {
            SCAN_DEVICES_PRESENT, /* if supplied to scan function, only connected devices are checked for updates available */
            SCAN_DEVICES_ALL /* if supplied to scan function, all connected and unplugged devices are checked for updates available */
        }

        /// <summary>
        /// various constants used in SDK
        /// Cancel events
        /// Registration status
        /// </summary>
        public static partial class DefineConstants
        {
            public const int LOG_SEVERITY_DEBUG = 0x01;
            public const int LOG_SEVERITY_INFO = 0x02;
            public const int LOG_SEVERITY_WARNING = 0x03;
            public const int LOG_SEVERITY_ERROR = 0x04;
            public const int LOG_SEVERITY_FATAL = 0x05;
            public const int NEED_RESTART = 0x06;
            public const int NOT_UNINSTALL = 0x07;
            public const int ACCESS_DENIED = 0x08;
            public const int INVALID_NAME = 0x09;
            public const int SERVICE_EXISTS = 0x10;
            public const int DUP_NAME = 0x11;
            public const int CANCEL_SCAN = 0x12; // call to stop scan
            public const int CANCEL_UPDATE_DRIVERS = 0x13; // call to stop driver update process
            public const int CANCEL_BACKUP = 0x14;
            public const int CANCEL_LIST_DRIVER_BACKUPS = 0x15;
            public const int CANCEL_LOAD_DRIVER_BACKUP = 0x16;
            public const int CANCEL_RESTORE = 0x17;
            public const int CANCEL_LIST_DEVICES_IN_EXCLUSION = 0x18;
            public const int CANCEL_ADD_DEVICE_TO_EXCLUSION = 0x19;
            public const int CANCEL_REMOVEDEVICE_FROM_EXCLUSION = 0x20;
            public const int CANCEL_REMOVEALL_DEVICES_FROM_EXCLUSION = 0x21;
            public const int CANCEL_DELETE_OLD_DRIVERS = 0x22;
            public const int CANCEL_UNINSTALL = 0x23;
            public const int CANCEL_UNKNOWN_SCAN = 0x24;
            public const int CANCEL_BACKUP_NON_PNP = 0x25;
            public const int CANCEL_XML_UPLOAD = 0x26;
            public const int CANCEL_INSTALL = 0x27;
            public const int DEFAULT = 0x28;
            public const int SUCCESS = 0x50;
            public const int NEED_DOWNLOAD = 0x51;
            public const int FAIL = 0x52;
            public const int INCORRECT_PARAM = 0x53;
            public const int OTHER_DRIVER_INSTALLATION_IN_PROGRESS = 0x54;
            public const int FAIL_NETCONNECTION = 0x55;
            public const int FAIL_BACKUP_NO_FILES_TO_BACKUP = 0x56;
            public const int FAIL_LIST_BACKUP_CURRPT_ZIP_FILE = 0x57;
            public const int FAIL_LIST_BACKUP_NOT_EXIST = 0x58;

            /// <summary>
            /// Registration status
            /// </summary>
            public const int REG_STATUS_EVAL_VERSION_EXPIRED = 0x80;
            public const int REG_STATUS_SUBSCRIPTION_EXPIRED = 0x81;
            public const int REG_STATUS_SUBSCRIPTION_ACTIVATION_FAILED = 0x82;
            public const int REG_STATUS_EVAL_PERIOD_REMAINING = 0x83;
            public const int REG_STATUS_SUBSCRIPTION_REMAINING = 0x84;
            public const int REG_STATUS_PRODUCT_KEY_INVALID = 0x85;
        }


        /*
        PROGRESS_TYPE - enum

        This enum specifies the state of callback data returned.
        These are only available while scan process.

        */
        /// <summary>
        /// Defines the progress type while various process of DUSDK
        /// </summary>
        public enum PROGRESS_TYPE
        {

            PROGRESS_SCANNING = 0x100,
            PROGRESS_SCANNED,
            PROGRESS_RETRIEVING_UPDATES_DATA,
            PROGRESS_RETRIEVING_UPDATES_FAILED_INET,
            PROGRESS_FILTERING_UPDATES,

            PROGRESS_BACKUP_IN_PROGRESS,
            PROGRESS_BACKUP_COMPRESSING,
            PROGRESS_BACKUP_COMPRESSED,
            PROGRESS_BACKUP_DELETING_TEMP_LOC,
            PROGRESS_BACKUP_DELETED_TEMP_LOC,
            PROGRESS_UNKNOWN,
            PROGRESS_SEARCHING_FOR_ANY_DOWNLOAD_REMAINING,
            PROGRESS_CREATING_RESTORE_POINT,
            PROGRESS_CREATING_CURRENT_DRIVER_BACKUP,
            PROGRESS_DOWNLOAD_INFO_RETRIEVING,
            PROGRESS_DOWNLOAD_INFO_RETRIEVED,				/* Starting to retrieve the the download info */
            PROGRESS_DOWNLOADING,							/* progress of driver downloading */
            PROGRESS_DOWNLOADING_OVERALL,					/* will provide the overall progress of downloading according to the */
            PROGRESS_DOWNLOAD_STARTED_FOR_ALL,
            PROGRESS_DOWNLOAD_END_FOR_ALL,
            PROGRESS_DOWNLOAD_STARTED_FOR_SINGLE,
            PROGRESS_DOWNLOAD_END_FOR_SINGLE,
            PROGRESS_DOWNLOAD_END_FOR_SINGLE_INET_ERROR,
            PROGRESS_DOWNLOAD_END_FOR_SINGLE_UNREG,
            PROGRESS_UPDATE_CREATING_RESTORE_POINT,
            PROGRESS_UPDATE_CREATING_BACKUP,
            PROGRESS_UPDATE_STARTED_FOR_ALL,
            PROGRESS_UPDATE_END_FOR_ALL,
            PROGRESS_UPDATE_STARTED_FOR_SINGLE,
            PROGRESS_UPDATE_END_FOR_SINGLE,					/* Driver Update process successful */
            PROGRESS_UPDATE_SUCCESSFUL,						/* Driver Update process successful */
            PROGRESS_UPDATE_FAILED,							/* Driver Update Failed */
            PROGRESS_UPDATE,								/* Driver Update Progress */
            PROGRESS_UPDATE_FAILED_INVALID_INSTALLER_PATH,	/* Invalid file Path of setup to install */
            PROGRESS_UPDATE_MANUAL_INSTALL_FILE_CATEGORY,	/* To Install this setup file, Click on help button. */
            PROGRESS_UPDATE_CORRUPT_DOWNLOAD,				/*Unable to Launch the setup file, the setup file is corrupt, please check your internet connection and redownload the setup.*/
            PROGRESS_LIST_DRIVER_BACKUP,
            PROGRESS_LOAD_DRIVER_BACKUP,
            PROGRESS_LIST_EXCLUSION,
            PROGRESS_ADD_TO_EXCLUSION,
            PROGRESS_REMOVE_FROM_EXCLUSION,
            PROGRESS_REMOVE_ALL_FROM_EXCLUSION,
            PROGRESS_RESTORE,

            /* Size of structure */
            PROGRESS_SIZE_SCAN_DATA,						/* callback returns the size of DriverData struct while scan */
            PROGRESS_SIZE_BACKUP_DATA,						/* callback returns the size of BackupData struct while list backup */
            PROGRESS_SIZE_LOAD_BACKUP_DATA,					/* callback returns the size of BackupData struct while load backup */
            PROGRESS_SIZE_EXCLUSION_DATA,					/* callback returns the size of BackupData struct while list exclusion */



            /* Restore archive progress in Os Migration Tool */
            PROGRESS_OSMT_ARCHIVE_VERIFYING_DISK_SPACE,		/* checking disk space is available to create the archive */
            PROGRESS_OSMT_ARCHIVE_UNSUFFICIENT_DISK_SPACE,	/* disk space not available */
            PROGRESS_OSMT_ARCHIVE_STARTED,
            PROGRESS_OSMT_ARCHIVE_CREATING,
            PROGRESS_OSMT_ARCHIVE_FAILED_NO_INSTALLER,		/* failed as no driver setup found to archive */
            PROGRESS_OSMT_ARCHIVE_COMPLETED,
            PROGRESS_OSMT_RST_INVALID_ARCHIVE_PATH,
            PROGRESS_OSMT_RST_CORRUPT_ARCHIVE,
            PROGRESS_OSMT_RST_EXTRACTING_ARCHIVE,			/* when archive extraction is started */
            PROGRESS_OSMT_RST_EXTRACTED_ARCHIVE,			/* when archive extraction is completed */
            PROGRESS_OSMT_RST_MATCHING_PARAMS,				/* matching archive is of current system or other */
            PROGRESS_OSMT_RST_MATCHED_PARAMS,				/* all params matched */
            PROGRESS_OSMT_RST_FAILED_OS_NOTMATCHED,			/* archive targeted OS and current OS are not matched */
            PROGRESS_OSMT_RST_FAILED_BIOS_NOTMATCHED,		/* Current system BIOS info is different from the source */
            PROGRESS_OSMT_RST_FAILED_BOARD_NOTMATCHED,		/* Current system Base board info is different from the source */
            PROGRESS_OSMT_RST_FAILED_MODEL_NOTMATCHED,		/* Current system computer model info is different from the source */
            PROGRESS_OSMT_RST_RESTORE_STARTED,				/* All params are matched and now restore has started */
            PROGRESS_OSMT_RST_RESTORE_COMPLETED,			/* Restore has been completed */

            PROGRESS_FILTERED_UPDATES,
            PROGRESS_RETRIEVED_UPDATES_DATA,
            PROGRESS_OSMT_RST_LIST_STARTED,                 /* Retrieval of list of drivers of operating system migration archive started */
            PROGRESS_OSMT_RST_LIST_ITEM,                    /* list of driver are passed to callback, one by one */
            PROGRESS_OSMT_RST_LIST_ITEM_UPDATE,             /* information of driver updates are passed to callback, one by one */
            PROGRESS_OSMT_RST_LIST_COMPLETED,               /* Retrieval of list of drivers of operating system migration archive completed */
            PROGRESS_OSMT_RST_LIST_FAILED                   /* Retrieval of list of drivers of operating system migration archive failed */
        }




        public enum UPDATE_FLAGS
        {
            UPDATE_FLAG_DOWNLOAD_ONLY = 1,
            UPDATE_FLAG_ARCHIVE_DOWNLOADS_ONLY = 2,
            UPDATE_FLAG_BACKUP_BEFORE_UPDATE_ONLY = 4,
            UPDATE_FLAG_CREATE_RESTOREPOINT_ONLY = 8,
            UPDATE_FLAG_UPDATE_DRIVERS_ONLY = 16,
            UPDATE_FLAG_RESTORE_ARCHIVE_ONLY = 32,
            UPDATE_FLAG_ALL = 64,
            UPDATE_FLAG_LIST_ARCHIVE_ONLY = 128,                /* pass this to get the list of drivers of archive */
            UPDATE_FLAG_RESTORE_SELECTED_ARCHIVE_ONLY = 256,    /* pass this to restore few drivers from archive */
        };







        #endregion


    }


    /// <summary>
    /// Reference: http://msdn.microsoft.com/en-us/library/ac7ay120%28v=VS.90%29.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DriverData
    {





        /// <summary>
        /// Installed driver name shown in device manager
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string driverName;

        /// <summary>
        /// Inf Path of currently installed system driver
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string location;

        /// <summary>
        /// current driver version
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string version;

        /// <summary>
        /// category of driver like Display Adapter, Sound...
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string category;

        /// <summary>
        /// any further description if have
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string description;

        /// <summary>
        /// provider name
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string publisher;

        /// <summary>
        /// manufacturer name
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string manufacturer;

        /// <summary>
        /// hardware id of driver
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string hardwareId; // 

        /// <summary>
        /// driver node guid path
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string guidPath;

        /// <summary>
        /// date of installed driver
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string date;

        /// <summary>
        /// service name of installed driver
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string serviceName;

        /// <summary>
        /// Is installed driver is signed or not
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string m_bIsSigned; //

        /// <summary>
        /// Inf path of update driver
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string installInf;

        /// <summary>
        /// Used while driver update process
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string md5;

        /// <summary>
        /// Not used
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string HardwareCode;

        /// <summary>
        /// Matching Hardware Id of current installed system driver
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string matchingId;

        /// <summary>
        /// Not used
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string libURL;

        /// <summary>
        /// Not used
        /// </summary>
        public Int32 fileCount;

        /// <summary>
        /// Not used
        /// </summary>
        public Int32 isChipsetDrv;

        /// <summary>
        /// Used while driver update process
        /// </summary>
        public Int32 isInstall;

        /// <summary>
        /// Used while driver update process
        /// </summary>
        public Int64 downloadid;

        /// <summary>
        /// Used while driver update process - value from DRIVER_UPDATE_SEVERITY_LEVEL
        /// </summary>
        public UInt16 nUpdateSeverity;

        /// <summary>
        /// Used while driver update process
        /// </summary>
        public UInt16 iRiskLevel;

        /// <summary>
        /// Not used
        /// </summary>
        public UInt16 nDbCategoryID;

        /// <summary>
        /// Age of driver
        /// </summary>
        public UInt32 nDriverAge;

        /// <summary>
        /// Used while driver update process
        /// </summary>
        public Int32 bIsAutoExcluded;

        /// <summary>
        /// Used while driver update process
        /// </summary>
        public Int32 bIsManuallyExcluded;

        /// <summary>
        /// whether driver is unknown 
        /// </summary>
        public Int32 bIsUnknownDevice;

        /// <summary>
        /// status of a device instance from its device node 
        /// 0 - good
        /// 13 - unplugged
        /// </summary>
        public UInt32 DevInst;

        /// <summary>
        /// Matching Hardware Id
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string MatchingDeviceID;

        /// <summary>
        /// String Array of hardware ids of device (*used to search for updates)
        /// </summary>
        public IntPtr HardwareIDs;

        /// <summary>
        ///  String Array of compatible ids  of device  (*used to search for updates)
        /// </summary>
        public IntPtr CompatibleIDs;


        /// <summary>
        /// String Array of all hardware ids of device
        /// </summary>
        public IntPtr HardwareIDsAll;


        /// <summary>
        /// String Array of compatible ids  of device
        /// </summary>
        public IntPtr CompatibleIDsAll;



        /// <summary>
        /// Rank of installed driver
        /// </summary>
        public UInt32 ulRank;

        /// <summary>
        /// Any device error shown for device            
        /// </summary>
        public UInt32 ulDeviceErrorCode;

        /// <summary>
        /// Is Current driver signed
        /// </summary>
        public Int32 bIsINFSigned;

        /// <summary>
        /// FeatureScore of current installed driver
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string FeatureScore;

        /// <summary>
        /// Index of current hardware id from the list of compatible ids, if matched
        /// </summary>
        public UInt32/*uint*/ CompatibleIdIndex;

        /// <summary>
        /// Driver Date of currently installed driver    (Low date part)
        /// </summary>
        public UInt32/*uint*/ dwLowDateTime;

        /// <summary>
        /// Driver Date of currently installed driver    (High date part)
        /// </summary>
        public UInt32/*uint*/ dwHighDateTime;

        public Int64 ulDateTimeQuadPart;

        /// <summary>
        /// Not used or used while driver update process
        /// </summary>
        public UInt16/*short*/ nDriverId;
        public UInt16/*short*/ nIconId;
        public UInt16/*short*/ nGUIRowId;
        public UInt16/*short*/ nIsItemSelectedForUpdate;
        public UInt16/*short*/ nDriverUpdateStateFlag;


        /// <summary>
        /// flags used while update driver process
        /// </summary>
        public UInt16/*short*/ InstallSafetyFlag;
        public UInt16/*short*/ InstallFlag;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string SetupLaunchPath;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string SetupLaunchParam;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string enumLoc;

        public IntPtr UpdateDriver;


    }  


    public enum DU_SUPPORTED_OS_NAMES
    {
        [Description("Default Os")]
        DEFAULT_OS = -1,
        [Description("Windows XP 32 bit")]
        WIN_XP_INTEL = 0,		// Windows XP 32 bit
        [Description("Windows XP 64 bit")]
        WIN_XP_AMD4 = 1,		// Windows XP 64 bit
        [Description("Windows Vista 32 bit")]
        WIN_VISTA_INTEL = 2,	// Windows Vista 32 bit
        [Description("Windows Vista 64 bit")]
        WIN_VISTA_AMD64 = 3,	// Windows Vista 64 bit
        [Description("Windows 7 32 bit")]
        WIN_7_INTEL = 4,		// Windows 7 32 bit
        [Description("Windows 7 64 bit")]
        WIN_7_AMD64 = 5,		// Windows 7 64 bit
        [Description("Windows 8 32 bit")]
        WIN_8_INTEL = 6,		// Windows 8 32 bit
        [Description("Windows 8 64 bit")]
        WIN_8_AMD64 = 7,		// Windows 8 64 bit
    };
}
