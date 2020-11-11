﻿using System;
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
using NBagOfTricks.UI;


namespace ClipExplorer
{
    [Serializable]
    public class UserSettings
    {
        #region Persisted editable properties
        [DisplayName("Root Directory")]
        [Description("Where to start.")]
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
        public List<string> AllTags { get; set; } = new List<string>(); //TODOC fancy editor maybe

        [DisplayName("Output Device")]
        [Description("Where to go.")]
        [Category("Audio")]
        [Browsable(true)]
        [TypeConverter(typeof(OutputDeviceTypeConverter))]
        public string OutputDevice { get; set; } = "";

        [DisplayName("Latency")]
        [Description("What's the hurry?")]
        [Category("Audio")]
        [Browsable(true)]
        [TypeConverter(typeof(LatencyTypeConverter))]
        public string Latency { get; set; } = "200";
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

        /// <summary>Current global user settings.</summary>
        public static UserSettings TheSettings { get; set; } = new UserSettings();

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
            TheSettings = null;
            string fn = Path.Combine(appDir, "settings.json");

            if (File.Exists(fn))
            {
                string json = File.ReadAllText(fn);
                TheSettings = JsonConvert.DeserializeObject<UserSettings>(json);

                // Clean up any bad file names.
                TheSettings.RecentFiles.RemoveAll(f => !File.Exists(f));

                TheSettings._fn = fn;
            }
            else
            {
                // Doesn't exist, create a new one.
                TheSettings = new UserSettings
                {
                    _fn = fn
                };
            }
        }
        #endregion
    }

    /// <summary>Converter for selecting property value from known lists.</summary>
    public class LatencyTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }

        /// Get the list using the property name as key.
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new List<string>() { "25", "50", "100", "150", "200", "300", "400", "500" });
        }
    }

    /// <summary>Converter for selecting property value from known lists.</summary>
    public class OutputDeviceTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }

        /// Get the list using the property name as key.
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> rec = new List<string>();
            // –1 indicates the default output device, while 0 is the first output device
            for (int id = 0; id < WaveOut.DeviceCount; id++)
            {
                var cap = WaveOut.GetCapabilities(id);
                rec.Add(cap.ProductName);
            }
            return new StandardValuesCollection(rec);
        }
    }
}
