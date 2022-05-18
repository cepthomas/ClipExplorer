using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using NBagOfTricks;


namespace ClipExplorer
{
    public class Common
    {
        #region Constants
        public const double MIN_VOLUME = 0.0;
        public const double MAX_VOLUME = 2.0;
        public const double DEFAULT_VOLUME = 0.8;
        #endregion



        /// <summary>Current global user settings.</summary>
        public static UserSettings Settings { get; set; } = new UserSettings();

        /// <summary>Where to put things.</summary>
        public static string ExportPath { get; set; }


        ///// <summary>Shared log file.</summary>
        //public static string LogFileName { get { return MiscUtils.GetAppDataDir("ClipPlayer", "Ephemera") + @"\mplog.txt"; } }
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
