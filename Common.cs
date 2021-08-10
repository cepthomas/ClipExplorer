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
        #endregion

        #region Events
        /// <summary>Client needs to know when playing is done.</summary>
        event EventHandler PlaybackCompleted;

        /// <summary>Log me please.</summary>
        event EventHandler<string> Log;
        #endregion

        #region Public Functions
        /// <summary>Open playback file in player.</summary>
        /// <param name="fn">The file to open.</param>
        /// <returns>Success</returns>
        bool OpenFile(string fn);

        /// <summary>Start playback.</summary>
        void Play();

        /// <summary>Stop playback.</summary>
        void Stop();

        /// <summary>Stop playback and return to beginning.</summary>
        void Rewind();

        /// <summary>Settings have been edited.</summary>
        bool SettingsChanged();

        /// <summary>Utility.</summary>
        /// <param name="fn">Output filename</param>
        /// <returns>Formatted content. Empty if there was an error.</returns>
        List<string> Dump();

        /// <summary>Save the current selection to a file.</summary>
        /// <param name="fn">Output filename</param>
        /// <returns>success</returns>
        bool SaveSelection(string fn);
        #endregion
    }
}
