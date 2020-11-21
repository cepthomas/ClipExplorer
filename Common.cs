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

    interface IPlayer : IDisposable
    {
        #region Properties
        /// <summary>Adjust master volume between 0.0 and 1.0.</summary>
        double Volume { get; set; }

        /// <summary>Current play time/position.</summary>
        double CurrentTime { get; set; }

        /// <summary>Play length in same units as Position.</summary>
        double Length { get; }
        #endregion

        #region Events
        /// <summary>Client needs to know when playing is done.</summary>
        event EventHandler PlaybackCompleted;
        #endregion

        #region Public Functions
        /// <summary>Open playback file in player.</summary>
        /// <param name="fn">The file to open.</param>
        /// <returns>Success</returns>
        bool OpenFile(string fn);

        /// <summary>Start playback.</summary>
        void Start();

        /// <summary>Stop playback.</summary>
        void Stop();

        /// <summary>Stop playback and return to beginning.</summary>
        void Rewind();

        /// <summary>Clean up internal resources.</summary>
        void Close();
        #endregion
    }
}
