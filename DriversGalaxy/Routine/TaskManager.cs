using System;
using Microsoft.Win32.TaskScheduler;

namespace DriversGalaxy.Routine
{
    public class TaskManager
    {
        #region Public Methods
     
        /// <summary>
        /// Gets task by its name
        /// </summary>
        /// <param name="taskName">task name</param>
        /// <returns></returns>
        public static Task GetTaskByName(string taskName)
        {
            Task task = null;
            try
            {
                TaskService service = new TaskService();
                task = service.FindTask(taskName, true);
                
            }
            catch { }

            return task;
        }

        /// <summary>
        /// Check if the task in schedule
        /// </summary>
        /// <param name="taskName">task name</param>
        /// <returns>true - if the task is scheduled, false - otherwise</returns>
        public static bool IsTaskScheduled(string taskName)
        {
            Task task = GetTaskByName(taskName);
            if (task != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets task description
        /// </summary>
        /// <param name="taskName">task name</param>
        /// <returns></returns>
        public static string GetTaskDescription(string taskName)
        {
            string taskDescription = "";
            Task task = GetTaskByName(taskName);
            if (task != null)
            {
                foreach (Trigger trg in task.Definition.Triggers)
                {
                    taskDescription += trg.ToString() + ". ";
                }
            }
            if (taskDescription.IndexOf(", starting") > 0)
                return taskDescription.Substring(0, (taskDescription.Length - taskDescription.IndexOf(", starting")) + 1);
            else
                return taskDescription;
        
        }

        public static void DeleteTask(string taskName)
        {
            Task task = null;
            try
            {
                TaskService service = new TaskService();
                task = service.FindTask(taskName, true);

                if (task != null)
                    service.RootFolder.DeleteTask(taskName);
            }
            catch { }
        }


        /// <summary>
        /// Updates task status
        /// </summary>
        /// <param name="taskName">task name</param>
        /// <param name="isEnabled">status of task - enabled/disabled</param>
        public static void UpdateTaskStatus(string taskName, bool isEnabled)
        {
            try
            {
                TaskService service = new TaskService();
                Task task = service.FindTask(taskName, true);

                if (task != null)
                {
                    task.Enabled = isEnabled;
                    task.RegisterChanges();
                }
            }
            catch{}
        }

        /// <summary>
        /// Creates default task in the Task Scheduler service
        /// </summary>
        /// <param name="taskName">task name</param>
        /// <param name="isEnabled">true - if enabled, false - otherwise</param>
        public static void CreateDefaultTask(string taskName, bool isEnabled)
        {
            try
            {
                DeleteTask(taskName);

                TaskService service = new TaskService();
                TaskDefinition td = service.NewTask();
                
                td.Settings.Enabled = isEnabled;
                td.RegistrationInfo.Description = "DriversGalaxy";
                td.Principal.RunLevel = TaskRunLevel.Highest;

                // Create an action that will launch Notepad whenever the trigger fires
                td.Actions.Add(new ExecAction(Environment.CurrentDirectory + "\\1Click.exe", null, Environment.CurrentDirectory));

                WeeklyTrigger mTrigger = new WeeklyTrigger();
                mTrigger.DaysOfWeek = DaysOfTheWeek.Friday;
                mTrigger.StartBoundary = DateTime.Today.AddHours(12);
               
                mTrigger.Repetition.StopAtDurationEnd = false;
                td.Triggers.Add(mTrigger);
                // Register the task in the root folder
                service.RootFolder.RegisterTaskDefinition(taskName, td);

            }
            catch { }
         
        }
        #endregion
    }
}
