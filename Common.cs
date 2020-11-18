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

    interface IPlayer
    {
        event EventHandler PlaybackCompleted;

        bool OpenFile(string fn);

        void Start();

        void Stop();

        void Rewind();

        void Close();

        float Volume { get; set; }

    }
}
