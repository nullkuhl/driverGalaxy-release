using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DriversGalaxy.Infrastructure;
using DriversGalaxy.Routine;
using DriversGalaxy.ViewModels;
using Microsoft.Win32.TaskScheduler;
using FreemiumUtil;
using WPFLocalizeExtension.Engine;

namespace DriversGalaxy.Views
{
    /// <summary>
    /// Interaction logic for PanelScan.xaml
    /// </summary>
    public partial class PanelScan
    {
        public PanelScan()
        {
            InitializeComponent();
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
            SetDaysFromLastScanProgressBarValue(duration);
            LoadTaskParams();
        }

        void SetDaysFromLastScanProgressBarValue(int daysFromLastScan)
        {
            int daysFromLastScanProgress = daysFromLastScan * 360 / MainWindowViewModel.DaysFromLastScanMax;
            if (daysFromLastScan == 0)
            {
                DaysFromLastScan.FontSize = 30;
                DaysFromLastScan.Margin = new Thickness(2, 56, 0, 0);
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(CfgFile.Get("Lang"));
                DaysFromLastScan.Text = WPFLocalizeExtensionHelpers.GetUIString("Today");
                DaysAgoText.Visibility = System.Windows.Visibility.Hidden;
                Gradient.Visibility = Visibility.Collapsed;
                MoreThan.Visibility = Visibility.Collapsed;
                return;
            }


            DaysFromLastScan.Text = daysFromLastScan.ToString(CultureInfo.InvariantCulture);
            if ((daysFromLastScanProgress < 359 && daysFromLastScanProgress > 0) || (daysFromLastScan <= MainWindowViewModel.DaysFromLastScanMax && daysFromLastScan > 0))
            {
                Gradient.Visibility = Visibility.Visible;
                DaysFromLastScanProgressBar.Visibility = Visibility.Visible;
                MoreThan.Visibility = Visibility.Collapsed;
                DaysFromLastScanProgressBar.RotationAngle = daysFromLastScanProgress;
                DaysFromLastScanProgressBar.WedgeAngle = 360 - daysFromLastScanProgress;
                DaysFromLastScanProgressBar.Visibility = Visibility.Visible;
            }
            else
            {
                if (daysFromLastScan > 0)
                {
                    MoreThan.Visibility = Visibility.Visible;
                    DaysFromLastScan.Text = MainWindowViewModel.DaysFromLastScanMax.ToString(CultureInfo.InvariantCulture);
                    DaysFromLastScanProgressBar.RotationAngle = 0;
                    DaysFromLastScanProgressBar.WedgeAngle = 0;
                    DaysFromLastScanProgressBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (daysFromLastScan == 0)
                    {
                        Gradient.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (daysFromLastScan == MainWindowViewModel.DaysFromLastScanMax)
            {
                DaysFromLastScanProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void StartScan(object sender, RoutedEventArgs e)
        {
            SetDaysFromLastScanProgressBarValue(0);
            CfgFile.Set("LastScanDate", DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
        }

        private void AutoScan_Click(object sender, RoutedEventArgs e)
        {
            Task task = TaskManager.GetTaskByName(MainWindowViewModel.DriversGalaxyTaskName);
            if (AutoScan.IsChecked == true)
            {
                if (task != null)
                {
                    TaskManager.UpdateTaskStatus(MainWindowViewModel.DriversGalaxyTaskName, true);
                }
                else
                {
                    TaskManager.CreateDefaultTask(MainWindowViewModel.DriversGalaxyTaskName, true);
                }
            }
            else
            {
                if (task != null)
                {
                    TaskManager.UpdateTaskStatus(MainWindowViewModel.DriversGalaxyTaskName, false);
                }
                else
                {
                    TaskManager.CreateDefaultTask(MainWindowViewModel.DriversGalaxyTaskName, false);
                }
            }
        }

        private void AutoUpdate_Click(object sender, RoutedEventArgs e)
        {
            Task task = TaskManager.GetTaskByName(MainWindowViewModel.DriversGalaxyTaskName);
            if (task != null)
            {
                task.Definition.Actions.Clear();
                task.Definition.Actions.Add(AutoUpdate.IsChecked == true
                                                ? new ExecAction(Environment.CurrentDirectory + @"\1Click.exe", "AutoUpdate")
                                                : new ExecAction(Environment.CurrentDirectory + @"\1Click.exe"));
                task.RegisterChanges();
            }
        }

        private void AutoScanPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (AutoScanPeriod.SelectedIndex)
            {
                case (int)Schedule.Daily:
                    {
                        AutoScanDate.Visibility = Visibility.Collapsed;
                        AutoScanDay.IsEnabled = false;
                        AutoScanDay.SelectedIndex = (int)DayOfWeek.NADay;
                        NADay.Visibility = Visibility.Visible;
                        AutoScanDay.Visibility = Visibility.Visible;
                        break;
                    }
                case (int)Schedule.Weekly:
                    {
                        AutoScanDate.Visibility = Visibility.Collapsed;
                        NADay.Visibility = Visibility.Collapsed;
                        AutoScanDay.SelectedIndex = (int)DayOfWeek.Monday;
                        AutoScanDay.IsEnabled = true;
                        AutoScanDay.Visibility = Visibility.Visible;
                        break;
                    }
                case (int)Schedule.Monthly:
                    {
                        NADay.Visibility = Visibility.Collapsed;
                        AutoScanDay.SelectedIndex = (int)DayOfWeek.Monday;
                        AutoScanDay.IsEnabled = true;
                        AutoScanDay.Visibility = Visibility.Collapsed;
                        AutoScanDate.Visibility = Visibility.Visible;
                        break;
                    }
            }
            SaveTask();
        }

        private void AutoScanDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveTask();
        }

        private void AutoScanDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveTask();
        }

        private void AutoScanTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveTask();
        }

        void SaveTask()
        {
            bool isNewTask = true;
            try
            {
                var service = new TaskService();
                TaskDefinition td = service.NewTask();
                Microsoft.Win32.TaskScheduler.TriggerCollection trgCollection;
                var oldTriggerDate = new DateTime();

                Task task = service.FindTask(MainWindowViewModel.DriversGalaxyTaskName);

                if (task != null)
                {
                    isNewTask = false;
                    oldTriggerDate = task.Definition.Triggers.Count > 0 ? task.Definition.Triggers[0].StartBoundary.Date : DateTime.Today;
                    task.Definition.Triggers.Clear();
                    trgCollection = task.Definition.Triggers;
                }
                else
                {
                    td.RegistrationInfo.Description = WPFLocalizeExtensionHelpers.GetUIString("WindowsTaskRegistrationInfo");
                    td.Settings.Enabled = true;
                    td.Actions.Add(new ExecAction(Environment.CurrentDirectory + @"\1Click.exe"));
                    trgCollection = td.Triggers;
                }

                TimeSpan selectedTime = TimeSpan.Parse(((ComboBoxItem)AutoScanTime.SelectedValue).Content.ToString());
                switch (AutoScanPeriod.SelectedIndex)
                {
                    case (int)Schedule.Daily:
                        {
                            var dTrigger = new DailyTrigger { DaysInterval = 1 };
                            if (isNewTask)
                                dTrigger.StartBoundary = DateTime.Today.Date + selectedTime;
                            else
                                dTrigger.StartBoundary = oldTriggerDate + selectedTime;

                            trgCollection.Add(dTrigger);
                            break;
                        }
                    case (int)Schedule.Weekly:
                        {
                            var wTrigger = new WeeklyTrigger();
                            switch (AutoScanDay.SelectedIndex)
                            {
                                case (int)DayOfWeek.Monday:
                                    {
                                        wTrigger.DaysOfWeek = DaysOfTheWeek.Monday;
                                        break;
                                    }
                                case (int)DayOfWeek.Tuesday:
                                    {
                                        wTrigger.DaysOfWeek = DaysOfTheWeek.Tuesday;
                                        break;
                                    }
                                case (int)DayOfWeek.Wednesday:
                                    {
                                        wTrigger.DaysOfWeek = DaysOfTheWeek.Wednesday;
                                        break;
                                    }
                                case (int)DayOfWeek.Thursday:
                                    {
                                        wTrigger.DaysOfWeek = DaysOfTheWeek.Thursday;
                                        break;
                                    }
                                case (int)DayOfWeek.Friday:
                                    {
                                        wTrigger.DaysOfWeek = DaysOfTheWeek.Friday;
                                        break;
                                    }
                                case (int)DayOfWeek.Saturday:
                                    {
                                        wTrigger.DaysOfWeek = DaysOfTheWeek.Saturday;
                                        break;
                                    }
                                case (int)DayOfWeek.Sunday:
                                    {
                                        wTrigger.DaysOfWeek = DaysOfTheWeek.Sunday;
                                        break;
                                    }
                            }
                            trgCollection.Add(wTrigger);
                            foreach (WeeklyTrigger trg in trgCollection)
                            {
                                if (isNewTask)
                                    trg.StartBoundary = DateTime.Today.Date + selectedTime;
                                else
                                    trg.StartBoundary = oldTriggerDate + selectedTime;
                                trg.WeeksInterval = 1;
                            }
                            break;
                        }
                    case (int)Schedule.Monthly:
                        {
                            var mTrigger = new MonthlyTrigger();
                            if (isNewTask)
                                mTrigger.StartBoundary = DateTime.Today.Date + selectedTime;
                            else
                                mTrigger.StartBoundary = oldTriggerDate + selectedTime;
                            mTrigger.MonthsOfYear = MonthsOfTheYear.AllMonths;
                            mTrigger.DaysOfMonth = new int[] { Int16.Parse(((ComboBoxItem)AutoScanDate.SelectedValue).Content.ToString()) };
                            trgCollection.Add(mTrigger);
                            break;
                        }
                }

                // Register the task in the root folder
                if (isNewTask)
                    service.RootFolder.RegisterTaskDefinition(MainWindowViewModel.DriversGalaxyTaskName, td);
                else
                    task.RegisterChanges();

                TaskManager.UpdateTaskStatus(MainWindowViewModel.DriversGalaxyTaskName, AutoScan.IsChecked == true);
            }
            catch { }
        }

        void LoadTaskParams()
        {
            try
            {
                var service = new TaskService();
                TaskDefinition td = service.NewTask();
                Microsoft.Win32.TaskScheduler.TriggerCollection trgCollection;
                var oldTriggerDate = new DateTime();

                Task task = service.FindTask(MainWindowViewModel.DriversGalaxyTaskName);

                if (task != null)
                {
                    AutoScan.IsChecked = task.Enabled;
                    if (task.Definition.Triggers.Count > 0)
                    {
                        oldTriggerDate = task.Definition.Triggers[0].StartBoundary.Date;
                    }
                    trgCollection = task.Definition.Triggers;
                    foreach (Microsoft.Win32.TaskScheduler.Trigger trg in task.Definition.Triggers)
                    {
                        string time = trg.StartBoundary.ToString("HH:mm");
                        byte index = 0;
                        byte i = 0;
                        foreach (var item in AutoScanTime.Items)
                        {
                            if (((ComboBoxItem)item).Content.ToString() == time)
                            {
                                index = i;
                                break;
                            }
                            i++;
                        }
                        AutoScanTime.SelectedIndex = index;

                        if (trg.TriggerType == TaskTriggerType.Daily)
                        {
                            AutoScanPeriod.SelectedIndex = (int)Schedule.Daily;
                        }
                        else if (trg.TriggerType == TaskTriggerType.Weekly)
                        {
                            AutoScanPeriod.SelectedIndex = (int)Schedule.Weekly;
                            var wTrigger = (trg as WeeklyTrigger);

                            if (wTrigger.DaysOfWeek == DaysOfTheWeek.Monday)
                            { AutoScanDay.SelectedIndex = (int)DayOfWeek.Monday; }

                            if (wTrigger.DaysOfWeek == DaysOfTheWeek.Tuesday)
                            { AutoScanDay.SelectedIndex = (int)DayOfWeek.Tuesday; }

                            if (wTrigger.DaysOfWeek == DaysOfTheWeek.Wednesday)
                            { AutoScanDay.SelectedIndex = (int)DayOfWeek.Wednesday; }

                            if (wTrigger.DaysOfWeek == DaysOfTheWeek.Thursday)
                            { AutoScanDay.SelectedIndex = (int)DayOfWeek.Thursday; }

                            if (wTrigger.DaysOfWeek == DaysOfTheWeek.Friday)
                            { AutoScanDay.SelectedIndex = (int)DayOfWeek.Friday; }

                            if (wTrigger.DaysOfWeek == DaysOfTheWeek.Saturday)
                            { AutoScanDay.SelectedIndex = (int)DayOfWeek.Saturday; }

                            if (wTrigger.DaysOfWeek == DaysOfTheWeek.Sunday)
                            { AutoScanDay.SelectedIndex = (int)DayOfWeek.Sunday; }
                        }
                        else if (trg.TriggerType == TaskTriggerType.Monthly || trg.TriggerType == TaskTriggerType.MonthlyDOW)
                        {
                            AutoScanPeriod.SelectedIndex = (int)Schedule.Monthly;

                            if (trg.TriggerType == TaskTriggerType.Monthly)
                            {
                                string date = (trg as MonthlyTrigger).DaysOfMonth[0].ToString(CultureInfo.InvariantCulture);
                                int selectedIndex = 0;
                                int k = 0;
                                foreach (var item in AutoScanDate.Items)
                                {
                                    if (((ComboBoxItem)item).Content.ToString() == date)
                                    {
                                        selectedIndex = k;
                                        break;
                                    }
                                    k++;
                                }
                                AutoScanDate.SelectedIndex = selectedIndex;
                            }
                        }
                    }
                }
                else
                {
                    AutoScan.IsChecked = false;
                    AutoScanPeriod.SelectedIndex = 2;
                    AutoScanDate.Visibility = Visibility.Visible;
                    AutoScanDate.SelectedIndex = 0;
                    AutoScanDay.SelectedIndex = (int)DayOfWeek.NADay;
                    AutoScanTime.SelectedIndex = 11;
                }
            }
            catch { }
        }

        #region Enums

        public enum Schedule
        {
            Daily = 0,
            Weekly = 1,
            Monthly = 2
        }

        public enum DayOfWeek // Waheed 
        {
            Monday = 0,
            Tuesday = 1,
            Wednesday = 2,
            Thursday = 3,
            Friday = 4,
            Saturday = 5,
            Sunday = 6,
            NADay = 7
        }

        #endregion
    }
}
