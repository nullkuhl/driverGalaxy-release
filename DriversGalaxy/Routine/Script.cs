using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mScriptable;
using System.Runtime.InteropServices;
using System.Collections;

namespace DriversGalaxy
{
    public class Script
    {

        #region Private Members

        private string m_getRestorePoint = "Set CDatum = CreateObject(\"WbemScripting.SWbemDateTime\")" + Environment.NewLine +
                                "strComputer = \".\"" + Environment.NewLine +
                                "Set oWm = GetObject(\"winmgmts:\\\\\" & strComputer & \"\\root\\default\") " + Environment.NewLine +
                                "Set cli = oWm.ExecQuery(\"Select * from SystemRestore\") " + Environment.NewLine +
                                "For Each objItem in cli " + Environment.NewLine +
                                "   CDatum.Value = objItem.CreationTime " + Environment.NewLine +
                                "   dtmCreationTime = CDatum.GetVarDate " + Environment.NewLine +
                                "   return objItem.SequenceNumber, dtmCreationTime & \"|\" & objItem.Description" + Environment.NewLine +
                                "Next ";

        private string m_restoreSystem = "If a = \"\" Then  " + Environment.NewLine +
                                        "Set a = Nothing  " + Environment.NewLine +
                                        "WScript.Quit " + Environment.NewLine +
                                        "End If  " + Environment.NewLine +
                                        "strComputer = \".\" " + Environment.NewLine +
                                        "Set objWMIService = GetObject(\"winmgmts:\" & \"{impersonationLevel=impersonate}!\\\\\" & strComputer & \"\\root\\default\")" + Environment.NewLine +
                                        "Set objItem = objWMIService.Get(\"SystemRestore\")" + Environment.NewLine +
                                        "errResults = objItem.Restore(a) " + Environment.NewLine +
                                        "Set OpSysSet = GetObject(\"winmgmts:{(Shutdown)}//./root/cimv2\").ExecQuery(\"select * from Win32_OperatingSystem where Primary=true\") " + Environment.NewLine +
                                        "For Each OpSys In OpSysSet " + Environment.NewLine +
                                        "OpSys.Reboot()" + Environment.NewLine +
                                        "Next ";

        private string m_createRestorePoint = "strComputer = \".\" " + Environment.NewLine +
                                              "Set objWMIService = GetObject(\"winmgmts:\\\\\" & strComputer & \"\\root\\default\") " + Environment.NewLine +
                                              "Set objItem = objWMIService.Get(\"SystemRestore\")" + Environment.NewLine +
                                              "CSRP = objItem.createrestorepoint (\"Freemium Utility Restore Point\", 0, 100)"+ Environment.NewLine +
                                              "return 1,1";

       
        #endregion

        #region Public Properties

        public string GetRestorePoints
        {
            get { return m_getRestorePoint; }
            set { m_getRestorePoint = value; }
        }

        public string RestoreSystem
        {
            get { return m_restoreSystem; }
            set { m_restoreSystem = value; }
        }

        public string CreateRestorePoint
        {
            get { return m_createRestorePoint; }
            set { m_createRestorePoint = value; }
        }

        #endregion

    }

    public class SystemRestore
    {

        mScript scriptManager = new mScript();
        Script myScript = new Script();
        [DllImport("Srclient.dll")]
        public static extern int SRRemoveRestorePoint(int index);


        public Hashtable GetRestorePoints()
        {
            Hashtable result = new Hashtable();
            try 
            {
                scriptManager.setScript(myScript.GetRestorePoints);
                result = scriptManager.runScript();
                
            }
            catch { }
            return result;
        }

        public void CreateRestorePoint()
        {
            try
            {
                scriptManager.setScript(myScript.CreateRestorePoint);
                Hashtable results = scriptManager.runScript();
            }
            catch { }
        }

        public void RestoreSystem(int sequenceNo)
        {
            try
            {
                scriptManager.setScript("a=" + sequenceNo + ":" + myScript.RestoreSystem);
                Hashtable results = scriptManager.runScript();
            }
            catch { }
           
        }

        public void DeleteRestorePoint(int sequenceNo)
        {
            try
            {
                SRRemoveRestorePoint(sequenceNo);
            }
            catch { }
           
        }
    }


}
