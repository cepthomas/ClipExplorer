using System.Windows.Forms;

namespace ClipExplorer
{
    partial class MidiExplorer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MidiExplorer));
            this.barBar = new MidiLib.BarBar();
            this.sldTempo = new NBagOfUis.Slider();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.lbPatterns = new System.Windows.Forms.CheckedListBox();
            this.btnAllPatterns = new System.Windows.Forms.Button();
            this.btnClearPatterns = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.fileDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.allToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patternToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.midiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmbDrumChannel1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cmbDrumChannel2 = new System.Windows.Forms.ToolStripComboBox();
            this.btnLogMidi = new System.Windows.Forms.ToolStripButton();
            this.btnKillMidi = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
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
            this.barBar.Location = new System.Drawing.Point(10, 32);
            this.barBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.barBar.MarkerColor = System.Drawing.Color.Black;
            this.barBar.Name = "barBar";
            this.barBar.ProgressColor = System.Drawing.Color.White;
            this.barBar.Size = new System.Drawing.Size(576, 62);
            this.barBar.Snap = MidiLib.BarBar.SnapType.Bar;
            this.barBar.SubdivsPerBeat = 8;
            this.barBar.TabIndex = 70;
            this.toolTip.SetToolTip(this.barBar, "Time in bar:beat:subdivision");
            this.barBar.ZeroBased = false;
            // 
            // sldTempo
            // 
            this.sldTempo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sldTempo.DrawColor = System.Drawing.Color.White;
            this.sldTempo.Label = "BPM";
            this.sldTempo.Location = new System.Drawing.Point(10, 102);
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
            // lbPatterns
            // 
            this.lbPatterns.BackColor = System.Drawing.SystemColors.Control;
            this.lbPatterns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbPatterns.FormattingEnabled = true;
            this.lbPatterns.Location = new System.Drawing.Point(10, 198);
            this.lbPatterns.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lbPatterns.Name = "lbPatterns";
            this.lbPatterns.Size = new System.Drawing.Size(98, 332);
            this.lbPatterns.TabIndex = 88;
            this.toolTip.SetToolTip(this.lbPatterns, "Select patterns in style file");
            this.lbPatterns.SelectedIndexChanged += new System.EventHandler(this.Patterns_SelectedIndexChanged);
            // 
            // btnAllPatterns
            // 
            this.btnAllPatterns.Location = new System.Drawing.Point(10, 162);
            this.btnAllPatterns.Name = "btnAllPatterns";
            this.btnAllPatterns.Size = new System.Drawing.Size(43, 29);
            this.btnAllPatterns.TabIndex = 90;
            this.btnAllPatterns.Text = "+P";
            this.toolTip.SetToolTip(this.btnAllPatterns, "All patterns");
            this.btnAllPatterns.UseVisualStyleBackColor = true;
            this.btnAllPatterns.Click += new System.EventHandler(this.AllOrNone_Click);
            // 
            // btnClearPatterns
            // 
            this.btnClearPatterns.Location = new System.Drawing.Point(66, 162);
            this.btnClearPatterns.Name = "btnClearPatterns";
            this.btnClearPatterns.Size = new System.Drawing.Size(43, 29);
            this.btnClearPatterns.TabIndex = 91;
            this.btnClearPatterns.Text = "-P";
            this.toolTip.SetToolTip(this.btnClearPatterns, "Clear patterns");
            this.btnClearPatterns.UseVisualStyleBackColor = true;
            this.btnClearPatterns.Click += new System.EventHandler(this.AllOrNone_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileDropDown,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.cmbDrumChannel1,
            this.toolStripLabel2,
            this.cmbDrumChannel2,
            this.toolStripSeparator2,
            this.btnLogMidi,
            this.btnKillMidi,
            this.toolStripSeparator3
            });
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(594, 28);
            this.toolStrip1.TabIndex = 82;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // fileDropDown
            // 
            this.fileDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem,
            this.patternToolStripMenuItem,
            this.midiToolStripMenuItem});
            this.fileDropDown.Image = ((System.Drawing.Image)(resources.GetObject("fileDropDown.Image")));
            this.fileDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileDropDown.Name = "fileDropDown";
            this.fileDropDown.Size = new System.Drawing.Size(66, 25);
            this.fileDropDown.Text = "Export";
            // 
            // allToolStripMenuItem
            // 
            this.allToolStripMenuItem.Name = "allToolStripMenuItem";
            this.allToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.allToolStripMenuItem.Text = "All";
            this.allToolStripMenuItem.ToolTipText = "Export all midi events to text file";
            this.allToolStripMenuItem.Click += new System.EventHandler(this.Export_Click);
            // 
            // patternToolStripMenuItem
            // 
            this.patternToolStripMenuItem.Name = "patternToolStripMenuItem";
            this.patternToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.patternToolStripMenuItem.Text = "Pattern";
            this.patternToolStripMenuItem.ToolTipText = "Export selected patterns to text file";
            this.patternToolStripMenuItem.Click += new System.EventHandler(this.Export_Click);
            // 
            // midiToolStripMenuItem
            // 
            this.midiToolStripMenuItem.Name = "midiToolStripMenuItem";
            this.midiToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.midiToolStripMenuItem.Text = "Midi";
            this.midiToolStripMenuItem.ToolTipText = "Export selected patterns to midii files";
            this.midiToolStripMenuItem.Click += new System.EventHandler(this.Export_Click);
            // 
            // cmbDrumChannel1
            // 
            this.cmbDrumChannel1.AutoSize = false;
            this.cmbDrumChannel1.Name = "cmbDrumChannel1";
            this.cmbDrumChannel1.Size = new System.Drawing.Size(50, 28);
            this.cmbDrumChannel1.ToolTipText = "Drum Channel - main";
            this.cmbDrumChannel1.SelectedIndexChanged += new System.EventHandler(this.DrumChannel_SelectedIndexChanged);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(40, 25);
            this.toolStripLabel2.Text = "DC2:";
            // 
            // cmbDrumChannel2
            // 
            this.cmbDrumChannel2.AutoSize = false;
            this.cmbDrumChannel2.Name = "cmbDrumChannel2";
            this.cmbDrumChannel2.Size = new System.Drawing.Size(50, 28);
            this.cmbDrumChannel2.ToolTipText = "Drum channel - secndary";
            this.cmbDrumChannel2.SelectedIndexChanged += new System.EventHandler(this.DrumChannel_SelectedIndexChanged);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // btnLogMidi
            // 
            this.btnLogMidi.CheckOnClick = true;
            this.btnLogMidi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLogMidi.Image = global::ClipExplorer.Properties.Resources.glyphicons_170_record;
            this.btnLogMidi.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLogMidi.Name = "btnLogMidi";
            this.btnLogMidi.Size = new System.Drawing.Size(29, 25);
            this.btnLogMidi.Text = "toolStripButton1";
            this.btnLogMidi.ToolTipText = "Enable logging midi events";
            // 
            // btnKillMidi
            // 
            this.btnKillMidi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnKillMidi.Image = global::ClipExplorer.Properties.Resources.glyphicons_242_flash;
            this.btnKillMidi.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnKillMidi.Name = "btnKillMidi";
            this.btnKillMidi.Size = new System.Drawing.Size(29, 25);
            this.btnKillMidi.Text = "toolStripButton1";
            this.btnKillMidi.ToolTipText = "Kill all midi channels";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(40, 25);
            this.toolStripLabel1.Text = "DC1:";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // MidiExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.lbPatterns);
            this.Controls.Add(this.btnAllPatterns);
            this.Controls.Add(this.btnClearPatterns);
            this.Controls.Add(this.barBar);
            this.Controls.Add(this.sldTempo);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MidiExplorer";
            this.Size = new System.Drawing.Size(594, 544);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private ToolTip toolTip;
        private ToolStrip toolStrip1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private MidiLib.BarBar barBar;
        private NBagOfUis.Slider sldTempo;
        private System.Windows.Forms.CheckedListBox lbPatterns;
        private System.Windows.Forms.ToolStripButton btnLogMidi;
        private System.Windows.Forms.ToolStripButton btnKillMidi;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox cmbDrumChannel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox cmbDrumChannel2;
        private System.Windows.Forms.Button btnAllPatterns;
        private System.Windows.Forms.Button btnClearPatterns;
        private ToolStripDropDownButton fileDropDown;
        private ToolStripMenuItem allToolStripMenuItem;
        private ToolStripMenuItem patternToolStripMenuItem;
        private ToolStripMenuItem midiToolStripMenuItem;
    }
}
