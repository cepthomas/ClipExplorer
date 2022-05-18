using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipExplorer
{

    public enum PlayState { Stopped = 0, Playing = 1, Complete = 2 }

    interface IPlayer : IDisposable
    {
        #region Properties
        /// <summary>Current master volume - between MIN_VOLUME and MAX_VOLUME. Default is DEFAULT_VOLUME.</summary>
        double Volume { get; set; }
        #endregion


        /// <summary>What are we doing right now.</summary>
        PlayState State { get; set; }


        #region Events
        /// <summary>Client needs to know when playing is done.</summary>
        event EventHandler PlaybackCompleted;

        /// <summary>Log me please.</summary>
        event EventHandler<LogEventArgs> Log;

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
        #endregion
    }
}
