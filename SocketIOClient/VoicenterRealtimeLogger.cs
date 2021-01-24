using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoicenterRealtimeAPI
{
    public enum LogLevel { Debug = 1,  Info , Error, Fatal }
    public class VoicenterRealtimeLogger : EventArgs
    {
        public Exception ex { get; set; }
        public string message { get; set; }

        public LogLevel level { get; set; }
    }
    public class Logger
    {
        public static LogLevel logLevel { get; set; }
        public static event EventHandler<VoicenterRealtimeLogger> onLog;
        public static void log(object sender, VoicenterRealtimeLogger e)
        {
            if(logLevel <= e.level)
            {
                onLog(sender, e);
            }
            
        }
    }
}
