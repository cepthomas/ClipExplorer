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
        /// <summary>Current global user settings.</summary>
        public static UserSettings Settings { get; set; } = new UserSettings();

        /// <summary>Where to put things.</summary>
        public static string OutPath { get; set; } = "";
        #endregion
    }
}
