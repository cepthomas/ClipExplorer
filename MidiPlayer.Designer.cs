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
            this.btnKill = new System.Windows.Forms.Button();
            this.chkLogMidi = new System.Windows.Forms.CheckBox();
            this.cmbDrumChannel = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
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
            this.barBar.ZeroBased = false;
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
            this.sldTempo.DrawColor = System.Drawing.Color.White;
            this.sldTempo.Label = "BPM";
            this.sldTempo.Location = new System.Drawing.Point(2, 70);
            this.sldTempo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.sldTempo.Maximum = 200D;
            this.sldTempo.Minimum = 50D;
            this.sldTempo.Name = "sldTempo";
            this.sldTempo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldTempo.Resolution = 5D;
            this.sldTempo.Size = new System.Drawing.Size(98, 53);
            this.sldTempo.TabIndex = 63;
            this.toolTip.SetToolTip(this.sldTempo, "Tempo adjuster");
            this.sldTempo.Value = 100D;
            this.sldTempo.ValueChanged += new System.EventHandler(this.Tempo_ValueChanged);
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
            // cmbDrumChannel
            // 
            this.cmbDrumChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDrumChannel.FormattingEnabled = true;
            this.cmbDrumChannel.Location = new System.Drawing.Point(38, 131);
            this.cmbDrumChannel.Name = "cmbDrumChannel";
            this.cmbDrumChannel.Size = new System.Drawing.Size(62, 28);
            this.cmbDrumChannel.TabIndex = 81;
            this.toolTip.SetToolTip(this.cmbDrumChannel, "Drums on this channel");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 135);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 20);
            this.label1.TabIndex = 80;
            this.label1.Text = "DC:";
            // 
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbDrumChannel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnKill);
            this.Controls.Add(this.chkLogMidi);
            this.Controls.Add(this.barBar);
            this.Controls.Add(this.cgChannels);
            this.Controls.Add(this.sldTempo);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MidiPlayer";
            this.Size = new System.Drawing.Size(588, 285);
            this.Load += new System.EventHandler(this.MidiPlayer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private NBagOfUis.Slider sldTempo;
        private NBagOfUis.ClickGrid cgChannels;
        private NBagOfUis.BarBar barBar;
        private ToolTip toolTip;
        private CheckBox chkLogMidi;
        private Button btnKill;
        private Label label1;
        private ComboBox cmbDrumChannel;
    }
}
