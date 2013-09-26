using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using DriversGalaxy.Infrastructure;
using DriversGalaxy.Models;
using DriversGalaxy.OSMigrationTool.Backup.Infrastructure;
using DriversGalaxy.OSMigrationTool.Backup.Models;
using DriversGalaxy.Utils;
using DUSDK_for.NET;
using FreemiumUtil;
using Ionic.Zip;
using MessageBoxUtils;
using Microsoft.Win32;
using WPFLocalizeExtension.Engine;

namespace DriversGalaxy.OSMigrationTool.Backup.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Constructors

        public MainWindowViewModel()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;

            #region Commands initialization

            scanCommand = new SimpleCommand
            {
                ExecuteDelegate = x => Scan()
            };

            cancelScanCommand = new SimpleCommand
            {
                ExecuteDelegate = x => CancelScan()
            };

            checkDevicesGroupCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDevicesGroup
            };

            checkDeviceCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDevice
            };

            downloadDriversCommand = new SimpleCommand
            {
                ExecuteDelegate = x => DownloadDrivers()
            };

            cancelDownloadDriversCommand = new SimpleCommand
            {
                ExecuteDelegate = x => CancelDownloadDrivers()
            };

            composeCommand = new SimpleCommand
            {
                ExecuteDelegate = x => Compose()
            };

            cancelComposeCommand = new SimpleCommand
            {
                ExecuteDelegate = x => CancelCompose()
            };

            closeCommand = new SimpleCommand
            {
                ExecuteDelegate = x => Close()
            };

            #endregion

            InitializeBackgroundWorkers();

            DownloadsDirectory = String.Format(@"{0}\Drivers Galaxy\Downloads", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            try
            {
                if (!Directory.Exists(DownloadsDirectory))
                {
                    Directory.CreateDirectory(DownloadsDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region Commands

        ICommand scanCommand;
        public ICommand ScanCommand
        {
            get { return scanCommand; }
        }

        void Scan()
        {
            try
            {
                if (Status == ScanStatus.NotStarted)
                {
                    if (GetOsIdFromComboBox() == -1)
                    {
                        WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("SelectOSToScan"), WPFLocalizeExtensionHelpers.GetUIString("SelectOS"), WPFMessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    // Update device collections in the UI thread
                    if (CurrentDispatcher.Thread != null)
                    {
                        CurrentDispatcher.BeginInvoke((Action)(() =>
                        {
                            AllDevices = new ObservableCollection<MigrationDeviceInfo>();
                            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("DriverScan");
                            ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("NowScanning");
                            Progress = 0;
                            Status = ScanStatus.ScanStarted;
                            ScanStatusText = string.Empty;
                        }));
                    }

                    bgScan = new BackgroundWorker();
                    bgScan.DoWork += StartScan;
                    bgScan.WorkerSupportsCancellation = true;
                    bgScan.RunWorkerCompleted += ScanCompleted;
                    bgScan.RunWorkerAsync();
                }
                else
                {
                    ABORT = true;
                    PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ScanDrivers");
                    Status = ScanStatus.NotStarted;
                }
            }
            catch { }
        }

        ICommand cancelScanCommand;
        public ICommand CancelScanCommand
        {
            get { return cancelScanCommand; }
        }
        void CancelScan()
        {
            RunCancelScan();
        }

        ICommand checkDevicesGroupCommand;
        public ICommand CheckDevicesGroupCommand
        {
            get { return checkDevicesGroupCommand; }
        }
        void CheckDevicesGroup(object selectedMigrationDevicesGroup)
        {
            MigrationDevicesGroup migrationDevicesGroup = GroupedDevices.Where(d => d.DeviceClass == (string)selectedMigrationDevicesGroup).FirstOrDefault();
            if (migrationDevicesGroup != null)
            {
                foreach (MigrationDeviceInfo item in migrationDevicesGroup.Devices.Where(d => d.IsDestOSDriverAvailable))
                {
                    item.SelectedForDestOSDriverDownload = migrationDevicesGroup.GroupChecked;
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
            MigrationDeviceInfo device = AllDevices.Where(d => d.Id == (string)selectedDevice).FirstOrDefault();
            if (device != null)
            {
                MigrationDevicesGroup migrationDevicesGroup = GroupedDevices.Where(d => d.DeviceClass == device.DeviceClass).FirstOrDefault();
                if (migrationDevicesGroup != null && migrationDevicesGroup.Devices.Where(d => d.SelectedForDestOSDriverDownload && d.IsDestOSDriverAvailable).Count() == 0)
                {
                    migrationDevicesGroup.GroupChecked = false;
                }
                else
                {
                    if (migrationDevicesGroup != null && migrationDevicesGroup.Devices.Where(d => d.SelectedForDestOSDriverDownload == false && d.IsDestOSDriverAvailable).Count() == 0)
                    {
                        migrationDevicesGroup.GroupChecked = true;
                    }
                }
            }
        }

        ICommand downloadDriversCommand;
        public ICommand DownloadDriversCommand
        {
            get { return downloadDriversCommand; }
        }
        void DownloadDrivers()
        {
            RunDownloadDrivers();
        }

        ICommand cancelDownloadDriversCommand;
        public ICommand CancelDownloadDriversCommand
        {
            get { return cancelDownloadDriversCommand; }
        }
        void CancelDownloadDrivers()
        {
            RunCancelDownloadDrivers();
        }

        ICommand composeCommand;
        public ICommand ComposeCommand
        {
            get { return composeCommand; }
        }
        void Compose()
        {
            RunCompose();
        }

        ICommand cancelComposeCommand;
        public ICommand CancelComposeCommand
        {
            get { return cancelComposeCommand; }
        }
        void CancelCompose()
        {
            RunCancelCompose();
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

        #region Global Variables

        bool bScanningIsGoingOn = false;
        bool bDriverUpdateIsGoingOn = false;
        bool bComposingIsGoingOn = false;
        bool bIsStopped = false;

        StringBuilder szAppDataLoc;
        StringBuilder szTempLoc;
        StringBuilder szProductKey = new StringBuilder().Append("46288-11183-23588");
        StringBuilder szRegistryLoc = new StringBuilder().Append("Software\\DriversGalaxy");
        StringBuilder szRestorePointName = new StringBuilder().Append("DriversGalaxy");
        uint dwScanFlag = (uint)DUSDK_for.NET.DUSDKHandler.SCAN_FLAGS.SCAN_DEVICES_PRESENT;
        DriverData[] driverData = null;
        Dictionary<int, DriverData> UserUpdates;
        int nTotalDrivers = 0;
        int nComboOsId = 0;
        BackgroundWorker bgScan;

        #endregion

        #region Properties

        private ManualResetEvent cancelEvtArgs;
        private Boolean scanCancelled = false;

        readonly RegistryKey deviceClasses = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\", true);

        BackgroundWorker DownloadingBackgroundWorker = new BackgroundWorker();
        BackgroundWorker ComposingBackgroundWorker = new BackgroundWorker();

        Dispatcher CurrentDispatcher;
        public delegate void MethodInvoker();
        DriverUtils driverUtils = new DriverUtils();
        bool ABORT;
        string DownloadsDirectory;

        string destinationDirectory;
        public string DestinationDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(destinationDirectory))
                    destinationDirectory = @"C:\";
                return destinationDirectory;
            }
            set
            {
                destinationDirectory = value;
                OnPropertyChanged("DestinationDirectory");
            }
        }

        KeyValuePair<object, object> destinationOS;
        public KeyValuePair<object, object> DestinationOS
        {
            get
            {
                return destinationOS;
            }
            set
            {
                destinationOS = value;
                OnPropertyChanged("DestinationOS");
            }
        }

        int driverIndex;
        bool driverDownloading;

        /// <summary>
        /// Drivers for a destination OS collection presented as a <c>Dictionary</c> because this approach
        /// used in the <c>GetDriversForDestinationOS(Dictionary<string, string[]> driverScanResults)</c> method
        /// from the Freemium Utils driver info service, which is referenced via WCF
        /// </summary>
        Dictionary<string, string[]> driversForDestinationOs;
        DestinationOSDevices DestinationOSDevices = new DestinationOSDevices();

        ScanStatus status = ScanStatus.NotStarted;
        public ScanStatus Status
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

        string scanStatusTitle;
        public string ScanStatusTitle
        {
            get
            {
                return scanStatusTitle;
            }
            set
            {
                scanStatusTitle = value;
                OnPropertyChanged("ScanStatusTitle");
            }
        }

        string scanStatusText;
        public string ScanStatusText
        {
            get
            {
                return scanStatusText;
            }
            set
            {
                scanStatusText = value;
                OnPropertyChanged("ScanStatusText");
            }
        }

        string scanFinishTitle;
        public string ScanFinishTitle
        {
            get
            {
                return scanFinishTitle;
            }
            set
            {
                scanFinishTitle = value;
                OnPropertyChanged("ScanFinishTitle");
            }
        }

        string panelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ScanDrivers");
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

        ObservableCollection<MigrationDeviceInfo> allDevices = new ObservableCollection<MigrationDeviceInfo>();
        public ObservableCollection<MigrationDeviceInfo> AllDevices
        {
            get
            {
                return allDevices;
            }
            set
            {
                allDevices = value;
                OnPropertyChanged("AllDevices");
            }
        }

        ObservableCollection<MigrationDevicesGroup> groupedDevices = new ObservableCollection<MigrationDevicesGroup>();
        public ObservableCollection<MigrationDevicesGroup> GroupedDevices
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

        List<MigrationDeviceInfo> devicesForDestinationOS = new List<MigrationDeviceInfo>();
        public List<MigrationDeviceInfo> DevicesForDestinationOS
        {
            get
            {
                return devicesForDestinationOS;
            }
            set
            {
                devicesForDestinationOS = value;
                OnPropertyChanged("DevicesForDestinationOS");
            }
        }

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

        #endregion

        #region Private methods

        private void InitializeBackgroundWorkers()
        {
            DownloadingBackgroundWorker.WorkerReportsProgress = false;
            DownloadingBackgroundWorker.WorkerSupportsCancellation = true;
            DownloadingBackgroundWorker.DoWork += DriverDownload;

            ComposingBackgroundWorker.WorkerReportsProgress = false;
            ComposingBackgroundWorker.WorkerSupportsCancellation = true;
            ComposingBackgroundWorker.DoWork += ComposeDrivers;
            ComposingBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ComposingBackgroundWorker_RunWorkerCompleted);
        }

        void ComposingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (CurrentDispatcher.Thread != null)
            {
                CurrentDispatcher.BeginInvoke((Action)(() =>
                {
                    Progress = 100;

                    PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("AllDriversComposed");
                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("AllDriversComposed");
                    ScanStatusText = string.Empty;
                    ScanFinishTitle = WPFLocalizeExtensionHelpers.GetUIString("AllDriversComposed");
                    Status = ScanStatus.ComposeFinished;
                    Progress = 0;
                }));
            }
        }

        void StartScan(object sender, DoWorkEventArgs e)
        {
            bScanningIsGoingOn = true;
            bIsStopped = false;

            string saveFolder = CfgFile.Get("DriverDownloadsFolder");
            if (!string.IsNullOrEmpty(saveFolder))
            {
                DriverUtils.SaveDir = String.Format(@"{0}\Restore", Uri.UnescapeDataString(CfgFile.Get("DriverDownloadsFolder")));
                if (!Directory.Exists(DriverUtils.SaveDir))
                    Directory.CreateDirectory(DriverUtils.SaveDir);
                if (!String.IsNullOrEmpty(DriverUtils.SaveDir) && Directory.Exists(DriverUtils.SaveDir))
                {
                    szTempLoc = new StringBuilder().Append(DriverUtils.SaveDir);
                    szAppDataLoc = new StringBuilder().Append(DriverUtils.SaveDir);
                }
            }

            // this call has to be in another Thread (not the UI)
            cancelEvtArgs = new ManualResetEvent(false);
            nComboOsId = GetOsIdFromComboBox();

            Thread thread = new Thread(delegate()
            {
                try
                {
                    DUSDKHandler.DUSDK_scanDeviceDriversForUpdatesForOS(
                        progressCallback,
                        szProductKey,
                        szAppDataLoc,
                        szTempLoc,
                        szRegistryLoc,
                        dwScanFlag,
                        nComboOsId);
                }
                catch (Exception)
                {
                }

                if (cancelEvtArgs != null) cancelEvtArgs.Set();
            });

            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            cancelEvtArgs.WaitOne();

            if (bIsStopped)
            {
                CancelOperation();

                cancelEvtArgs = null;
                scanCancelled = true;
            }
        }

        void ScanCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool isCancelled = bIsStopped;

            bScanningIsGoingOn = false;
            bIsStopped = false;

            if (CurrentDispatcher.Thread != null)
            {
                CurrentDispatcher.Invoke((MethodInvoker)delegate
                {
                    if (isCancelled || scanCancelled)
                    {
                        scanCancelled = false;

                        return;
                    }
                    scanCancelled = false;

                    PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ScanDrivers");
                    Status = ScanStatus.NotStarted;
                    ScanStatusTitle = string.Empty;
                    ScanStatusText = string.Empty;
                    Progress = 0;

                    UpdateGroupedDevices();
                    int devicesForUpdate = AllDevices.Where(wh => wh.SelectedForDestOSDriverDownload == true).Count();
                    if (devicesForUpdate == 0)
                    {
                        PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("NoDriversToDownload");
                        ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("NoDriversToDownload");
                        ScanStatusText = string.Empty;
                        ScanFinishTitle = WPFLocalizeExtensionHelpers.GetUIString("NoDriversToDownload");
                        Status = ScanStatus.ScanFinishedNoDrivers;
                    }
                    else
                    {
                        PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ChooseDriversToDownload");
                        Status = ScanStatus.ScanFinishedDriversFound;
                        ScanFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversReadyToDownload"), AllDevices.Where(d => d.IsDestOSDriverAvailable).Count());
                    }
                }
                , null);
            }
        }

        /// <summary>
        /// Will call to cancel driver scan or driver update function
        /// </summary>
        /// <returns></returns>
        public bool CancelOperation()
        {
            bool retval = false;
            if (bScanningIsGoingOn || bDriverUpdateIsGoingOn || bComposingIsGoingOn)
            {
                int CancelFn = DUSDKHandler.DefineConstants.DEFAULT;
                if (bScanningIsGoingOn)
                {
                    CancelFn = DUSDKHandler.DefineConstants.CANCEL_SCAN;
                }
                else if (bDriverUpdateIsGoingOn)
                {
                    CancelFn = DUSDKHandler.DefineConstants.CANCEL_INSTALL;
                }
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

        /// <summary>
        /// Get os id from combo box
        /// </summary>
        /// <returns></returns>
        int GetOsIdFromComboBox()
        {
            if (DestinationOS.Key == null && DestinationOS.Value == null)
            {
                return -1;
            }
            else
            {
                return Convert.ToInt32(((KeyValuePair<object, object>)DestinationOS).Value);
            }
        }


        void RunCancelScan()
        {
            bIsStopped = true;
            if (bgScan.IsBusy)
                bgScan.CancelAsync();

            cancelEvtArgs.Set();

            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ScanDrivers");
            Status = ScanStatus.NotStarted;
            ScanStatusTitle = string.Empty;
            ScanStatusText = string.Empty;
            Progress = 0;
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
            string VersionInstalled = string.Empty;
            string VersionUpdated = string.Empty;
            DateTime dtInstalled;

            if (data != IntPtr.Zero)
            {
                dd = (DriverData)Marshal.PtrToStructure(
                            (IntPtr)(data.ToInt64() + currentItemPos * Marshal.SizeOf(typeof(DriverData))),
                            typeof(DriverData)
                            );

            }

            switch (progressType)
            {
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_SIZE_SCAN_DATA:
                    nTotalDrivers = nTotalDriversToScan;
                    driverData = new DriverData[nTotalDriversToScan];
                    UserUpdates = new Dictionary<int, DriverData>();
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_SCANNING:
                    if (data != IntPtr.Zero && !bIsStopped)
                    {
                        driverData[currentItemPos] = (DriverData)dd;
                        string infName = string.Empty;
                        try
                        {
                            infName = new FileInfo(driverData[currentItemPos].location).Name;
                        }
                        catch
                        {
                        }
                        MigrationDeviceInfo item = new MigrationDeviceInfo(
                        null,
                        driverData[currentItemPos].category,
                        driverData[currentItemPos].driverName,
                        infName,
                        string.IsNullOrEmpty(driverData[currentItemPos].version) || driverData[currentItemPos].version == "null" ? string.Empty : driverData[currentItemPos].version,
                        currentItemPos.ToString(),
                        driverData[currentItemPos].hardwareId,
                        driverData[currentItemPos].CompatibleIdIndex.ToString()
                        );

                        dtInstalled = DateTime.FromFileTime(driverData[currentItemPos].ulDateTimeQuadPart);

                        string driverName = driverData[currentItemPos].driverName;
                        if (driverName.Length > 46)
                            driverName = driverName.Substring(0, 43) + "...";

                        if (CurrentDispatcher.Thread != null)
                        {
                            CurrentDispatcher.Invoke((MethodInvoker)delegate
                            {
                                ScanStatusText = driverName;
                                AllDevices.Add(item);
                            }
                            , null);
                        }
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_RETRIEVING_UPDATES_DATA:
                    if (CurrentDispatcher.Thread != null)
                    {
                        CurrentDispatcher.Invoke((MethodInvoker)delegate
                        {
                            ScanStatusText = string.Empty;
                            ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("ContactingServer");
                        }, null);
                    }
                    break;
                //case DUSDKHandler.PROGRESS_TYPE.PROGRESS_RETRIEVING_UPDATES_FAILED_INET:
                //    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_FILTERING_UPDATES:
                    if (data != IntPtr.Zero && !bIsStopped)
                    {
                        // get driver update information                        
                        DriverData DriverUpdate = (DriverData)Marshal.PtrToStructure(
                              (IntPtr)(data.ToInt64() + currentItemPos * Marshal.SizeOf(typeof(DriverData))),
                              typeof(DriverData)
                              );

                        if (CurrentDispatcher.Thread != null)
                        {
                            CurrentDispatcher.Invoke((MethodInvoker)delegate
                            {
                                MigrationDeviceInfo migrationDeviceInfo = AllDevices.Where(wh => wh.Id == currentItemPos.ToString()).FirstOrDefault();
                                if (migrationDeviceInfo != null)
                                {
                                    migrationDeviceInfo.IsDestOSDriverAvailable = true;
                                    migrationDeviceInfo.SelectedForDestOSDriverDownload = true;
                                    migrationDeviceInfo.DownloadLink = DriverUpdate.libURL;
                                }

                                UserUpdates.Add(currentItemPos, DriverUpdate);
                            }
                            , null);
                        }
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_SCANNED:
                    //if (CurrentDispatcher.Thread != null)
                    //{
                    //    CurrentDispatcher.BeginInvoke((Action)(() =>
                    //    {
                    //        ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("ScanCompleted");
                    //        ScanStatusText = "";
                    //    }));
                    //}                   
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_UPDATE_STARTED_FOR_SINGLE:
                    {
                        if (data != IntPtr.Zero && dd.HasValue)
                        {
                            if (CurrentDispatcher.Thread != null)
                            {
                                CurrentDispatcher.BeginInvoke((Action)(() =>
                                {
                                    // DownloadedDrivers.Add(new DownloadingDriverModel(WPFLocalizeExtensionHelpers.GetUIString("InstallingDriver"), devicesForUpdate[currentItemPos]));
                                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("InstallingDriver");
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
                                    //ScanStatusTitle = 
                                    ScanStatusText = WPFLocalizeExtensionHelpers.GetUIString("UpdateCompleted") + " " + ((DriverData)dd).driverName;
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
                                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("UpdateFailed") + " " + driverName;
                                    ScanStatusText = string.Empty;
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
                                    ScanFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversDownloaded"), DevicesForDestinationOS.Count());
                                    Status = ScanStatus.DownloadFinished;
                                    driverDownloading = false;
                                    ScanStatusTitle = ScanFinishTitle;
                                    ScanStatusText = string.Empty;
                                    driverIndex = 0;
                                    Progress = 0;
                                }));
                        }));
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_DOWNLOAD_STARTED_FOR_SINGLE:
                    {
                        if (data != IntPtr.Zero && dd.HasValue)
                        {
                            string driverName = ((DriverData)dd).driverName;
                            if (driverName.Length > 23)
                                driverName = driverName.Substring(0, 20) + "...";

                            if (CurrentDispatcher.Thread != null)
                            {
                                CurrentDispatcher.BeginInvoke((Action)(() =>
                                {
                                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("DownloadingDriver");
                                    //string driverDownloadingDirectory = "";
                                    //string clearedDeviceName = DevicesForDestinationOS[driverIndex].DeviceName.Replace(" ", "").Replace(@"/", "");

                                    //try
                                    //{
                                    //    driverDownloadingDirectory = String.Format(@"{0}\{1}", DownloadsDirectory, clearedDeviceName);
                                    //    Directory.CreateDirectory(driverDownloadingDirectory);
                                    //}
                                    //catch { }
                                    //DriverUtils.SaveDir = driverDownloadingDirectory;
                                }));
                            }
                        }
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_DOWNLOAD_END_FOR_SINGLE:
                    {
                        if (data != IntPtr.Zero && dd.HasValue)
                        {
                            if (CurrentDispatcher.Thread != null)
                            {
                                CurrentDispatcher.BeginInvoke((Action)(() =>
                                {
                                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("DownloadComplete") + " " + ((DriverData)dd).driverName;
                                    ScanStatusText = string.Empty;
                                    DestinationOSDevices.DownloadedDestinationOSDrivers.Add(
                                        new DestinationOSDeviceInfo(
                                            DevicesForDestinationOS[driverIndex].DeviceClass,
                                            DevicesForDestinationOS[driverIndex].DeviceClassName,
                                            DevicesForDestinationOS[driverIndex].DeviceName,
                                            DevicesForDestinationOS[driverIndex].InfName,
                                            DevicesForDestinationOS[driverIndex].Version,
                                            DevicesForDestinationOS[driverIndex].Id,
                                            DevicesForDestinationOS[driverIndex].HardwareID,
                                            DevicesForDestinationOS[driverIndex].CompatID
                                        )
                                    );
                                    driverIndex++;
                                }));
                            }
                        }
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_DOWNLOAD_END_FOR_SINGLE_UNREG:
                    {
                        // download failed as user is unregistered
                    }
                    break;
                case DUSDKHandler.PROGRESS_TYPE.PROGRESS_DOWNLOAD_END_FOR_SINGLE_INET_ERROR:
                    {
                        // download failed because of Internet Error
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

        void UpdateGroupedDevices()
        {
            foreach (MigrationDeviceInfo device in AllDevices)
            {
                if (!device.IsDestOSDriverAvailable)
                    continue;

                MigrationDevicesGroup migrationDevicesGroup = GroupedDevices.Where(g => g.DeviceClass == device.DeviceClass).FirstOrDefault();
                if (migrationDevicesGroup == null)
                {
                    GroupedDevices.Add(new MigrationDevicesGroup(device.DeviceClass, device.DeviceClassName, device.DeviceClassImageSmall, new List<MigrationDeviceInfo> { device }));
                }
                else
                {
                    migrationDevicesGroup.Devices.Add(device);
                }
            }

            foreach (var deviceGroup in GroupedDevices)
            {
                deviceGroup.GroupChecked = true;
                deviceGroup.IsDestOSDriversAvailable = true;
            }

            OrderedDeviceGroups = CollectionViewSource.GetDefaultView(GroupedDevices);
            OrderedDeviceGroups.SortDescriptions.Clear();
            OrderedDeviceGroups.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
            OrderedDeviceGroups.Refresh();
        }

        void RunDownloadDrivers()
        {
            DevicesForDestinationOS = AllDevices.Where(d => d.SelectedForDestOSDriverDownload).ToList();
            if (DevicesForDestinationOS.Count == 0)
            {
                WPFMessageBox.Show(WPFLocalizeExtensionHelpers.GetUIString("SelectAtLeastOneDriver"), WPFLocalizeExtensionHelpers.GetUIString("CheckPreferences"), WPFMessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (DownloadingBackgroundWorker.IsBusy != true)
            {
                if (!String.IsNullOrEmpty(DownloadsDirectory) && Directory.Exists(DownloadsDirectory))
                {
                    Progress = 0;
                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("StartingDriversDownload");
                    ScanStatusText = string.Empty;
                    PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("DownloadingDrivers");
                    Status = ScanStatus.DownloadStarted;

                    // Start the asynchronous operation.
                    DownloadingBackgroundWorker.RunWorkerAsync();
                }
                else
                {
                    WPFMessageBox.Show(WPFLocalizeExtensionHelpers.GetUIString("CheckDownloadFolder"), WPFLocalizeExtensionHelpers.GetUIString("CheckPreferences"), WPFMessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        void DriverDownload(object sender, DoWorkEventArgs e)
        {
            bDriverUpdateIsGoingOn = true;

            //string driverDownloadingDirectory = "";
            string clearedDeviceName = DevicesForDestinationOS[driverIndex].DeviceName.Replace(" ", "").Replace(@"/", "");

            // Delete all previously downloaded drivers
            try
            {
                Directory.Delete(DownloadsDirectory, true);
            }
            catch { }

            DestinationOSDevices.OS = nComboOsId;//DestinationOS.Key.ToString();
            DestinationOSDevices.DownloadedDestinationOSDrivers = new List<DestinationOSDeviceInfo>();

            DriverData[] scanData = new DriverData[DevicesForDestinationOS.Count];
            DriverData[] updateData = new DriverData[DevicesForDestinationOS.Count];

            int i = 0;
            int id;

            foreach (MigrationDeviceInfo di in DevicesForDestinationOS)
            {
                if (Int32.TryParse(di.Id, out id))
                {
                    scanData[i] = driverData[id];
                    updateData[i] = UserUpdates[id];
                    i++;
                }
            }

            UpdateDrivers(scanData, updateData, nComboOsId, DUSDKHandler.UPDATE_FLAGS.UPDATE_FLAG_DOWNLOAD_ONLY, null);

            scanData = null;
            updateData = null;
        }

        /// <summary>
        /// call to SDK for driver update function
        /// </summary>
        /// <param name="arrUserDrivers"></param>
        /// <param name="arrUserUpdates"></param>
        public int UpdateDrivers(DriverData[] arrUserDrivers, DriverData[] arrUserUpdates, int nOsId, DUSDKHandler.UPDATE_FLAGS updateFlag, string archivePath)
        {
            int retval = 0;
            IntPtr pUnmanagedUserDrivers = IntPtr.Zero;
            IntPtr pUnmanagedUserUpdates = IntPtr.Zero;

            try
            {
                if (arrUserDrivers != null)
                {
                    // marshal input
                    pUnmanagedUserDrivers = MarshalHelper.MarshalArray(ref arrUserDrivers);
                }

                if (arrUserUpdates != null)
                {
                    pUnmanagedUserUpdates = MarshalHelper.MarshalArray(ref arrUserUpdates);
                }

                int arrLen = 0;

                if (arrUserDrivers != null)
                {
                    arrLen = arrUserDrivers.Length;
                }

                if ((updateFlag & DUSDKHandler.UPDATE_FLAGS.UPDATE_FLAG_RESTORE_ARCHIVE_ONLY) == DUSDKHandler.UPDATE_FLAGS.UPDATE_FLAG_RESTORE_ARCHIVE_ONLY)
                {
                    // call driver update function
                    retval = DUSDKHandler.OSMT_updateDeviceDriversEx(
                        progressCallback,
                        szProductKey,
                        szAppDataLoc,
                        szTempLoc,
                        szRegistryLoc,
                        downloadProgressCallback,
                        pUnmanagedUserDrivers,
                        pUnmanagedUserUpdates,
                        arrLen,
                        szRestorePointName,
                        nOsId,
                        (uint)updateFlag,
                        new StringBuilder(archivePath)
                        );
                }
                else
                {
                    if (nOsId == -1)
                    {
                        // call driver update function
                        retval = DUSDKHandler.updateDeviceDriversEx(
                            progressCallback,
                            szProductKey,
                            szAppDataLoc,
                            szTempLoc,
                            szRegistryLoc,
                            downloadProgressCallback,
                            pUnmanagedUserDrivers,
                            pUnmanagedUserUpdates,
                            arrLen,
                            szRestorePointName
                            );
                    }
                    else
                    {
                        // call driver update function
                        retval = DUSDKHandler.OSMT_updateDeviceDriversEx(
                            progressCallback,
                            szProductKey,
                            szAppDataLoc,
                            szTempLoc,
                            szRegistryLoc,
                            downloadProgressCallback,
                            pUnmanagedUserDrivers,
                            pUnmanagedUserUpdates,
                            arrLen,
                            szRestorePointName,
                            nOsId,
                            (uint)updateFlag,
                            new StringBuilder(archivePath)
                            );
                    }
                }


                if (retval == DUSDKHandler.DefineConstants.SUCCESS)
                {
                }
                else if (retval == DUSDKHandler.DefineConstants.CANCEL_INSTALL)
                {
                }
                else if (retval == DUSDKHandler.DefineConstants.FAIL)
                {
                    if (CurrentDispatcher.Thread != null)
                    {
                        CurrentDispatcher.BeginInvoke((Action)(() =>
                        {
                            ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("UpdateFailed");
                            ScanStatusText = string.Empty;
                        }));
                    }
                }
                else
                {
                    ; // check for error code
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            finally
            {
                // free memory
                if (pUnmanagedUserDrivers != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pUnmanagedUserDrivers);
                }

                if (pUnmanagedUserUpdates != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pUnmanagedUserUpdates);
                }

                pUnmanagedUserDrivers = IntPtr.Zero;
                pUnmanagedUserUpdates = IntPtr.Zero;

                bDriverUpdateIsGoingOn = false;
            }

            return retval;
        }

        /// <summary>
        /// Driver download progress
        /// </summary>
        /// <param name="progressType"></param>
        /// <param name="iTotalDownloaded"></param>
        /// <param name="iTotalSize"></param>
        /// <param name="iRetCode"></param>
        /// <param name="iPercentageCompleted"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public bool downloadProgressCallback(
           DUSDKHandler.PROGRESS_TYPE progressType,
           long iTotalDownloaded,
           long iTotalSize,
           int iRetCode,
           int iPercentageCompleted,
           int progress
           )
        {

            if (iTotalDownloaded >= 0 && iTotalSize > 0)
            {
                string strDownloadedSize = string.Empty, strTotalSize = string.Empty;
                CommonMethods.getSizeInKBMBGB((double)iTotalDownloaded, ref strDownloadedSize);
                CommonMethods.getSizeInKBMBGB((double)iTotalSize, ref strTotalSize);
                if (string.IsNullOrEmpty(strDownloadedSize)) { strDownloadedSize = "0 KB"; }

                if (CurrentDispatcher.Thread != null)
                {
                    CurrentDispatcher.BeginInvoke((Action)(() =>
                    {
                        ScanStatusTitle = String.Format(WPFLocalizeExtensionHelpers.GetUIString("DownloadStatus"), strDownloadedSize, strTotalSize);
                        ScanStatusText = string.Empty;
                        Progress = progress;
                    }));
                }
            }

            return true;
        }

        void driverUtils_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (!driverDownloading)
            {
                if (CurrentDispatcher.Thread != null)
                {
                    CurrentDispatcher.BeginInvoke((Action)(() =>
                    {
                        ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("DownloadingDriver");
                        ScanStatusText = DevicesForDestinationOS[driverIndex].DeviceName;
                    }));
                }
            }
            Progress = e.ProgressPercentage;
            driverDownloading = true;
        }

        void RunCancelDownloadDrivers()
        {
            bIsStopped = true;
            if (DownloadingBackgroundWorker.IsBusy)
                DownloadingBackgroundWorker.CancelAsync();
            CancelOperation();

            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ChooseDriversToDownload");
            Status = ScanStatus.ScanFinishedDriversFound;
            ScanFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversReadyToDownload"), AllDevices.Where(d => d.IsDestOSDriverAvailable).Count());
            ScanStatusTitle = ScanFinishTitle;
            ScanStatusText = string.Empty;
            driverDownloading = false;
            driverIndex = 0;
            Progress = 0;
        }

        void RunCompose()
        {
            Progress = 0;
            ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("NowComposing");
            ScanStatusText = string.Empty;
            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ComposingDrivers");
            Status = ScanStatus.ComposeStarted;

            ComposingBackgroundWorker.RunWorkerAsync();
        }

        private void ComposeDrivers(object sender, DoWorkEventArgs e)
        {
            bComposingIsGoingOn = true;
            string destinationOSDriversXMLFilePath = String.Format(@"{0}\DriverData.xml", DownloadsDirectory);
            try
            {
                var xs = new XmlSerializer(typeof(DestinationOSDevices));
                using (FileStream fs = File.Create(destinationOSDriversXMLFilePath))
                {
                    xs.Serialize(fs, DestinationOSDevices);
                }
            }
            catch
            {
            }

            if (CurrentDispatcher.Thread != null)
            {
                CurrentDispatcher.BeginInvoke((Action)(() =>
                {
                    Progress = 50;
                }));
            }

            try
            {
                DriverData[] scanData = new DriverData[DevicesForDestinationOS.Count];
                DriverData[] updateData = new DriverData[DevicesForDestinationOS.Count];

                int i = 0;
                int id;

                foreach (MigrationDeviceInfo di in DevicesForDestinationOS)
                {
                    if (Int32.TryParse(di.Id, out id))
                    {
                        scanData[i] = driverData[id];
                        updateData[i] = UserUpdates[id];
                        i++;
                    }
                }

                DestinationDirectory = String.Format(@"{0}\DriversGalaxy.OSMigrationTool.Restore\", DestinationDirectory);
                Directory.CreateDirectory(DestinationDirectory);
                string archivePath = DestinationDirectory + "\\DriverData.zip";
                int retval = UpdateDrivers(scanData, updateData, nComboOsId, DUSDKHandler.UPDATE_FLAGS.UPDATE_FLAG_ARCHIVE_DOWNLOADS_ONLY, archivePath);
                if (retval == DUSDKHandler.DefineConstants.SUCCESS)
                {
                    //Adding DriverData.xml file
                    ZipFile zip = new ZipFile(archivePath);
                    zip.AddFile(destinationOSDriversXMLFilePath, @"\");
                    zip.Save();
                }
                scanData = null;
                updateData = null;
            }
            catch { }
            finally
            {
                bComposingIsGoingOn = false;
            }
        }

        void RunCancelCompose()
        {
            if (ComposingBackgroundWorker.IsBusy)
                ComposingBackgroundWorker.CancelAsync();

            CancelOperation();

            ScanFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversDownloaded"), DevicesForDestinationOS.Count());
            Status = ScanStatus.DownloadFinished;
            ScanStatusTitle = ScanFinishTitle;
            ScanStatusText = string.Empty;
            Progress = 0;
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
