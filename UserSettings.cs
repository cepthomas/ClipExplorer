using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using NAudio.CoreAudioApi;
using NBagOfTricks.Utils;
using Newtonsoft.Json;


namespace ClipExplorer
{
    [Serializable]
    public class UserSettings
    {
        #region Persisted editable properties
        //TODOC Support multiple dirs.
        [DisplayName("Root Directoriy"), Description("Where to start."), Browsable(true)]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string RootDir { get; set; } = "";

        [DisplayName("Latency"), Description("What's the hurry?"), Browsable(true)]
        [TypeConverter(typeof(SystemFixedListTypeConverter))]
        public string Latency { get; set; } = "200";

        [DisplayName("Output Device"), Description("Where to go."), Browsable(true)]
        [TypeConverter(typeof(SystemFixedListTypeConverter))]
        public string OutputDevice { get; set; } = "";

        [DisplayName("Wasapi Exclusive Mode"), Description("Mode."), Browsable(true)]
        public bool WasapiExclusive { get; set; } = true;

        [DisplayName("Wasapi Event Callback"), Description("Mode."), Browsable(true)]
        public bool WasapiEventCallback { get; set; } = false;
        #endregion

        #region Persisted non-editable properties
        [Browsable(false)]
        public FormInfo MainFormInfo { get; set; } = new FormInfo();

        [Browsable(false)]
        public int Volume { get; set; } = 40;

        [Browsable(false)]
        public int Speed { get; set; } = 100;

        [Browsable(false)]
        public List<string> RecentFiles { get; set; } = new List<string>();
        #endregion

        #region Classes
        /// <summary>General purpose container for persistence.</summary>
        [Serializable]
        public class FormInfo
        {
            public int X { get; set; } = 50;
            public int Y { get; set; } = 50;
            public int Width { get; set; } = 1000;
            public int Height { get; set; } = 700;

            public void FromForm(Form f)
            {
                X = f.Location.X;
                Y = f.Location.Y;
                Width = f.Width;
                Height = f.Height;
            }
        }
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
    public class SystemFixedListTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }

        /// Get the list using the property name as key.
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> rec = null;

            switch (context.PropertyDescriptor.Name)
            {
                case "Latency":
                    rec = new List<string>() { "25", "50", "100", "150", "200", "300", "400", "500" };
                    break;

                case "OutputDevice":
                    rec = new List<string>();
                    var endPoints = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    endPoints.ForEach(e => rec.Add(e.FriendlyName));
                    break;
            }

            StandardValuesCollection coll = new StandardValuesCollection(rec);
            return coll;
        }
    }
}
