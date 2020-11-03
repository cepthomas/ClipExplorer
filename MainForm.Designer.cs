namespace ClipExplorer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        ///// <summary>
        ///// Clean up any resources being used.
        ///// </summary>
        ///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.fileDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.labelCurrentTime = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.labelTotalTime = new System.Windows.Forms.ToolStripLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.navigator = new ClipExplorer.Navigator();
            this.chkLoop = new System.Windows.Forms.CheckBox();
            this.waveformPainter2 = new NAudio.Gui.WaveformPainter();
            this.waveformPainter1 = new NAudio.Gui.WaveformPainter();
            this.volumeMeter2 = new NAudio.Gui.VolumeMeter();
            this.volumeMeter1 = new NAudio.Gui.VolumeMeter();
            this.trackBarPosition = new System.Windows.Forms.TrackBar();
            this.sldVolume = new NBagOfTricks.UI.Slider();
            this.potSpeed = new NBagOfTricks.UI.Pot();
            this.btnRewind = new System.Windows.Forms.Button();
            this.chkPlay = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPosition)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileDropDownButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1284, 27);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // fileDropDownButton
            // 
            this.fileDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.recentToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.fileDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileDropDownButton.Name = "fileDropDownButton";
            this.fileDropDownButton.Size = new System.Drawing.Size(46, 24);
            this.fileDropDownButton.Text = "File";
            this.fileDropDownButton.ToolTipText = "File operations";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.Open_Click);
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.recentToolStripMenuItem.Text = "Recent";
            this.recentToolStripMenuItem.Click += new System.EventHandler(this.Recent_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.Settings_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.About_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(97, 24);
            this.toolStripLabel1.Text = "Current Time:";
            // 
            // labelCurrentTime
            // 
            this.labelCurrentTime.Name = "labelCurrentTime";
            this.labelCurrentTime.Size = new System.Drawing.Size(44, 24);
            this.labelCurrentTime.Text = "00:00";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(82, 24);
            this.toolStripLabel3.Text = "Total Time:";
            // 
            // labelTotalTime
            // 
            this.labelTotalTime.Name = "labelTotalTime";
            this.labelTotalTime.Size = new System.Drawing.Size(44, 24);
            this.labelTotalTime.Text = "00:00";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.navigator);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chkLoop);
            this.splitContainer1.Panel2.Controls.Add(this.waveformPainter2);
            this.splitContainer1.Panel2.Controls.Add(this.waveformPainter1);
            this.splitContainer1.Panel2.Controls.Add(this.volumeMeter2);
            this.splitContainer1.Panel2.Controls.Add(this.volumeMeter1);
            this.splitContainer1.Panel2.Controls.Add(this.trackBarPosition);
            this.splitContainer1.Panel2.Controls.Add(this.sldVolume);
            this.splitContainer1.Panel2.Controls.Add(this.potSpeed);
            this.splitContainer1.Panel2.Controls.Add(this.btnRewind);
            this.splitContainer1.Panel2.Controls.Add(this.chkPlay);
            this.splitContainer1.Size = new System.Drawing.Size(1284, 608);
            this.splitContainer1.SplitterDistance = 427;
            this.splitContainer1.TabIndex = 1;
            // 
            // navigator
            // 
            this.navigator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navigator.Location = new System.Drawing.Point(0, 0);
            this.navigator.Name = "navigator";
            this.navigator.Size = new System.Drawing.Size(427, 608);
            this.navigator.TabIndex = 0;
            // 
            // chkLoop
            // 
            this.chkLoop.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkLoop.FlatAppearance.BorderSize = 0;
            this.chkLoop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkLoop.Image = global::ClipExplorer.Properties.Resources.glyphicons_366_restart;
            this.chkLoop.Location = new System.Drawing.Point(115, 16);
            this.chkLoop.Margin = new System.Windows.Forms.Padding(4);
            this.chkLoop.Name = "chkLoop";
            this.chkLoop.Size = new System.Drawing.Size(43, 39);
            this.chkLoop.TabIndex = 55;
            this.chkLoop.UseVisualStyleBackColor = false;
            // 
            // waveformPainter2
            // 
            this.waveformPainter2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveformPainter2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.waveformPainter2.ForeColor = System.Drawing.Color.SaddleBrown;
            this.waveformPainter2.Location = new System.Drawing.Point(66, 298);
            this.waveformPainter2.Margin = new System.Windows.Forms.Padding(4);
            this.waveformPainter2.Name = "waveformPainter2";
            this.waveformPainter2.Size = new System.Drawing.Size(744, 74);
            this.waveformPainter2.TabIndex = 51;
            this.waveformPainter2.Text = "waveformPainter1";
            // 
            // waveformPainter1
            // 
            this.waveformPainter1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveformPainter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.waveformPainter1.ForeColor = System.Drawing.Color.SaddleBrown;
            this.waveformPainter1.Location = new System.Drawing.Point(66, 220);
            this.waveformPainter1.Margin = new System.Windows.Forms.Padding(4);
            this.waveformPainter1.Name = "waveformPainter1";
            this.waveformPainter1.Size = new System.Drawing.Size(744, 74);
            this.waveformPainter1.TabIndex = 52;
            this.waveformPainter1.Text = "waveformPainter1";
            // 
            // volumeMeter2
            // 
            this.volumeMeter2.Amplitude = 0F;
            this.volumeMeter2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.volumeMeter2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.volumeMeter2.Location = new System.Drawing.Point(443, 16);
            this.volumeMeter2.Margin = new System.Windows.Forms.Padding(4);
            this.volumeMeter2.MaxDb = 3F;
            this.volumeMeter2.MinDb = -60F;
            this.volumeMeter2.Name = "volumeMeter2";
            this.volumeMeter2.Size = new System.Drawing.Size(19, 126);
            this.volumeMeter2.TabIndex = 49;
            this.volumeMeter2.Text = "volumeMeter1";
            // 
            // volumeMeter1
            // 
            this.volumeMeter1.Amplitude = 0F;
            this.volumeMeter1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.volumeMeter1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.volumeMeter1.Location = new System.Drawing.Point(397, 16);
            this.volumeMeter1.Margin = new System.Windows.Forms.Padding(4);
            this.volumeMeter1.MaxDb = 3F;
            this.volumeMeter1.MinDb = -60F;
            this.volumeMeter1.Name = "volumeMeter1";
            this.volumeMeter1.Size = new System.Drawing.Size(19, 126);
            this.volumeMeter1.TabIndex = 50;
            this.volumeMeter1.Text = "volumeMeter1";
            // 
            // trackBarPosition
            // 
            this.trackBarPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarPosition.LargeChange = 10;
            this.trackBarPosition.Location = new System.Drawing.Point(47, 445);
            this.trackBarPosition.Margin = new System.Windows.Forms.Padding(4);
            this.trackBarPosition.Maximum = 100;
            this.trackBarPosition.Name = "trackBarPosition";
            this.trackBarPosition.Size = new System.Drawing.Size(771, 56);
            this.trackBarPosition.TabIndex = 46;
            this.trackBarPosition.TickFrequency = 5;
            this.trackBarPosition.Scroll += new System.EventHandler(this.TrackBarPosition_Scroll);
            // 
            // sldVolume
            // 
            this.sldVolume.ControlColor = System.Drawing.Color.Orange;
            this.sldVolume.DecPlaces = 1;
            this.sldVolume.Label = "vol";
            this.sldVolume.Location = new System.Drawing.Point(219, 16);
            this.sldVolume.Margin = new System.Windows.Forms.Padding(4);
            this.sldVolume.Maximum = 1D;
            this.sldVolume.Minimum = 0D;
            this.sldVolume.Name = "sldVolume";
            this.sldVolume.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldVolume.ResetValue = 0D;
            this.sldVolume.Size = new System.Drawing.Size(88, 42);
            this.sldVolume.TabIndex = 42;
            this.sldVolume.Value = 1D;
            this.sldVolume.ValueChanged += new System.EventHandler(this.Volume_ValueChanged);
            // 
            // potSpeed
            // 
            this.potSpeed.ControlColor = System.Drawing.Color.Black;
            this.potSpeed.DecPlaces = 0;
            this.potSpeed.Label = "";
            this.potSpeed.Location = new System.Drawing.Point(164, 16);
            this.potSpeed.Margin = new System.Windows.Forms.Padding(5);
            this.potSpeed.Maximum = 200D;
            this.potSpeed.Minimum = 30D;
            this.potSpeed.Name = "potSpeed";
            this.potSpeed.Size = new System.Drawing.Size(43, 39);
            this.potSpeed.TabIndex = 40;
            this.potSpeed.Taper = NBagOfTricks.UI.Taper.Linear;
            this.potSpeed.Value = 100D;
            // 
            // btnRewind
            // 
            this.btnRewind.FlatAppearance.BorderSize = 0;
            this.btnRewind.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRewind.Image = global::ClipExplorer.Properties.Resources.glyphicons_173_rewind;
            this.btnRewind.Location = new System.Drawing.Point(21, 16);
            this.btnRewind.Margin = new System.Windows.Forms.Padding(4);
            this.btnRewind.Name = "btnRewind";
            this.btnRewind.Size = new System.Drawing.Size(45, 39);
            this.btnRewind.TabIndex = 39;
            this.btnRewind.UseVisualStyleBackColor = false;
            this.btnRewind.Click += new System.EventHandler(this.Rewind_Click);
            // 
            // chkPlay
            // 
            this.chkPlay.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPlay.FlatAppearance.BorderSize = 0;
            this.chkPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkPlay.Image = global::ClipExplorer.Properties.Resources.glyphicons_174_play;
            this.chkPlay.Location = new System.Drawing.Point(73, 16);
            this.chkPlay.Margin = new System.Windows.Forms.Padding(4);
            this.chkPlay.Name = "chkPlay";
            this.chkPlay.Size = new System.Drawing.Size(43, 39);
            this.chkPlay.TabIndex = 41;
            this.chkPlay.UseVisualStyleBackColor = false;
            this.chkPlay.CheckedChanged += new System.EventHandler(this.Play_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 635);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Sound Explorer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPosition)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton fileDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private NBagOfTricks.UI.Slider sldVolume;
        private NBagOfTricks.UI.Pot potSpeed;
        private System.Windows.Forms.Button btnRewind;
        private System.Windows.Forms.CheckBox chkPlay;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel labelCurrentTime;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripLabel labelTotalTime;
        private NAudio.Gui.WaveformPainter waveformPainter2;
        private NAudio.Gui.WaveformPainter waveformPainter1;
        private NAudio.Gui.VolumeMeter volumeMeter2;
        private NAudio.Gui.VolumeMeter volumeMeter1;
        private System.Windows.Forms.TrackBar trackBarPosition;
        private Navigator navigator;
        private System.Windows.Forms.CheckBox chkLoop;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

