using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using System.Text.Json;
using System.Text.Json.Serialization;
using NAudio.Wave;
using NAudio.Midi;
using NBagOfTricks;
using NBagOfUis;
using MidiLib;


namespace ClipExplorer
{
    [Serializable]
    public class UserSettings : Settings
    {
        #region Persisted editable properties
        #region General        
        [DisplayName("Root Directories")]
        [Description("Where to look in order as they appear.")]
        [Category("\tGeneral")]
        [Browsable(true)]
        [Editor(typeof(StringListEditor), typeof(UITypeEditor))] // Ideally a multi folder picker.
        public List<string> RootDirs { get; set; } = new();

        [DisplayName("Dump To Clipboard")]
        [Description("Otherwise to file.")]
        [Category("\tGeneral")]
        [Browsable(true)]
        public bool DumpToClip { get; set; } = false;

        [DisplayName("Control Color")]
        [Description("Pick what you like.")]
        [Category("\tGeneral")]
        [Browsable(true)]
        [JsonConverter(typeof(JsonColorConverter))]
        public Color ControlColor { get; set; } = Color.MediumOrchid;
        #endregion

        #region Midi        
        [DisplayName("Midi Output Device")]
        [Description("How to play the midi files.")]
        [Category("Midi")]
        [Browsable(true)]
        [TypeConverter(typeof(FixedListTypeConverter))]
        public string MidiOutDevice { get; set; } = "Microsoft GS Wavetable Synth";

        [DisplayName("Time is 0-based")]
        [Description("Engineers prefer this to musician style.")]
        [Category("Midi")]
        [Browsable(true)]
        public bool ZeroBased { get; set; } = false;

        [DisplayName("Default Tempo")]
        [Description("Use this tempo if it's not in the file.")]
        [Category("Midi")]
        [Browsable(true)]
        public int DefaultTempo { get; set; } = 100;

        [DisplayName("Tempo Resolution")]
        [Description("Adjust tempo in UI.")]
        [Category("Midi")]
        [Browsable(true)]
        public int TempoResolution { get; set; } = 5;

        [DisplayName("Snap To Grid")]
        [Description("Snap to bar | beat | subdiv.")]
        [Category("Midi")]
        [Browsable(true)]
        public SnapType Snap { get; set; } = SnapType.Bar;
        #endregion

        #region Audio
        [DisplayName("Wave Output Device")]
        [Description("How to play the audio files.")]
        [Category("Audio")]
        [Browsable(true)]
        [TypeConverter(typeof(FixedListTypeConverter))]
        public string WavOutDevice { get; set; } = "Microsoft Sound Mapper";

        [DisplayName("Latency")]
        [Description("What's the hurry?")]
        [Category("Audio")]
        [Browsable(true)]
        [TypeConverter(typeof(FixedListTypeConverter))]
        public string Latency { get; set; } = "200";

        [DisplayName("Snap To Grid")]
        [Description("Snap to msec.")]
        [Category("Audio")]
        [Browsable(true)]
        public int SnapMsec { get; set; } = 100;
        #endregion
        #endregion

        #region Persisted Non-editable Properties
        [Browsable(false)]
        public bool Autoplay { get; set; } = true;

        [Browsable(false)]
        public bool Loop { get; set; } = false;

        [Browsable(false)]
        public double Volume { get; set; } = 0.5;
        #endregion

        #region Non-persisted Properties
        [Browsable(false)]
        public bool Valid { get; set; } = false;
        #endregion
    }

    #region Editing helpers
    /// <summary>Converter for selecting property value from known lists.</summary>
    public class FixedListTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }

        // Get the specific list based on the property name.
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string>? rec = null;

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
    #endregion
}
