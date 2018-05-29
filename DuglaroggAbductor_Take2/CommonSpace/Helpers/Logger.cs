using Sims3.Gameplay.Utilities;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.CommonSpace.Helpers
{
    public static class Logger
    {
        private static AlarmHandle sReporterHandle = AlarmHandle.kInvalidHandle;
        private static List<string> sLog;
        private static DateTime sLastReported = DateTime.MinValue;
        private static bool sWorldLoaded = false;

        private static void AddAlarm()
        {
            if (sWorldLoaded && sReporterHandle == AlarmHandle.kInvalidHandle)
            {
                sReporterHandle = 
                    AlarmManager.Global.AddAlarmRepeating(1f, TimeUnit.Minutes,
                    new AlarmTimerCallback(Report), 1f, TimeUnit.Minutes,
                    "Logger Report Alarm", AlarmType.NeverPersisted, null);
            }
        }

        public static void Append(string str)
        {
            if (sLog == null)
            {
                sLog = new List<string>();
            }

            sLog.Add(str);
            AddAlarm();
        }

        public static void OnWorldLoadFinished(object sender, EventArgs evtArgs)
        {
            sWorldLoaded = true;

            if (sLog == null)
            {
                return;
            }

            AddAlarm();
        }

        public static void Report()
        {
            if (sLog != null)
            {
                foreach (string current in sLog)
                {
                    StyledNotification.Show(new StyledNotification.Format(current, StyledNotification.NotificationStyle.kDebugAlert));
                }
            }
        }

        public static void WriteExceptionLog(Exception e, object sender, string message)
        {
            string text = "\nError: ";
            text += !string.IsNullOrEmpty(message) ? message : "No error message given.";
            text += "\n\n ----- Sender: ";
            text += sender != null ? sender.GetType().FullName : "Null or Static";
            text += "\n\n ----- Exception: " + e.ToString();
            ScriptError error = new ScriptError(null, new Exception(text), 0);
            error.WriteMiniScriptError();

            if (sLastReported == DateTime.MinValue || (DateTime.Now - sLastReported).Minutes > 1)
            {
                Append("Script Error Caught. Check user files for specifics.");
            }

            sLastReported = DateTime.Now;
        }
    }
}
