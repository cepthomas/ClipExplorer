using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipExplorer
{
    public class Common
    {
        /// <summary>Current global user settings.</summary>
        public static UserSettings Settings { get; set; } = new UserSettings();
    }

    /// <summary>Player has something to say or show.</summary>
    public class LogEventArgs : EventArgs
    {
        public string Category { get; private set; } = "";
        public string Message { get; private set; } = "";
        public LogEventArgs(string cat, string msg)
        {
            Category = cat;
            Message = msg;
        }
    }
}
