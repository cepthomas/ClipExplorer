using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using Newtonsoft.Json;
using NAudio.Wave;
using NAudio.Midi;
using NBagOfTricks.UI;


namespace ClipExplorer
{
    [Serializable]
    public class UserSettings
    {
        #region Persisted editable properties
        [DisplayName("Root Directories")]
        [Description("Where to look in order as they appear.")]
        [Category("Navigator")]
        [Browsable(true)]
        [Editor(typeof(ListEditor), typeof(UITypeEditor))]
        public List<string> RootDirs { get; set; } = new List<string>();

        [DisplayName("Autoplay Files")]
        [Description("Single click plays file otherwise double click.")]
        [Category("Navigator")]
        [Browsable(true)]
        public bool Autoplay { get; set; } = true;

        [DisplayName("All Tags")]
        [Description("All possible tags.")]
        [Category("Navigator")]
        [Browsable(true)]
        [Editor(typeof(ListEditor), typeof(UITypeEditor))]
        public List<string> AllTags { get; set; } = new List<string>();

        [DisplayName("Wave Output Device")]
        [Description("Where to go.")]
        [Category("Audio")]
        [Browsable(true)]
        [TypeConverter(typeof(FixedListTypeConverter))]
        public string WavOutDevice { get; set; } = "";

        [DisplayName("Latency")]
        [Description("What's the hurry?")]
        [Category("Audio")]
        [Browsable(true)]
        [TypeConverter(typeof(FixedListTypeConverter))]
        public string Latency { get; set; } = "200";

        [DisplayName("Midi Output Device")]
        [Description("Where to go.")]
        [Category("Midi")]
        [Browsable(true)]
        [TypeConverter(typeof(FixedListTypeConverter))]
        public string MidiOutDevice { get; set; } = "";

        [DisplayName("Drum Channel")]
        [Description("Some files don't use ch10 so map from these.")]
        [Category("Midi")]
        [Browsable(true)]
        public int DrumChannel { get; set; } = 1;

        [DisplayName("Enable Mapping Drum Channel")]
        [Description("See Drum Channel.")]
        [Category("Midi")]
        [Browsable(true)]
        public bool MapDrumChannel { get; set; } = false;

        [DisplayName("Snap To Grid")]
        [Description("Snap to bar/beat/tick.")]
        [Category("Midi")]
        [Browsable(true)]
        public BarBar.SnapType Snap { get; set; } = BarBar.SnapType.Bar;
        #endregion

        #region Persisted Non-editable Properties
        [Browsable(false)]
        public Rectangle MainFormInfo { get; set; } = new Rectangle(50, 50, 500, 400);

        [Browsable(false)]
        public double Volume { get; set; } = 0.5;

        [Browsable(false)]
        public List<string> RecentFiles { get; set; } = new List<string>();

        [Browsable(false)]
        public Dictionary<string, string> TaggedPaths { get; set; } = new Dictionary<string, string>();
        #endregion

        #region Fields
        /// <summary>The file name.</summary>
        string _fn = "???";
        #endregion

        #region Persistence
        /// <summary>Save object to file.</summary>
        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_fn, json);
        }

        /// <summary>Create object from file.</summary>
        public static void Load(string appDir)
        {
            Common.Settings = null;
            string fn = Path.Combine(appDir, "settings.json");

            if (File.Exists(fn))
            {
                string json = File.ReadAllText(fn);
                Common.Settings = JsonConvert.DeserializeObject<UserSettings>(json);

                // Clean up any bad file names.
                Common.Settings.RecentFiles.RemoveAll(f => !File.Exists(f));

                Common.Settings._fn = fn;
            }
            else
            {
                // Doesn't exist, create a new one.
                Common.Settings = new UserSettings
                {
                    _fn = fn
                };
            }
        }
        #endregion
    }

    /// <summary>Converter for selecting property value from known lists.</summary>
    public class FixedListTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }

        // Get the specific list based on the property name.
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> rec = null;

            switch (context.PropertyDescriptor.Name)
            {
                case "Latency":
                    rec = new List<string>() { "25", "50", "100", "150", "200", "300", "400", "500" };
                    break;

                case "WavOutDevice":
                    rec = new List<string>();
                    for (int id = -1; id < WaveOut.DeviceCount; id++) // –1 indicates the default output device, while 0 is the first output device
                    {
                        var cap = WaveOut.GetCapabilities(id);
                        rec.Add(cap.ProductName);
                    }
                    break;

                case "MidiOutDevice":
                    rec = new List<string>();
                    for (int devindex = 0; devindex < MidiOut.NumberOfDevices; devindex++)
                    {
                        rec.Add(MidiOut.DeviceInfo(devindex).ProductName);
                    }
                    break;
            }

            return new StandardValuesCollection(rec);
        }
    }
}
