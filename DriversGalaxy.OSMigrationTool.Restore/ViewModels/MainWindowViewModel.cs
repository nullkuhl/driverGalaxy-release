using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using DriversGalaxy.Models;
using DriversGalaxy.OSMigrationTool.Restore.Infrastructure;
using DriversGalaxy.OSMigrationTool.Restore.Models;
using DriversGalaxy.Utils;
using Ionic.Zip;
using MessageBoxUtils;
using DUSDK_for.NET;
using System.Text;

namespace DriversGalaxy.OSMigrationTool.Restore.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Constructors

        public MainWindowViewModel()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;

            #region Commands initialization

            checkDevicesGroupCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDevicesGroup
            };

            checkDeviceCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDevice
            };

            installCommand = new SimpleCommand
            {
                ExecuteDelegate = x => Install()
            };

            cancelInstallCommand = new SimpleCommand
            {
                ExecuteDelegate = x => CancelInstall()
            };

            closeCommand = new SimpleCommand
            {
                ExecuteDelegate = x => Close()
            };

            #endregion
        }

        #endregion

        #region Commands

        ICommand checkDevicesGroupCommand;
        public ICommand CheckDevicesGroupCommand
        {
            get { return checkDevicesGroupCommand; }
        }
        void CheckDevicesGroup(object selectedDestinationOSDevicesGroup)
        {
            DestinationOSDevicesGroup destinationOSDevicesGroup = GroupedDevices.Where(d => d.DeviceClass == (string)selectedDestinationOSDevicesGroup).FirstOrDefault();
            if (destinationOSDevicesGroup != null)
            {
                foreach (DestinationOSDeviceInfo item in destinationOSDevicesGroup.Drivers)
                {
                    item.SelectedForInstall = destinationOSDevicesGroup.GroupChecked;
                }
            }
        }

        ICommand checkDeviceCommand;
        public ICommand CheckDeviceCommand
        {
            get { return checkDeviceCommand; }
        }
        void CheckDevice(object selectedDevice)
        {
            DestinationOSDeviceInfo device = allDrivers.DownloadedDestinationOSDrivers.Where(d => d.Id == (string)selectedDevice).FirstOrDefault();
            if (device != null)
            {
                DestinationOSDevicesGroup destinationOSDevicesGroup = GroupedDevices.Where(d => d.DeviceClass == device.DeviceClass).FirstOrDefault();
                if (destinationOSDevicesGroup != null && destinationOSDevicesGroup.Drivers.Where(d => d.SelectedForInstall).Count() == 0)
                {
                    destinationOSDevicesGroup.GroupChecked = false;
                }
                else
                {
                    if (destinationOSDevicesGroup != null && destinationOSDevicesGroup.Drivers.Where(d => d.SelectedForInstall == false).Count() == 0)
                    {
                        destinationOSDevicesGroup.GroupChecked = true;
                    }
                }
            }
        }

        ICommand installCommand;
        public ICommand InstallCommand
        {
            get { return installCommand; }
        }
        void Install()
        {
            RunInstall();
        }

        ICommand cancelInstallCommand;
        public ICommand CancelInstallCommand
        {
            get { return cancelInstallCommand; }
        }
        void CancelInstall()
        {
            RunCancelInstall();
        }

        ICommand closeCommand;
        public ICommand CloseCommand
        {
            get { return closeCommand; }
        }
        void Close()
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Properties

        public string ZipToUnpack;
        Dispatcher CurrentDispatcher;
        string tempDirectory;
        DriverUtils driverUtils = new DriverUtils();
        bool ABORT;
        BackgroundWorker bgInstall;
        bool bDriverInstallIsGoingOn = false;
        int DriverIndex;

        StringBuilder szAppDataLoc;
        StringBuilder szTempLoc;
        StringBuilder szProductKey = new StringBuilder().Append("46288-11183-23588");
        StringBuilder szRegistryLoc = new StringBuilder().Append("Software\\DriversGalaxy");
        StringBuilder szRestorePointName = new StringBuilder().Append("DriversGalaxy");

        List<KeyValuePair<object, object>> _OsList;
        public List<KeyValuePair<object, object>> OsList
        {
            get
            {
                if (_OsList == null)
                {
                    _OsList = new List<KeyValuePair<object, object>>();
                    foreach (DU_SUPPORTED_OS_NAMES supportedOs in EnumExtensions.EnumToList<DU_SUPPORTED_OS_NAMES>())
                    {
                        OsList.Add(new KeyValuePair<object, object>(supportedOs.DescriptionAttr(), supportedOs));
                    }
                }
                return _OsList;
            }
        }

        InstallStatus status = InstallStatus.NotStarted;
        public InstallStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        string installStatusTitle;
        public string InstallStatusTitle
        {
            get
            {
                return installStatusTitle;
            }
            set
            {
                installStatusTitle = value;
                OnPropertyChanged("InstallStatusTitle");
            }
        }

        string installStatusText;
        public string InstallStatusText
        {
            get
            {
                return installStatusText;
            }
            set
            {
                installStatusText = value;
                OnPropertyChanged("InstallStatusText");
            }
        }

        string installFinishTitle;
        public string InstallFinishTitle
        {
            get
            {
                return installFinishTitle;
            }
            set
            {
                installFinishTitle = value;
                OnPropertyChanged("InstallFinishTitle");
            }
        }

        string panelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("SelectDrivers");
        public string PanelScanHeader
        {
            get
            {
                return panelScanHeader;
            }
            set
            {
                panelScanHeader = value;
                OnPropertyChanged("PanelScanHeader");
            }
        }

        int progress;
        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        DestinationOSDevices allDrivers = new DestinationOSDevices();
        List<DestinationOSDeviceInfo> driversToInstall;

        ObservableCollection<DestinationOSDevicesGroup> groupedDevices = new ObservableCollection<DestinationOSDevicesGroup>();
        public ObservableCollection<DestinationOSDevicesGroup> GroupedDevices
        {
            get
            {
                return groupedDevices;
            }
            set
            {
                groupedDevices = value;
                OnPropertyChanged("GroupedDevices");
            }
        }

        ICollectionView orderedDeviceGroups;
        public ICollectionView OrderedDeviceGroups
        {
            get
            {
                return orderedDeviceGroups;
            }
            set
            {
                orderedDeviceGroups = value;
                OnPropertyChanged("OrderedDeviceGroups");
            }
        }

        #endregion

        #region Private methods

        public void UnZipDriverData()
        {
            tempDirectory = String.Format(@"{0}\Drivers Galaxy\Temp", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            try
            {
                Directory.Delete(tempDirectory);
                Directory.CreateDirectory(tempDirectory);
            }
            catch { }

            if (File.Exists(ZipToUnpack))
            {
                string unpackDirectory = tempDirectory;
                using (ZipFile zipFile = ZipFile.Read(ZipToUnpack))
                {
                    // here we extract every entry
                    foreach (ZipEntry e in zipFile)
                    {
                        e.Extract(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            else
            {
                WPFMessageBox.Show(WPFLocalizeExtensionHelpers.GetUIString("DriverDataNotFound"), WPFLocalizeExtensionHelpers.GetUIString("FileNotFound"), WPFMessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        /// <summary>
        /// Reads driver data from the XML file
        /// </summary>
        public void ReadDriverDataFromXML()
        {
            string driverDataXMLFilePath = String.Format(@"{0}\DriverData.xml", tempDirectory);
            if (File.Exists(driverDataXMLFilePath))
            {
                try
                {
                    var xs = new XmlSerializer(typeof(DestinationOSDevices));
                    using (var rd = new StreamReader(driverDataXMLFilePath))
                    {
                        allDrivers = xs.Deserialize(rd) as DestinationOSDevices;
                    }

                    UpdateGroupedDevices();
                }
                catch { }
            }
            else
            {
                WPFMessageBox.Show(WPFLocalizeExtensionHelpers.GetUIString("DriverDataXMLNotFound"), WPFLocalizeExtensionHelpers.GetUIString("FileNotFound"), WPFMessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        /// <summary>
        /// Analyzes the <c>Environment.OSVersion</c> value and returns a string which represents the current OS info
        /// </summary>
        /// <returns>A string which represents the current OS info</returns>
        string GetOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        operatingSystem = vs.Minor == 0 ? "2000" : "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        if (vs.Minor == 1)
                            operatingSystem = "7";
                        if (vs.Minor == 2)
                            operatingSystem = "8";
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;

                //Append the OS architecture.  i.e. "Windows XP 32-bit"
                operatingSystem += " " + GetOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }

        #region Detecting Windows 64 bit platform

        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public extern static IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public extern static IntPtr GetProcAddress(IntPtr hwnd, string procedureName);

        private delegate bool IsWow64ProcessDelegate([In] IntPtr handle, [Out] out bool isWow64Process);

        public static int GetOSArchitecture()
        {
            if (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()))
            {
                return 64;
            }
            return 32;
        }

        private static IsWow64ProcessDelegate GetIsWow64ProcessDelegate()
        {
            IntPtr handle = LoadLibrary("kernel32");

            if (handle != IntPtr.Zero)
            {
                IntPtr fnPtr = GetProcAddress(handle, "IsWow64Process");

                if (fnPtr != IntPtr.Zero)
                {
                    return (IsWow64ProcessDelegate)Marshal.GetDelegateForFunctionPointer(fnPtr, typeof(IsWow64ProcessDelegate));
                }
            }

            return null;
        }

        private static bool Is32BitProcessOn64BitProcessor()
        {
            IsWow64ProcessDelegate fnDelegate = GetIsWow64ProcessDelegate();

            if (fnDelegate == null)
            {
                return false;
            }

            bool isWow64;
            bool retVal = fnDelegate.Invoke(Process.GetCurrentProcess().Handle, out isWow64);

            if (retVal == false)
            {
                return false;
            }

            return isWow64;
        }

        #endregion

        void UpdateGroupedDevices()
        {
            foreach (DestinationOSDeviceInfo driver in allDrivers.DownloadedDestinationOSDrivers)
            {
                DestinationOSDevicesGroup destinationOSDevicesGroup = GroupedDevices.Where(g => g.DeviceClass == driver.DeviceClass).FirstOrDefault();
                if (destinationOSDevicesGroup == null)
                {
                    GroupedDevices.Add(new DestinationOSDevicesGroup(driver.DeviceClass, driver.DeviceClassName, driver.DeviceClassImageSmall, new List<DestinationOSDeviceInfo> { driver }));
                }
                else
                {
                    destinationOSDevicesGroup.Drivers.Add(driver);
                }
            }

            OrderedDeviceGroups = CollectionViewSource.GetDefaultView(GroupedDevices);
            OrderedDeviceGroups.SortDescriptions.Clear();
            OrderedDeviceGroups.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
            OrderedDeviceGroups.Refresh();
        }

        void RunInstall()
        {
            driversToInstall = allDrivers.DownloadedDestinationOSDrivers.Where(d => d.SelectedForInstall).ToList();
            if (driversToInstall.Count == 0)
            {
                WPFMessageBox.Show(WPFLocalizeExtensionHelpers.GetUIString("SelectAtLeastOneDriver"), WPFLocalizeExtensionHelpers.GetUIString("SelectDrivers"), WPFMessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Progress = 0;
            InstallStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("InstallingDrivers");
            InstallStatusText = string.Empty;
            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("InstallingDrivers");
            Status = InstallStatus.Started;
            DriverIndex = 0;

            if (ABORT)
            {
                ABORT = false;
                return;
            }

            bgInstall = new BackgroundWorker();
            bgInstall.DoWork += DriverInstall;
            bgInstall.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgInstall_RunWorkerCompleted);
            bgInstall.WorkerSupportsCancellation = true;
            bgInstall.RunWorkerAsync();
        }

        void bgInstall_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!ABORT)
            {
                if (CurrentDispatcher.Thread != null)
                {
                    CurrentDispatcher.BeginInvoke((Action)(() =>
                    {
                        InstallFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversInstalled"), driversToInstall.Count());
                        Status = InstallStatus.Finished;
                        InstallStatusTitle = InstallFinishTitle;
                        InstallStatusText = "";
                        DriverIndex = 0;
                    }));
                }
            }
        }

        void DriverInstall(object sender, DoWorkEventArgs e)
        {
            bDriverInstallIsGoingOn = true;
            int nOsis = allDrivers.OS;
            try
            {
                int retval = DUSDKHandler.OSMT_updateDeviceDriversEx(
                    progressCallback,
                    szProductKey,
                    szAppDataLoc,
                    szTempLoc,
                    szRegistryLoc,
                    downloadProgressCallback,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    0,
                    szRestorePointName,
                    allDrivers.OS,
                    (uint)DUSDKHandler.UPDATE_FLAGS.UPDATE_FLAG_RESTORE_ARCHIVE_ONLY,
                    new StringBuilder(ZipToUnpack)
                    );

                //if (retval >= (int)DUSDKHandler.PROGRESS_TYPE.PROGRESS_SCANNING && retval < (int)DUSDKHandler.PROGRESS_TYPE.PROGRESS_RETRIEVED_UPDATES_DATA)
                //{
                //    MessageBox.Show(((DUSDKHandler.PROGRESS_TYPE)(retval)).ToString());
                //}
                //else
                //{
                //    MessageBox.Show(retval.ToString());
                //}
            }
            catch (Exception)
            {
            }
            finally
            {
                bDriverInstallIsGoingOn = false;
            }
        }

        public bool downloadProgressCallback(
          DUSDKHandler.PROGRESS_TYPE progressType,
          long iTotalDownloaded,
          long iTotalSize,
          int iRetCode,
          int iPercentageCompleted,
          int progress
          )
        {
            return true;
        }



        /// <summary>-
        /// receives the progress of driver scan
        /// </summary>
        /// <param name="progressType"></param>
        /// <param name="data"></param>
        /// <param name="currentItemPos"></param>
        /// <param name="nTotalDriversToScan"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public bool progressCallback(DUSDKHandler.PROGRESS_TYPE progressType, IntPtr data, int currentItemPos, int nTotalDriversToScan, int progress)
        {
            // here we will get the progress of driver scan.
            DriverData? dd = null;
            string category = string.Empty;

            if (data != IntPtr.Zero)
            {
                dd = (DriverData)Marshal.PtrToStructure(
                            (IntPtr)(data.ToInt64() + currentItemPos * Marshal.SizeOf(typeof(DriverData))),
                            typeof(DriverData)
                            );

            }

            switch (progressType)
            {
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_UPDATE_STARTED_FOR_SINGLE:
                    {
                        if (data != IntPtr.Zero && dd.HasValue)
                        {
                            if (CurrentDispatcher.Thread != null)
                            {
                                CurrentDispatcher.BeginInvoke((Action)(() =>
                                {
                                    // DownloadedDrivers.Add(new DownloadingDriverModel(WPFLocalizeExtensionHelpers.GetUIString("InstallingDriver"), devicesForUpdate[currentItemPos]));
                                    //ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("InstallingDriver");
                                    // ScanStatusText = devicesForUpdate[currentItemPos].DeviceName;                                    
                                }));
                            }
                        }
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_UPDATE_END_FOR_SINGLE:
                    {
                        if (data != IntPtr.Zero && dd.HasValue)
                        {
                            if (CurrentDispatcher.Thread != null)
                            {
                                CurrentDispatcher.BeginInvoke((Action)(() =>
                                {
                                    InstallFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversInstalled"), driversToInstall.Count());
                                    Status = InstallStatus.Finished;
                                    InstallStatusTitle = InstallFinishTitle;
                                    InstallStatusText = "";
                                    DriverIndex = 0;
                                }));
                            }
                        }
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_UPDATE_SUCCESSFUL:
                    {
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_UPDATE_FAILED:
                    {
                        if (data != IntPtr.Zero && dd.HasValue)
                        {
                            string driverName = ((DriverData)dd).driverName;
                            if (driverName.Length > 37)
                                driverName = driverName.Substring(0, 34) + "...";
                            if (CurrentDispatcher.Thread != null)
                            {
                                CurrentDispatcher.BeginInvoke((Action)(() =>
                                {
                                    //          ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("UpdateFailed") + " " + driverName;
                                    //        ScanStatusText = string.Empty;
                                }));
                            }
                        }
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_UPDATE_END_FOR_ALL:
                    if (CurrentDispatcher.Thread != null)
                    {
                        CurrentDispatcher.BeginInvoke((Action)(() =>
                        {
                            CurrentDispatcher.BeginInvoke((Action)(() =>
                            {
                                //ScanFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversDownloaded"), DevicesForDestinationOS.Count());
                                //Status = ScanStatus.DownloadFinished;
                                //driverDownloading = false;
                                //ScanStatusTitle = ScanFinishTitle;
                                //ScanStatusText = string.Empty;
                                //driverIndex = 0; 
                                Progress = 0;
                            }));
                        }));
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_DOWNLOAD_END_FOR_SINGLE_INET_ERROR:
                    {
                        // download failed because of Internet Error
                    }
                    break;

                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_OSMT_RST_FAILED_OS_NOTMATCHED:
                    //OS aren't match
                    if (CurrentDispatcher.Thread != null)
                    {
                        CurrentDispatcher.BeginInvoke((Action)(() =>
                        {
                            CurrentDispatcher.BeginInvoke((Action)(() =>
                            {
                                string OSInfo = GetOSInfo();
                                string OSName = OsList[allDrivers.OS].Key.ToString();//((DU_SUPPORTED_OS_NAMES)allDrivers.OS).ToString();
                                WPFMessageBox.Show(
                                       String.Format(WPFLocalizeExtensionHelpers.GetUIString("OSMismatchText"), OSName, OSInfo),
                                       WPFLocalizeExtensionHelpers.GetUIString("OSMismatch"),
                                       WPFMessageBoxButton.OK,
                                       MessageBoxImage.Error
                                   );
                                Close();
                            }));
                        }));
                    }
                    break;
                default:
                    break;
            }

            if (progress > 0)
            {
                if (CurrentDispatcher.Thread != null)
                {
                    CurrentDispatcher.BeginInvoke((Action)(() =>
                    {
                        Progress = progress;
                    }));
                }
            }

            return true;
        }


        void RunCancelInstall()
        {
            ABORT = true;
            if (bgInstall.IsBusy)
                bgInstall.CancelAsync();
            CancelOperation();

            InstallFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversInstalled"), DriverIndex + 1);
            Status = InstallStatus.NotStarted;
            InstallStatusTitle = InstallFinishTitle;
            InstallStatusText = string.Empty;
            Progress = 0;
        }

        /// <summary>
        /// Will call to cancel driver scan or driver update function
        /// </summary>
        /// <returns></returns>
        public bool CancelOperation()
        {
            bool retval = false;
            if (bDriverInstallIsGoingOn)
            {
                int CancelFn = DUSDKHandler.DefineConstants.CANCEL_INSTALL;
                try
                {
                    retval = DUSDKHandler.DUSDK_cancelOperation(CancelFn);
                }
                catch (Exception)
                {
                }
            }

            return retval;
        }


        #endregion

        #region INotifyPropertyChanged

        void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
