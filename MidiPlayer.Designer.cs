using System.Windows.Forms;

namespace ClipExplorer
{
    partial class MidiPlayer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.barBar = new NBagOfUis.BarBar();
            this.cgChannels = new NBagOfUis.ClickGrid();
            this.sldTempo = new NBagOfUis.Slider();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.chkDrumsOn1 = new System.Windows.Forms.CheckBox();
            this.btnKill = new System.Windows.Forms.Button();
            this.chkLogMidi = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // barBar
            // 
            this.barBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.barBar.BeatsPerBar = 4;
            this.barBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.barBar.FontLarge = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.barBar.FontSmall = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.barBar.Location = new System.Drawing.Point(2, 0);
            this.barBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.barBar.MarkerColor = System.Drawing.Color.Black;
            this.barBar.Name = "barBar";
            this.barBar.ProgressColor = System.Drawing.Color.White;
            this.barBar.Size = new System.Drawing.Size(570, 62);
            this.barBar.Snap = NBagOfUis.BarBar.SnapType.Bar;
            this.barBar.SubdivsPerBeat = 8;
            this.barBar.TabIndex = 70;
            this.toolTip.SetToolTip(this.barBar, "Time in bar:beat:subdivision");
            // 
            // cgChannels
            // 
            this.cgChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cgChannels.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cgChannels.Location = new System.Drawing.Point(126, 70);
            this.cgChannels.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cgChannels.Name = "cgChannels";
            this.cgChannels.Size = new System.Drawing.Size(448, 203);
            this.cgChannels.TabIndex = 69;
            this.toolTip.SetToolTip(this.cgChannels, "Midi channels with mute/solo");
            this.cgChannels.IndicatorEvent += new System.EventHandler<NBagOfUis.IndicatorEventArgs>(this.Channels_IndicatorEvent);
            // 
            // sldTempo
            // 
            this.sldTempo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sldTempo.DecPlaces = 0;
            this.sldTempo.DrawColor = System.Drawing.Color.White;
            this.sldTempo.Label = "BPM";
            this.sldTempo.Location = new System.Drawing.Point(2, 70);
            this.sldTempo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.sldTempo.Maximum = 200D;
            this.sldTempo.Minimum = 50D;
            this.sldTempo.Name = "sldTempo";
            this.sldTempo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldTempo.ResetValue = 50D;
            this.sldTempo.Size = new System.Drawing.Size(98, 53);
            this.sldTempo.TabIndex = 63;
            this.toolTip.SetToolTip(this.sldTempo, "Tempo adjuster");
            this.sldTempo.Value = 100D;
            this.sldTempo.ValueChanged += new System.EventHandler(this.Tempo_ValueChanged);
            // 
            // chkDrumsOn1
            // 
            this.chkDrumsOn1.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkDrumsOn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDrumsOn1.Location = new System.Drawing.Point(2, 136);
            this.chkDrumsOn1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDrumsOn1.Name = "chkDrumsOn1";
            this.chkDrumsOn1.Size = new System.Drawing.Size(98, 34);
            this.chkDrumsOn1.TabIndex = 77;
            this.chkDrumsOn1.Text = "Drums on 1";
            this.toolTip.SetToolTip(this.chkDrumsOn1, "Drums are on channel 1");
            this.chkDrumsOn1.UseVisualStyleBackColor = true;
            this.chkDrumsOn1.CheckedChanged += new System.EventHandler(this.DrumsOn1_CheckedChanged);
            // 
            // btnKill
            // 
            this.btnKill.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKill.Image = global::ClipExplorer.Properties.Resources.glyphicons_242_flash;
            this.btnKill.Location = new System.Drawing.Point(46, 178);
            this.btnKill.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnKill.Name = "btnKill";
            this.btnKill.Size = new System.Drawing.Size(32, 40);
            this.btnKill.TabIndex = 79;
            this.toolTip.SetToolTip(this.btnKill, "Kill all midi channels");
            this.btnKill.UseVisualStyleBackColor = true;
            this.btnKill.Click += new System.EventHandler(this.Kill_Click);
            // 
            // chkLogMidi
            // 
            this.chkLogMidi.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkLogMidi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkLogMidi.Image = global::ClipExplorer.Properties.Resources.glyphicons_170_record;
            this.chkLogMidi.Location = new System.Drawing.Point(2, 178);
            this.chkLogMidi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLogMidi.Name = "chkLogMidi";
            this.chkLogMidi.Size = new System.Drawing.Size(32, 40);
            this.chkLogMidi.TabIndex = 76;
            this.toolTip.SetToolTip(this.chkLogMidi, "Enable logging midi events");
            this.chkLogMidi.UseVisualStyleBackColor = true;
            // 
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnKill);
            this.Controls.Add(this.chkDrumsOn1);
            this.Controls.Add(this.chkLogMidi);
            this.Controls.Add(this.barBar);
            this.Controls.Add(this.cgChannels);
            this.Controls.Add(this.sldTempo);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MidiPlayer";
            this.Size = new System.Drawing.Size(588, 285);
            this.Load += new System.EventHandler(this.MidiPlayer_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private NBagOfUis.Slider sldTempo;
        private NBagOfUis.ClickGrid cgChannels;
        private NBagOfUis.BarBar barBar;
        private ToolTip toolTip;
        private CheckBox chkLogMidi;
        private CheckBox chkDrumsOn1;
        private Button btnKill;
    }
}
