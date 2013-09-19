using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Serialization;
using DUSDK_for.NET;
using DriversGalaxy.Infrastructure;
using DriversGalaxy.Models;
using DriversGalaxy.Routine;
using DriversGalaxy.Utils;
using FreemiumUtil;
using FreemiumUtilites;
using MessageBoxUtils;
using Microsoft.Win32;
using WPFLocalizeExtension.Engine;
using System.Diagnostics;

namespace DriversGalaxy.ViewModels
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
                ExecuteDelegate = x => RunScan()
            };

            cancelScanCommand = new SimpleCommand
            {
                ExecuteDelegate = x => CancelScan()
            };

            showScanCommand = new SimpleCommand
            {
                ExecuteDelegate = x => ShowScan()
            };

            checkDevicesForUpdateCommand = new SimpleCommand
            {
                ExecuteDelegate = x => CheckDevicesForUpdate()
            };

            checkDeviceForUpdateCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDeviceForUpdate
            };

            enterLicenseKeyCommand = new SimpleCommand
            {
                ExecuteDelegate = x => EnterLicenseKey()
            };

            subscribeCommand = new SimpleCommand
            {
                ExecuteDelegate = x => Subscribe()
            };

            verifyLicenseKeyCommand = new SimpleCommand
            {
                ExecuteDelegate = x => VerifyLicenseKey()
            };

            updateCommand = new SimpleCommand
            {
                ExecuteDelegate = x => RunUpdate()
            };

            cancelUpdateCommand = new SimpleCommand
            {
                ExecuteDelegate = x => CancelUpdate()
            };

            selectBackupTypeCommand = new SimpleCommand
            {
                ExecuteDelegate = SelectBackupType
            };

            backupSelectedDriversCommand = new SimpleCommand
            {
                ExecuteDelegate = x => BackupSelectedDrivers()
            };

            backupActionFinishedCommand = new SimpleCommand
            {
                ExecuteDelegate = x => BackupActionFinished()
            };

            checkDevicesGroupCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDevicesGroup
            };

            checkDeviceCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDevice
            };

            selectDriversToRestoreCommand = new SimpleCommand
            {
                ExecuteDelegate = SelectDriversToRestore
            };

            checkDriverRestoreGroupCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDriverRestoreGroup
            };

            checkDriverRestoreCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDriverRestore
            };

            restoreDriversCommand = new SimpleCommand
            {
                ExecuteDelegate = x => RestoreDrivers()
            };

            excludeDeviceCommand = new SimpleCommand
            {
                ExecuteDelegate = ExcludeDevice
            };

            getDriverInfoForUpdateNeededDeviceCommand = new SimpleCommand
            {
                ExecuteDelegate = GetDriverInfoForUpdateNeededDevice
            };

            getDriverInfoForExcludedDeviceCommand = new SimpleCommand
            {
                ExecuteDelegate = GetDriverInfoForExcludedDevice
            };

            checkDevicesForIncludeCommand = new SimpleCommand
            {
                ExecuteDelegate = x => CheckDevicesForInclude()
            };

            checkDeviceForIncludeCommand = new SimpleCommand
            {
                ExecuteDelegate = CheckDeviceForInclude
            };

            includeDevicesCommand = new SimpleCommand
            {
                ExecuteDelegate = x => IncludeDevices()
            };

            #endregion

            SetBackupTypes();
            SetSocialButtonsMargin();
            SetLabels();
            initBackgroundWorker = new BackgroundWorker();
            initBackgroundWorker.DoWork += InitBackgroundWorkerDoWork;
            initBackgroundWorker.RunWorkerCompleted += (sender, args) =>
            {
                // Fill grouped devices collection with a values from XML
                UpdateGroupedDevices();

                InitialScanFinished = true;
            };
            initBackgroundWorker.RunWorkerAsync();
        }

        public void SetLabels()
        {
            var scanResultTextBlock = UIUtils.FindChild<TextBlock>(Application.Current.MainWindow, "ScanResult");
            if (scanResultTextBlock != null)
            {
                scanResultTextBlock.Text = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("OutdatedDriversFound"), AllDevices.Count(d => d.NeedsUpdate));
            }

            //Days from last scan

            int duration = 0;
            try
            {
                string lastscandate = CfgFile.Get("LastScanDate");
                DateTime LastScanDate = DateTime.ParseExact(lastscandate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (LastScanDate < DateTime.Now.Date)
                    duration = (DateTime.Now - DateTime.ParseExact(lastscandate, "dd/MM/yyyy", CultureInfo.InvariantCulture)).Duration().Days;
            }
            catch
            {
            }

            int daysFromLastScanProgress = duration * 360 / MainWindowViewModel.DaysFromLastScanMax;
            if (daysFromLastScanProgress == 0)
            {
                var daysFromLastScanTextBlock = UIUtils.FindChild<TextBlock>(Application.Current.MainWindow, "DaysFromLastScan");
                if (daysFromLastScanTextBlock != null)
                    daysFromLastScanTextBlock.Text = WPFLocalizeExtensionHelpers.GetUIString("Today");
            }
        }

        /// <summary>
        /// Sets backup types
        /// </summary>
        public void SetBackupTypes()
        {
            BackupTypes = new Dictionary<BackupType, string>
				                            	{
				                            		{
				                            			BackupType.ManualFull,
														WPFLocalizeExtensionHelpers.GetUIString("ManualBackupFull")
				                            		},
				                            		{
				                            			BackupType.ManualSelected,
				                            			WPFLocalizeExtensionHelpers.GetUIString("ManualBackupSelected")
				                            		}
				                            	};
            var backupTypesList = UIUtils.FindChild<ListView>(Application.Current.MainWindow, "BackupTypesList");
            if (backupTypesList != null)
            {
                backupTypesList.ItemsSource = BackupTypes;
            }
        }

        public void SetSocialButtonsMargin()
        {
            var twitBtn = UIUtils.FindChild<Button>(Application.Current.MainWindow, "TweetButton");
            if (twitBtn != null)
            {
                twitBtn.Margin = new Thickness(Convert.ToDouble(WPFLocalizeExtensionHelpers.GetUIString("TweetMargin")), 0, 0, 0);
            }

            var googleBtn = UIUtils.FindChild<Button>(Application.Current.MainWindow, "GoogleButton");
            if (googleBtn != null)
            {
                googleBtn.Margin = new Thickness(Convert.ToDouble(WPFLocalizeExtensionHelpers.GetUIString("GoogleMargin")), 0, 0, 0);
            }
        }

        #endregion

        #region Commands

        #region Driver scan related commands

        readonly ICommand scanCommand;
        readonly ICommand cancelScanCommand;
        readonly ICommand showScanCommand;
        readonly ICommand checkDevicesForUpdateCommand;
        readonly ICommand checkDeviceForUpdateCommand;
        readonly ICommand subscribeCommand;
        readonly ICommand verifyLicenseKeyCommand;
        readonly ICommand enterLicenseKeyCommand;
        readonly ICommand updateCommand;
        readonly ICommand cancelUpdateCommand;

        public ICommand ScanCommand
        {
            get { return scanCommand; }
        }

        public ICommand CancelScanCommand
        {
            get { return cancelScanCommand; }
        }

        public ICommand ShowScanCommand
        {
            get { return showScanCommand; }
        }

        public ICommand CheckDevicesForUpdateCommand
        {
            get { return checkDevicesForUpdateCommand; }
        }

        public ICommand CheckDeviceForUpdateCommand
        {
            get { return checkDeviceForUpdateCommand; }
        }

        public ICommand SubscribeCommand
        {
            get { return subscribeCommand; }
        }

        public ICommand EnterLicenseKeyCommand
        {
            get { return enterLicenseKeyCommand; }
        }

        public ICommand VerifyLicenseKeyCommand
        {
            get { return verifyLicenseKeyCommand; }
        }

        public ICommand UpdateCommand
        {
            get { return updateCommand; }
        }

        public ICommand CancelUpdateCommand
        {
            get { return cancelUpdateCommand; }
        }

        void CancelScan()
        {
            RunCancelScan();
        }

        void ShowScan()
        {
            RunShowScan();
        }

        void CheckDeviceForUpdate(object selectedDevice)
        {
            if (!AllDevices.Any(d => d.NeedsUpdate && d.SelectedForUpdate))
            {
                AllDevicesSelectedForUpdate = false;
            }
            else
            {
                if (AllDevices.Count(d => d.NeedsUpdate && d.SelectedForUpdate == false) == 0)
                {
                    AllDevicesSelectedForUpdate = true;
                }
            }
        }

        void CheckDevicesForUpdate()
        {
            foreach (DeviceInfo item in DevicesThatNeedsUpdate)
            {
                item.SelectedForUpdate = AllDevicesSelectedForUpdate;
            }
        }

        void CancelUpdate()
        {
            RunCancelUpdate();
        }

        #endregion

        #region Driver backup related commands

        readonly ICommand selectBackupTypeCommand;
        public ICommand SelectBackupTypeCommand
        {
            get { return selectBackupTypeCommand; }
        }
        void SelectBackupType(object selectedBackupType)
        {
            if (selectedBackupType != null)
            {
                var backupType = ((KeyValuePair<BackupType, string>)selectedBackupType).Key;
                switch (backupType)
                {
                    case BackupType.ManualFull:

                        if (!deviceListReadyForBackup)
                        {
                            WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("DriverListLoadingText"), WPFLocalizeExtensionHelpers.GetUIString("DriverListLoading"), WPFMessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }

                        ThreadPool.QueueUserWorkItem(x => RunBackup(GroupedDevices, BackupType.ManualFull));
                        break;

                    case BackupType.ManualSelected:
                        BackupStatus = BackupStatus.BackupTargetsSelection;
                        break;
                }
            }
            else
            {
                WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("SelectBackupTypeText"), WPFLocalizeExtensionHelpers.GetUIString("SelectBackupType"), WPFMessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        readonly ICommand backupSelectedDriversCommand;
        public ICommand BackupSelectedDriversCommand
        {
            get { return backupSelectedDriversCommand; }
        }
        void BackupSelectedDrivers()
        {

            var devicesToBackup = new ObservableCollection<DevicesGroup>();
            int i = 0;
            foreach (DevicesGroup group in GroupedDevices)
            {
                if (@group.Devices.Count(d => d.SelectedForBackup) > 0)
                {
                    devicesToBackup.Add(group);
                    devicesToBackup[i].Devices.RemoveAll(d => d.SelectedForBackup == false);
                    i++;
                }
            }
            if (devicesToBackup.Count == 0)
            {
                WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("SelectDriversToBackup"), WPFLocalizeExtensionHelpers.GetUIString("SelectDrivers"), WPFMessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
                ThreadPool.QueueUserWorkItem(x => RunBackup(devicesToBackup, BackupType.ManualSelected));
        }

        readonly ICommand backupActionFinishedCommand;
        public ICommand BackupActionFinishedCommand
        {
            get { return backupActionFinishedCommand; }
        }
        void BackupActionFinished()
        {
            BackupStatus = BackupStatus.NotStarted;
            BackupFinishTitle = "";
        }

        readonly ICommand checkDevicesGroupCommand;
        public ICommand CheckDevicesGroupCommand
        {
            get { return checkDevicesGroupCommand; }
        }
        void CheckDevicesGroup(object selectedDevicesGroup)
        {
            DevicesGroup devicesGroup = GroupedDevices.FirstOrDefault(d => d.DeviceClass == (string)selectedDevicesGroup);
            if (devicesGroup != null)
            {
                foreach (DeviceInfo item in devicesGroup.Devices)
                {
                    item.SelectedForBackup = devicesGroup.GroupChecked;
                }
            }
        }

        readonly ICommand checkDeviceCommand;
        public ICommand CheckDeviceCommand
        {
            get { return checkDeviceCommand; }
        }

        void CheckDevice(object selectedDevice)
        {
            DeviceInfo device = AllDevices.FirstOrDefault(d => d.Id == (string)selectedDevice);
            if (device != null)
            {
                DevicesGroup devicesGroup = GroupedDevices.FirstOrDefault(d => d.DeviceClass == device.DeviceClass);
                if (devicesGroup != null && devicesGroup.Devices.Count(d => d.SelectedForBackup) == 0)
                {
                    devicesGroup.GroupChecked = false;
                }
                else
                {
                    if (devicesGroup != null && devicesGroup.Devices.Count(d => d.SelectedForBackup == false) == 0)
                    {
                        devicesGroup.GroupChecked = true;
                    }
                }
            }
        }

        readonly ICommand selectDriversToRestoreCommand;
        public ICommand SelectDriversToRestoreCommand
        {
            get { return selectDriversToRestoreCommand; }
        }
        void SelectDriversToRestore(object backupItem)
        {
            if (backupItem != null)
            {
                RunSelectDriversToRestore((BackupItem)backupItem);
            }
            else
            {
                WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("SelectBackupTypeText"), WPFLocalizeExtensionHelpers.GetUIString("SelectDrivers"), WPFMessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        readonly ICommand checkDriverRestoreGroupCommand;
        public ICommand CheckDriverRestoreGroupCommand
        {
            get { return checkDriverRestoreGroupCommand; }
        }
        void CheckDriverRestoreGroup(object selectedDevicesGroup)
        {
            DevicesGroup devicesGroup = currentBackupItem.GroupedDrivers.FirstOrDefault(d => d.DeviceClass == (string)selectedDevicesGroup);
            if (devicesGroup != null)
            {
                foreach (DeviceInfo item in devicesGroup.Devices)
                {
                    item.SelectedForRestore = devicesGroup.GroupChecked;
                }
            }
        }

        readonly ICommand checkDriverRestoreCommand;
        public ICommand CheckDriverRestoreCommand
        {
            get { return checkDriverRestoreCommand; }
        }
        void CheckDriverRestore(object selectedDevice)
        {
            DeviceInfo device = AllDevices.FirstOrDefault(d => d.Id == (string)selectedDevice);
            if (device != null)
            {
                DevicesGroup devicesGroup = currentBackupItem.GroupedDrivers.FirstOrDefault(d => d.DeviceClass == device.DeviceClass);
                if (devicesGroup != null && devicesGroup.Devices.Count(d => d.SelectedForRestore) == 0)
                {
                    devicesGroup.GroupChecked = false;
                }
                else
                {
                    if (devicesGroup != null && devicesGroup.Devices.Count(d => d.SelectedForRestore == false) == 0)
                    {
                        devicesGroup.GroupChecked = true;
                    }
                }
            }
        }

        readonly ICommand restoreDriversCommand;
        public ICommand RestoreDriversCommand
        {
            get { return restoreDriversCommand; }
        }
        void RestoreDrivers()
        {
            ThreadPool.QueueUserWorkItem(x => RunRestore());
        }

        #endregion

        #region Devices exclusion related commands

        readonly ICommand excludeDeviceCommand;
        public ICommand ExcludeDeviceCommand
        {
            get { return excludeDeviceCommand; }
        }

        void ExcludeDevice(object id)
        {
            DeviceInfo device = AllDevices.FirstOrDefault(d => d.Id == (string)id);
            if (device != null)
            {
                MessageBoxResult result = WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, String.Format(WPFLocalizeExtensionHelpers.GetUIString("Exclude") + " {0} " + WPFLocalizeExtensionHelpers.GetUIString("FromScans"), device.DeviceName), WPFLocalizeExtensionHelpers.GetUIString("ExcludeDevice"), WPFMessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var item = DevicesForScanning.FirstOrDefault(d => d.Id == device.Id);
                    if (item != null)
                    {
                        DevicesForScanning.Remove(item);
                    }

                    device.IsExcluded = true;
                    ExcludedDevices.Add(device);

                    SaveExcludedDevicesToXML();
                    WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, String.Format(WPFLocalizeExtensionHelpers.GetUIString("DriverExcluded"), device.DeviceName), "Device excluded", WPFMessageBoxButton.OK, MessageBoxImage.Information);

                    ScanFinishTitle = String.Format("{{0}} " + WPFLocalizeExtensionHelpers.GetUIString("OutdatedDriversFound"), DevicesForScanning.Count(d => d.NeedsUpdate));

                    if (DevicesThatNeedsUpdate.IsEmpty)
                    {
                        RunShowScan();
                    }
                }
            }
        }

        readonly ICommand checkDevicesForIncludeCommand;
        public ICommand CheckDevicesForIncludeCommand
        {
            get { return checkDevicesForIncludeCommand; }
        }
        void CheckDevicesForInclude()
        {
            foreach (DeviceInfo item in ExcludedDevices.Where(d => d.IsExcluded))
            {
                item.SelectedForExclude = AllDevicesSelectedForInclude;
            }
        }

        readonly ICommand checkDeviceForIncludeCommand;
        public ICommand CheckDeviceForIncludeCommand
        {
            get { return checkDeviceForIncludeCommand; }
        }
        void CheckDeviceForInclude(object selectedDevice)
        {
            if (ExcludedDevices.Count(d => d.IsExcluded && d.SelectedForExclude) == 0)
            {
                AllDevicesSelectedForInclude = false;
            }
            else
            {
                if (ExcludedDevices.Count(d => d.IsExcluded && d.SelectedForExclude == false) == 0)
                {
                    AllDevicesSelectedForInclude = true;
                }
            }
        }

        readonly ICommand includeDevicesCommand;
        public ICommand IncludeDevicesCommand
        {
            get { return includeDevicesCommand; }
        }
        void IncludeDevices()
        {
            var devicesSelectedForInclude = ExcludedDevices.Where(d => d.SelectedForExclude).ToList();
            if (devicesSelectedForInclude.Any())
            {
                foreach (DeviceInfo item in devicesSelectedForInclude)
                {
                    item.IsExcluded = false;
                    item.SelectedForExclude = false;
                    ExcludedDevices.Remove(item);
                }

                SaveExcludedDevicesToXML();
                AllDevicesSelectedForInclude = false;
            }
            else
            {
                WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("SelectDevicesToRemoveFromExclude"), WPFLocalizeExtensionHelpers.GetUIString("SelectDevices"), WPFMessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Driver Info related commands
        readonly ICommand getDriverInfoForUpdateNeededDeviceCommand;
        readonly ICommand getDriverInfoForExcludedDeviceCommand;

        public ICommand GetDriverInfoForUpdateNeededDeviceCommand
        {
            get { return getDriverInfoForUpdateNeededDeviceCommand; }
        }

        public ICommand GetDriverInfoForExcludedDeviceCommand
        {
            get { return getDriverInfoForExcludedDeviceCommand; }
        }

        void ShowDriverInfo(DeviceInfo device)
        {
            Window mainWnd = Application.Current.MainWindow;
            DriverDetailInfoPopup driverInfoDetailPopup = new DriverDetailInfoPopup { Owner = mainWnd };

            driverInfoDetailPopup.DriverName.Text = device.DeviceName;
            if (!string.IsNullOrEmpty(device.InstalledDriverDate))
                driverInfoDetailPopup.DetailCurrentDriverDate.Text = device.InstalledDriverDate.ToString();
            if (!string.IsNullOrEmpty(device.NewDriverDate))
                driverInfoDetailPopup.DetailNewDriverDate.Text = device.NewDriverDate.ToString();

            driverInfoDetailPopup.Left = mainWnd.Left + (mainWnd.Width / 2 - driverInfoDetailPopup.Width / 2);
            int regToolsHeight = (int)driverInfoDetailPopup.Height;
            driverInfoDetailPopup.Height = 0;
            int topStart = (int)(mainWnd.Top + mainWnd.Height) + 30;
            driverInfoDetailPopup.Top = topStart;
            int topFinal = (int)(mainWnd.Top + (mainWnd.Height / 2 - regToolsHeight / 2));

            const int fullAnimationDuration = 50;
            int heightAnimationDuration = (fullAnimationDuration * regToolsHeight / (topStart - topFinal));

            DoubleAnimation slideUp = new DoubleAnimation
             {
                 From = topStart,
                 To = topFinal,
                 Duration = new Duration(TimeSpan.FromMilliseconds(fullAnimationDuration))
             };

            driverInfoDetailPopup.BeginAnimation(Window.TopProperty, slideUp);

            DoubleAnimation scaleUp = new DoubleAnimation
            {
                From = 0,
                To = regToolsHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(heightAnimationDuration))
            };
            driverInfoDetailPopup.BeginAnimation(Window.HeightProperty, scaleUp);

            driverInfoDetailPopup.AnimateInnerBox();

            driverInfoDetailPopup.ShowDialog();
        }

        void GetDriverInfoForUpdateNeededDevice(object id)
        {
            DeviceInfo device = AllDevices.FirstOrDefault(d => d.Id == (string)id);
            if (device != null)
            {
                ShowDriverInfo(device);
            }

        }

        void GetDriverInfoForExcludedDevice(object id)
        {
            DeviceInfo device = ExcludedDevices.FirstOrDefault(d => d.Id == (string)id);
            if (device != null)
            {
                ShowDriverInfo(device);
            }
        }
        #endregion

        #endregion

        #region Properties

        readonly BackgroundWorker initBackgroundWorker;
        const uint dwScanFlag = (uint)DUSDKHandler.SCAN_FLAGS.SCAN_DEVICES_PRESENT;
        static readonly string appFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\DriversGalaxy\\";
        readonly string ExcludedDevicesXMLFilePath = appFilesPath + "excludedDevices.xml";
        readonly string BackupItemsXMLFilePath = appFilesPath + "backupItems.xml";
        public static readonly int DaysFromLastScanMax = 31;
        public static readonly string DriversGalaxyTaskName = "DriversGalaxy";
        readonly RegistryKey deviceClasses = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\", true);

        readonly Dispatcher CurrentDispatcher;
        public delegate void MethodInvoker();
        readonly DriverUtils driverUtils = new DriverUtils();

        Dictionary<int, DriverData> UserUpdates;
        DriverData[] driverData = null;
        int nTotalDrivers = 0;
        bool silentUpdate = false;

        bool deviceListReadyForBackup = false;

        BackgroundWorker bgScan;
        BackgroundWorker bgUpdate;

        List<DeviceInfo> devicesForUpdate;
        BackupItem currentBackupItem;

        bool xmlItemsLoaded;
        public bool XMLItemsLoaded
        {
            get
            {
                return xmlItemsLoaded;
            }
            set
            {
                xmlItemsLoaded = value;
                OnPropertyChanged("XMLItemsLoaded");
            }
        }

        bool initialScanFinished;
        public bool InitialScanFinished
        {
            get
            {
                return initialScanFinished;
            }
            set
            {
                initialScanFinished = value;
                OnPropertyChanged("InitialScanFinished");
            }
        }

        bool driverInfoPopupIsOpen;
        public bool DriverInfoPopupIsOpen
        {
            get
            {
                return driverInfoPopupIsOpen;
            }
            set
            {
                driverInfoPopupIsOpen = value;
                OnPropertyChanged("DriverInfoPopupIsOpen");
            }
        }

        Dictionary<BackupType, String> backupTypes = new Dictionary<BackupType, string>();
        public Dictionary<BackupType, String> BackupTypes
        {
            get
            {
                return backupTypes;
            }
            set
            {
                backupTypes = value;
                OnPropertyChanged("BackupTypes");
            }
        }

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


        private Boolean _CreatingBackup;
        /// <summary>
        /// Gets a value indicating if a backup operation is pending.
        /// </summary>
        public Boolean CreatingBackup
        {
            get
            {
                return _CreatingBackup;
            }
            private set
            {
                _CreatingBackup = value;
                OnPropertyChanged("CreatingBackup");
                OnPropertyChanged("CanBackup");
            }
        }

        public Boolean CanBackup
        {
            get
            {
                return !CreatingBackup;
            }
        }



        private Boolean _RestoringBackup;
        /// <summary>
        /// Gets a value indicating if a restore operation is pending.
        /// </summary>
        public Boolean RestoringBackup
        {
            get
            {
                return _RestoringBackup;
            }
            private set
            {
                _RestoringBackup = value;
                OnPropertyChanged("RestoringBackup");
                OnPropertyChanged("CanRestore");
            }
        }

        /// <summary>
        /// Gets a vlaue indicating is a restore operation can be launched.
        /// </summary>
        public Boolean CanRestore
        {
            get
            {
                return !RestoringBackup;
            }
        }



        BackupStatus backupStatus = BackupStatus.NotStarted;
        /// <summary>
        /// Gets the backup status.
        /// </summary>
        public BackupStatus BackupStatus
        {
            get
            {
                return backupStatus;
            }
            private set
            {
                backupStatus = value;
                OnPropertyChanged("BackupStatus");
            }
        }

        string panelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ScanAndUpdateDrivers");
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

        string backupFinishTitle;
        public string BackupFinishTitle
        {
            get
            {
                return backupFinishTitle;
            }
            set
            {
                backupFinishTitle = value;
                OnPropertyChanged("BackupFinishTitle");
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

        string driverInfoPopupText;
        public string DriverInfoPopupText
        {
            get
            {
                return driverInfoPopupText;
            }
            set
            {
                driverInfoPopupText = value;
                OnPropertyChanged("DriverInfoPopupText");
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

        bool allDevicesSelectedForUpdate = true;
        public bool AllDevicesSelectedForUpdate
        {
            get
            {
                return allDevicesSelectedForUpdate;
            }
            set
            {
                allDevicesSelectedForUpdate = value;
                OnPropertyChanged("AllDevicesSelectedForUpdate");
            }
        }

        bool allDevicesSelectedForInclude;
        public bool AllDevicesSelectedForInclude
        {
            get
            {
                return allDevicesSelectedForInclude;
            }
            set
            {
                allDevicesSelectedForInclude = value;
                OnPropertyChanged("AllDevicesSelectedForInclude");
            }
        }

        ObservableCollection<DeviceInfo> excludedDevices = new ObservableCollection<DeviceInfo>();
        public ObservableCollection<DeviceInfo> ExcludedDevices
        {
            get
            {
                return excludedDevices;
            }
            set
            {
                excludedDevices = value;
                OnPropertyChanged("ExcludedDevices");
            }
        }

        ObservableCollection<DeviceInfo> allDevices = new ObservableCollection<DeviceInfo>();
        public ObservableCollection<DeviceInfo> AllDevices
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

        ObservableCollection<DeviceInfo> devicesForScanning;
        public ObservableCollection<DeviceInfo> DevicesForScanning
        {
            get
            {
                return devicesForScanning;
            }
            set
            {
                devicesForScanning = value;
                OnPropertyChanged("DevicesForScanning");
            }
        }

        ObservableCollection<DownloadingDriverModel> downloadedDrivers = new ObservableCollection<DownloadingDriverModel>();
        public ObservableCollection<DownloadingDriverModel> DownloadedDrivers
        {
            get
            {
                return downloadedDrivers;
            }
            set
            {
                downloadedDrivers = value;
                OnPropertyChanged("DownloadedDrivers");
            }
        }

        ICollectionView devicesThatNeedsUpdate;
        public ICollectionView DevicesThatNeedsUpdate
        {
            get
            {
                return devicesThatNeedsUpdate;
            }
            set
            {
                devicesThatNeedsUpdate = value;
                OnPropertyChanged("DevicesThatNeedsUpdate");
            }
        }

        ObservableCollection<BackupItem> backupItems = new ObservableCollection<BackupItem>();
        public ObservableCollection<BackupItem> BackupItems
        {
            get
            {
                return backupItems;
            }
            set
            {
                backupItems = value;
                OnPropertyChanged("BackupItems");
            }
        }

        ObservableCollection<DevicesGroup> groupedDevices = new ObservableCollection<DevicesGroup>();
        public ObservableCollection<DevicesGroup> GroupedDevices
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

        ICollectionView orderedDriverRestoreGroups;
        public ICollectionView OrderedDriverRestoreGroups
        {
            get
            {
                return orderedDriverRestoreGroups;
            }
            set
            {
                orderedDriverRestoreGroups = value;
                OnPropertyChanged("OrderedDriverRestoreGroups");
            }
        }

        #endregion


        #region Global Variables

        StringBuilder szProductKey = new StringBuilder().Append("18546-16122-13463");
        StringBuilder szAppDataLoc;
        StringBuilder szTempLoc;
        StringBuilder szRegistryLoc = new StringBuilder().Append("Software\\DriversGalaxy");
        StringBuilder szRestorePointName = new StringBuilder().Append("DriversGalaxy");

        bool bScanningIsGoingOn = false;
        bool bDriverUpdateIsGoingOn = false;
        bool bIsStopped = false;

        #endregion

        #region Private Methods

        private void InitBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            /*
             * Schedule 
             */
            if (TaskManager.IsTaskScheduled(DriversGalaxyTaskName))
            {
                Microsoft.Win32.TaskScheduler.Task task = TaskManager.GetTaskByName(DriversGalaxyTaskName);
            }
            else
            {
                TaskManager.CreateDefaultTask(DriversGalaxyTaskName, false);
            }
            string taskDescription = TaskManager.GetTaskDescription(DriversGalaxyTaskName);

            //Fill BackupItems from XML
            if (File.Exists(BackupItemsXMLFilePath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<BackupItem>));
                    using (StreamReader rd = new StreamReader(BackupItemsXMLFilePath))
                    {
                        BackupItems = xs.Deserialize(rd) as ObservableCollection<BackupItem>;
                    }
                }
                catch { }
            }

            //Fill ExcludedDevices from XML
            if (File.Exists(ExcludedDevicesXMLFilePath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<DeviceInfo>));
                    using (StreamReader rd = new StreamReader(ExcludedDevicesXMLFilePath))
                    {
                        ExcludedDevices = xs.Deserialize(rd) as ObservableCollection<DeviceInfo>;
                    }
                }
                catch { }
            }

            XMLItemsLoaded = true;

            // For testing only
            //Thread.Sleep(20000);

            // Fill AllDevices models            
            RunInitialScan();
        }

        private ManualResetEvent cancelEvtArgs;
        private Boolean scanCancelled = false;

        private void StartScan(object sender, DoWorkEventArgs e)
        {
            bScanningIsGoingOn = true;
            bIsStopped = false;

            string saveFolder = CfgFile.Get("DriverDownloadsFolder");
            if (!string.IsNullOrEmpty(saveFolder))
            {
                DriverUtils.SaveDir = Uri.UnescapeDataString(CfgFile.Get("DriverDownloadsFolder"));
                if (!String.IsNullOrEmpty(DriverUtils.SaveDir) && Directory.Exists(DriverUtils.SaveDir))
                {
                    szTempLoc = new StringBuilder().Append(DriverUtils.SaveDir);
                    szAppDataLoc = new StringBuilder().Append(DriverUtils.SaveDir);
                }
            }


            // this call has to be in another Thread (not the UI)
            cancelEvtArgs = new ManualResetEvent(false);

            Thread thread = new Thread(delegate()
            {
                try
                {
                    DUSDKHandler.DUSDK_scanDeviceDriversForUpdates(
                        progressCallback,
                        szProductKey,
                        szAppDataLoc,
                        szTempLoc,
                        szRegistryLoc,
                        dwScanFlag);
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
                //bIsStopped = false;
                //bScanningIsGoingOn = false;
            }

            //bScanningIsGoingOn = false;
            //bIsStopped = false;
        }

        void RunInitialScan()
        {
            var devices = driverUtils.ScanDevices();
            AllDevices = new ObservableCollection<DeviceInfo>();
            object ClassGuid;
            foreach (ManagementObject device in devices)
            {
                if (DriverUtils.RestrictedClasses.Contains(device.GetPropertyValue("DeviceClass")))
                    continue;

                //Uses device friendly name instead of device name when it's possible
                string deviceName = "";
                if (device.GetPropertyValue("FriendlyName") != null)
                {
                    deviceName = (string)device.GetPropertyValue("FriendlyName");
                }
                else
                {
                    deviceName = (string)(device.GetPropertyValue("DeviceName"));
                }

                //Uses device friendly name instead of class name when it's possible
                string deviceClassName = "";

                if (device.GetPropertyValue("ClassGuid") != null)
                {
                    var openSubKey = deviceClasses.OpenSubKey((string)device.GetPropertyValue("ClassGuid"));
                    if (openSubKey != null)
                    {
                        ClassGuid = openSubKey.GetValue(null);
                        if (ClassGuid != null)
                        {
                            deviceClassName = ClassGuid.ToString();
                        }
                    }
                }
                else
                {
                    deviceClassName = (string)(device.GetPropertyValue("DeviceClass"));
                }

                var item = new DeviceInfo(
                    (string)(device.GetPropertyValue("DeviceClass")),
                    deviceClassName,
                    deviceName,
                    (string)(device.GetPropertyValue("InfName")),
                    (string)(device.GetPropertyValue("DriverVersion")),
                    (string)(device.GetPropertyValue("DeviceId")),
                    (string)(device.GetPropertyValue("HardwareID")),
                    (string)(device.GetPropertyValue("CompatID"))
                    );
                AllDevices.Add(item);
            }
        }

        void RunScan(bool silentUpdate = false)
        {
            try
            {
                if (Status == ScanStatus.NotStarted)
                {
                    // Update device collections in the UI thread
                    if (CurrentDispatcher.Thread != null)
                    {
                        CurrentDispatcher.BeginInvoke((Action)(() =>
                        {
                            AllDevices = new ObservableCollection<DeviceInfo>();
                            DevicesForScanning = new ObservableCollection<DeviceInfo>();

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
                    PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ScanAndUpdateDrivers");
                    Status = ScanStatus.NotStarted;
                }
            }
            catch { }
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
                        ShowScan();
                        return;
                    }
                    scanCancelled = false;


                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("ScanCompleted");
                    ScanStatusText = string.Empty;

                    DevicesThatNeedsUpdate = CollectionViewSource.GetDefaultView(DevicesForScanning);
                    DevicesThatNeedsUpdate.Filter = i => ((DeviceInfo)i).NeedsUpdate;

                    UpdateGroupedDevices();

                    if (DevicesThatNeedsUpdate.IsEmpty)
                    {
                        PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("DriverUpdate");
                        ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("AllDriversAreUpToDate");
                        ScanStatusText = string.Empty;
                        ScanFinishTitle = WPFLocalizeExtensionHelpers.GetUIString("AllDriversAreUpToDate");
                        Status = ScanStatus.ScanFinishedOK;
                    }
                    else
                    {
                        if (isCancelled)
                        {
                            ShowScan();
                            return;
                        }

                        if (!silentUpdate)
                        {
                            AllDevicesSelectedForUpdate = true;
                            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("DriverScanResults");
                            Status = ScanStatus.ScanFinishedError;
                            ScanFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("OutdatedDriversFound"), AllDevices.Count(d => d.NeedsUpdate));
                        }
                        else
                        {
                            RunUpdate();
                        }
                    }
                }
                , null);
            }
            SaveExcludedDevicesToXML();
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
                        DeviceInfo item = new DeviceInfo(
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
                        item.InstalledDriverDate = dtInstalled.ToShortDateString();

                        string driverName = driverData[currentItemPos].driverName;
                        if (driverName.Length > 46)
                            driverName = driverName.Substring(0, 43) + "...";

                        if (CurrentDispatcher.Thread != null)
                        {
                            CurrentDispatcher.Invoke((MethodInvoker)delegate
                            {
                                ScanStatusText = driverName;

                                var excludedDevice = ExcludedDevices.FirstOrDefault(d => d.Id == item.Id);
                                if (excludedDevice != null)
                                {
                                    item.IsExcluded = excludedDevice.IsExcluded;
                                }
                                if (!item.IsExcluded)
                                {
                                    DevicesForScanning.Add(item);
                                }
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
                                DeviceInfo deviceInfo = DevicesForScanning.Where(wh => wh.Id == currentItemPos.ToString()).FirstOrDefault();
                                if (deviceInfo != null)
                                {
                                    deviceInfo.NeedsUpdate = true;
                                    deviceInfo.SelectedForUpdate = true;
                                    deviceInfo.NewDriverDate = DateTime.FromFileTime(DriverUpdate.ulDateTimeQuadPart).ToShortDateString();
                                    deviceInfo.DownloadLink = DriverUpdate.libURL;
                                    deviceInfo.InstallCommand = DriverUpdate.SetupLaunchParam;
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
                                    DownloadedDrivers.Add(new DownloadingDriverModel(WPFLocalizeExtensionHelpers.GetUIString("InstallingDriver"), devicesForUpdate[currentItemPos]));
                                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("InstallingDriver");
                                    ScanStatusText = devicesForUpdate[currentItemPos].DeviceName;
                                    //Progress = 0;
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
                                    DownloadedDrivers.Add(new DownloadingDriverModel(WPFLocalizeExtensionHelpers.GetUIString("DownloadingDriver"), devicesForUpdate[currentItemPos]));
                                    ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("DownloadingDriver");
                                    ScanStatusText = driverName;// ((DriverData)dd).driverName;
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


        void RunCancelScan()
        {
            bIsStopped = true;
            if (bgScan.IsBusy)
                bgScan.CancelAsync();

            if (cancelEvtArgs != null)
                cancelEvtArgs.Set();
            //            CancelOperation();
        }

        void RunShowScan()
        {
            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("ScanAndUpdateDrivers");
            Status = ScanStatus.NotStarted;
        }

        void EnterLicenseKey()
        {
            Status = ScanStatus.LicenseKeyEnter;
        }

        private bool CheckActivation()
        {
            bool result = false;
            RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\DriversGalaxy");
            string activationKey = string.Empty;
            if (reg != null)
            {
                if (reg.GetValue("ActivationKey") != null)
                    activationKey = reg.GetValue("ActivationKey").ToString();

                if (!string.IsNullOrEmpty(activationKey))
                {
                    if (IsValidLicense("driversgalaxy1", activationKey))
                        result = true;
                }
            }
            return result;
        }

        private bool CheckEnteredLicenseKey(string activationKey)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(activationKey))
            {
                if (IsValidLicense("driversgalaxy1", activationKey))
                {
                    try
                    {
                        RegistryKey rk = Registry.LocalMachine.CreateSubKey("Software\\DriversGalaxy");
                        rk.SetValue("ActivationKey", activationKey);
                    }
                    catch
                    {
                    }
                    result = true;
                }
            }
            return result;
        }


        private bool IsValidLicense(string product, string key)
        {
            string url = string.Format("http://license-management.azurewebsites.net/GetKeyValidUntil/{0}/{1} ", product, key);

            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            try
            {
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.Stream stream = resp.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                string Out = sr.ReadToEnd();
                sr.Close();

                Out = Out.Replace("\"", "");
                DateTime dt = DateTime.ParseExact(Out, "yyyy-MM-ddThh:mm:ss", null);

                return (dt >= DateTime.Now);
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        void Subscribe()
        {
            Process.Start(new ProcessStartInfo("https://www.cleverbridge.com/825/?scope=checkout&cart=128826"));            
        }

        void VerifyLicenseKey()
        {
            var licenseKeyTextBlock = UIUtils.FindChild<TextBox>(Application.Current.MainWindow, "LicenseKey");
            if (licenseKeyTextBlock != null)
            {
                string licenseKey = licenseKeyTextBlock.Text;
                if (CheckEnteredLicenseKey(licenseKey))
                {
                    RunUpdate(true);
                    return;
                }
                else
                {
                    WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("WrongLicenseKey"),
                                      "", WPFMessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        void RunUpdate()
        {
            RunUpdate(false);
        }

        void RunUpdate(bool activationIsChecked)
        {
            devicesForUpdate = DevicesForScanning.Where(d => d.NeedsUpdate && d.SelectedForUpdate).ToList();
            if (devicesForUpdate.Any())
            {
                if (activationIsChecked || CheckActivation())
                {
                    string saveFolder = CfgFile.Get("DriverDownloadsFolder");
                    if (!string.IsNullOrEmpty(saveFolder))
                        DriverUtils.SaveDir = Uri.UnescapeDataString(CfgFile.Get("DriverDownloadsFolder"));

                    if (!String.IsNullOrEmpty(DriverUtils.SaveDir) && Directory.Exists(DriverUtils.SaveDir))
                    {
                        MessageBoxResult result = WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("BeforeDriverUpdateMessage"),
                                                  "", WPFMessageBoxButton.ContinueCancel, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.OK)
                        {
                            bIsStopped = false;
                            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("DriverDownloadingAndInstallation");
                            Status = ScanStatus.UpdateStarted;
                            Progress = 0;
                            DownloadedDrivers.Clear();

                            szTempLoc = new StringBuilder().Append(DriverUtils.SaveDir);
                            szAppDataLoc = new StringBuilder().Append(DriverUtils.SaveDir);

                            bgUpdate = new BackgroundWorker();
                            bgUpdate.DoWork += DriverUpdate;
                            bgUpdate.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgUpdate_RunWorkerCompleted);
                            bgUpdate.WorkerSupportsCancellation = true;
                            bgUpdate.RunWorkerAsync();
                        }
                    }
                    else
                    {
                        WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("CheckDownloadFolder"), WPFLocalizeExtensionHelpers.GetUIString("CheckPreferences"), WPFMessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("UpgradeNow");
                    Status = ScanStatus.PaymentNeeded;
                }
            }
            else
            {
                WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("SelectDriversToUpdate"), WPFLocalizeExtensionHelpers.GetUIString("SelectDrivers"), WPFMessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        void bgUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!bIsStopped)
            {
                if (CurrentDispatcher.Thread != null)
                {
                    CurrentDispatcher.BeginInvoke((Action)(() =>
                    {
                        ScanFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("OutdatedDriversUpdated"), devicesForUpdate.Count());
                        Status = ScanStatus.UpdateFinished;
                        ScanStatusTitle = ScanFinishTitle;
                        ScanStatusText = string.Empty;
                    }));
                }
            }
        }


        void DriverUpdate(object sender, DoWorkEventArgs e)
        {
            bDriverUpdateIsGoingOn = true;
            devicesForUpdate = DevicesForScanning.Where(d => d.NeedsUpdate && d.SelectedForUpdate).ToList();

            DriverData[] scanData = new DriverData[devicesForUpdate.Count];
            DriverData[] updateData = new DriverData[devicesForUpdate.Count];

            int i = 0;
            int id;

            foreach (DeviceInfo di in devicesForUpdate)
            {
                if (Int32.TryParse(di.Id, out id))
                {
                    scanData[i] = driverData[id];
                    updateData[i] = UserUpdates[id];
                    i++;
                }
            }

            UpdateDrivers(scanData, updateData);

            scanData = null;
            updateData = null;
        }

        /// <summary>
        /// call to SDK for driver update function
        /// </summary>
        /// <param name="arrUserDrivers"></param>
        /// <param name="arrUserUpdates"></param>
        public int UpdateDrivers(DriverData[] arrUserDrivers, DriverData[] arrUserUpdates)
        {
            //bDriverUpdateIsGoingOn = true;
            int retval = 0;
            IntPtr pUnmanagedUserDrivers = IntPtr.Zero;
            IntPtr pUnmanagedUserUpdates = IntPtr.Zero;

            try
            {
                // marshal input
                pUnmanagedUserDrivers = MarshalHelper.MarshalArray(ref arrUserDrivers);
                pUnmanagedUserUpdates = MarshalHelper.MarshalArray(ref arrUserUpdates);

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
                    arrUserDrivers.Length,
                    szRestorePointName
                    );


                if (retval == DUSDKHandler.DefineConstants.SUCCESS)
                {
                    //this.UIThread(() => this.label3.Text = "Drivers Updated Successfully!!");
                }
                else if (retval == DUSDKHandler.DefineConstants.CANCEL_INSTALL)
                {
                    //this.UIThread(() => this.label3.Text = "Update Stopped Successfully!!");
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
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                //TODO: Add SmartAssembly bug tracking here!
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
                        //ScanStatusTitle = WPFLocalizeExtensionHelpers.GetUIString("DownloadingDriver");
                        ScanStatusTitle = String.Format(WPFLocalizeExtensionHelpers.GetUIString("DownloadStatus"), strDownloadedSize, strTotalSize);
                        ScanStatusText = string.Empty;
                        Progress = progress;
                    }));
                }
            }

            return true;
        }

        void RunCancelUpdate()
        {
            bIsStopped = true;
            if (bgUpdate.IsBusy)
                bgUpdate.CancelAsync();
            CancelOperation();
            PanelScanHeader = WPFLocalizeExtensionHelpers.GetUIString("DriverScanResults");
            Status = ScanStatus.ScanFinishedError;
            ScanFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("OutdatedDriversFound"), DevicesForScanning.Count(d => d.NeedsUpdate));
        }

        void RunBackup(ObservableCollection<DevicesGroup> driversToBackup, BackupType backupType)
        {
            CreatingBackup = true;

            string backupDir = Uri.UnescapeDataString(CfgFile.Get("BackupsFolder"));
            if (!String.IsNullOrEmpty(backupDir) && new DirectoryInfo(backupDir).Exists)
            {
                Progress = 0;
                int driversCount = 0;
                int totalDriversCount = driversToBackup.Sum(su => su.Devices.Count);
                foreach (DevicesGroup group in driversToBackup)
                {
                    group.GroupChecked = true;
                    foreach (DeviceInfo item in group.Devices)
                    {
                        if (driverUtils.BackupDriver(item.DeviceName, item.InfName, backupDir))
                        {
                            item.SelectedForRestore = true;
                            driversCount++;
                            Progress = driversCount * 100 / totalDriversCount;
                        }
                    }
                }
                if (CurrentDispatcher.Thread != null)
                {
                    CurrentDispatcher.BeginInvoke((Action)(() =>
                    {
                        BackupItems.Add(
                            new BackupItem(
                                backupDir,
                                DateTime.Now,
                                BackupTypes[backupType],
                                driversToBackup
                            )
                        );
                        SaveBackupItemsToXML();

                        BackupFinishTitle = String.Format(WPFLocalizeExtensionHelpers.GetUIString("DriversBackupedSuccesfully"), driversCount);
                        BackupStatus = BackupStatus.BackupFinished;
                        CreatingBackup = false;
                    }));
                }
            }
            else
            {
                if (CurrentDispatcher.Thread != null)
                {
                    CurrentDispatcher.BeginInvoke((Action)(() => WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("CheckBackupsFolder"), WPFLocalizeExtensionHelpers.GetUIString("CheckPreferences"), WPFMessageBoxButton.OK, MessageBoxImage.Error)));
                    CreatingBackup = false;
                }
            }
        }

        void RunSelectDriversToRestore(BackupItem backupItem)
        {
            currentBackupItem = backupItem;
            OrderedDriverRestoreGroups = CollectionViewSource.GetDefaultView(backupItem.GroupedDrivers);
            OrderedDriverRestoreGroups.SortDescriptions.Clear();
            OrderedDriverRestoreGroups.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
            OrderedDriverRestoreGroups.Refresh();
            BackupStatus = BackupStatus.RestoreTargetsSelection;
            RestoringBackup = false;
        }

        void RunRestore()
        {
            RestoringBackup = true;

            string backupDir = currentBackupItem.Path;
            DirectoryInfo[] subDirs = new DirectoryInfo(backupDir).GetDirectories();
            int restoredDriversCount = 0;
            int selectedDriversCount = currentBackupItem.GroupedDrivers.Sum
                (su => su.Devices.Where(wh => wh.SelectedForRestore == true).Count());

            Progress = 0;
            foreach (DevicesGroup group in currentBackupItem.GroupedDrivers)
            {
                foreach (DeviceInfo item in group.Devices)
                {
                    if (item.SelectedForRestore)
                    {
                        foreach (DirectoryInfo dirInfo in subDirs)
                        {
                            string tmpDirInfo = dirInfo.Name.Replace("/", string.Empty).Replace(" ", string.Empty);
                            string tempDeviceName = item.DeviceName.Replace("/", string.Empty).Replace(" ", string.Empty);
                            // see if dirInfo.Name == item.DeviceName               
                            if (tmpDirInfo == tempDeviceName)
                            {
                                if (driverUtils.RestoreDriver(dirInfo.Name, backupDir))
                                {
                                    restoredDriversCount++;
                                    Progress = restoredDriversCount * 100 / selectedDriversCount;
                                }
                                continue;
                            }
                        }
                    }
                }
            }

            //Thread.Sleep(2000);
            if (CurrentDispatcher.Thread != null)
            {
                CurrentDispatcher.BeginInvoke((Action)(() =>
                {
                    RestoringBackup = false;
                    if (restoredDriversCount != 0)
                    {
                        BackupFinishTitle = String.Format("{0} " + WPFLocalizeExtensionHelpers.GetUIString("DriversRestoredSuccesfully"), restoredDriversCount);
                        BackupStatus = BackupStatus.RestoreFinished;
                    }
                    else if (selectedDriversCount == 0)
                    {
                        WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("SelectDriversForRestoreText"), WPFLocalizeExtensionHelpers.GetUIString("SelectDriversForRestoreCaption"), WPFMessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        WPFMessageBox.Show(Application.Current.MainWindow, LocalizeDictionary.Instance.Culture, WPFLocalizeExtensionHelpers.GetUIString("RestoreFailedText"), WPFLocalizeExtensionHelpers.GetUIString("RestoreFailedCaption"), WPFMessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
        }

        /// <summary>
        /// Will call to cancel driver scan or driver update function
        /// </summary>
        /// <returns></returns>
        public bool CancelOperation()
        {
            bool retval = false;
            if (bScanningIsGoingOn || bDriverUpdateIsGoingOn)
            {
                //Thread th = new Thread(delegate()
                //{
                //this.UIThread(() => btnCancel.Enabled = false);

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
                //this.UIThread(() => btnCancel.Enabled = true);
                //});
                //th.Start();
                //th.Join();
            }

            return retval;
        }


        void UpdateGroupedDevices()
        {
            GroupedDevices = new ObservableCollection<DevicesGroup>();
            foreach (DeviceInfo device in AllDevices)
            {
                //device.InfName == "null" means that there is no installed *.inf for that device, so nothing to backup for it
                if (device.InfName != "null")
                {
                    DevicesGroup devicesGroup = GroupedDevices.FirstOrDefault(g => g.DeviceClass == device.DeviceClass);
                    if (devicesGroup == null)
                    {
                        GroupedDevices.Add(new DevicesGroup(device.DeviceClass, device.DeviceClassName, device.DeviceClassImageSmall, new List<DeviceInfo> { device }));
                    }
                    else
                    {
                        devicesGroup.Devices.Add(device);
                    }
                }
            }

            OrderedDeviceGroups = CollectionViewSource.GetDefaultView(GroupedDevices);
            OrderedDeviceGroups.SortDescriptions.Clear();
            OrderedDeviceGroups.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
            OrderedDeviceGroups.Refresh();

            deviceListReadyForBackup = true;
        }

        void SaveExcludedDevicesToXML()
        {
            try
            {
                var xs = new XmlSerializer(typeof(ObservableCollection<DeviceInfo>));
                using (StreamWriter wr = new StreamWriter(ExcludedDevicesXMLFilePath))
                {
                    xs.Serialize(wr, ExcludedDevices);
                }
            }
            catch { }
        }

        void SaveBackupItemsToXML()
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<BackupItem>));
                using (StreamWriter wr = new StreamWriter(BackupItemsXMLFilePath))
                {
                    xs.Serialize(wr, BackupItems);
                }
            }
            catch { }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Selects all apps and runs Scan on it
        /// </summary>
        public void SelectAllAndScan(bool updateAfterScan)
        {
            ThreadPool.QueueUserWorkItem(x => RunScan(updateAfterScan));
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
