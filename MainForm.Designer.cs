namespace ClipExplorer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ftree = new NBagOfTricks.UI.FilTree();
            this.volR = new NBagOfTricks.UI.Meter();
            this.volL = new NBagOfTricks.UI.Meter();
            this.lblTime = new System.Windows.Forms.Label();
            this.rtbInfo = new System.Windows.Forms.RichTextBox();
            this.chkLoop = new System.Windows.Forms.CheckBox();
            this.waveformPainter2 = new NAudio.Gui.WaveformPainter();
            this.waveformPainter1 = new NAudio.Gui.WaveformPainter();
            this.trackBarPosition = new System.Windows.Forms.TrackBar();
            this.sldVolume = new NBagOfTricks.UI.Slider();
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
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ftree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.volR);
            this.splitContainer1.Panel2.Controls.Add(this.volL);
            this.splitContainer1.Panel2.Controls.Add(this.lblTime);
            this.splitContainer1.Panel2.Controls.Add(this.rtbInfo);
            this.splitContainer1.Panel2.Controls.Add(this.chkLoop);
            this.splitContainer1.Panel2.Controls.Add(this.waveformPainter2);
            this.splitContainer1.Panel2.Controls.Add(this.waveformPainter1);
            this.splitContainer1.Panel2.Controls.Add(this.trackBarPosition);
            this.splitContainer1.Panel2.Controls.Add(this.sldVolume);
            this.splitContainer1.Panel2.Controls.Add(this.btnRewind);
            this.splitContainer1.Panel2.Controls.Add(this.chkPlay);
            this.splitContainer1.Size = new System.Drawing.Size(1284, 608);
            this.splitContainer1.SplitterDistance = 625;
            this.splitContainer1.TabIndex = 1;
            // 
            // ftree
            // 
            this.ftree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ftree.DoubleClickSelect = false;
            this.ftree.Location = new System.Drawing.Point(0, 0);
            this.ftree.Name = "ftree";
            this.ftree.Size = new System.Drawing.Size(625, 608);
            this.ftree.TabIndex = 0;
            this.ftree.FileSelectedEvent += new System.EventHandler<string>(this.Navigator_FileSelectedEvent);
            // 
            // volR
            // 
            this.volR.ControlColor = System.Drawing.Color.Orange;
            this.volR.Label = "R";
            this.volR.Location = new System.Drawing.Point(312, 16);
            this.volR.Maximum = 3D;
            this.volR.MeterType = NBagOfTricks.UI.MeterType.Log;
            this.volR.Minimum = -60D;
            this.volR.Name = "volR";
            this.volR.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.volR.Size = new System.Drawing.Size(32, 150);
            this.volR.TabIndex = 59;
            // 
            // volL
            // 
            this.volL.ControlColor = System.Drawing.Color.Orange;
            this.volL.Label = "L";
            this.volL.Location = new System.Drawing.Point(274, 16);
            this.volL.Maximum = 3D;
            this.volL.MeterType = NBagOfTricks.UI.MeterType.Log;
            this.volL.Minimum = -60D;
            this.volL.Name = "volL";
            this.volL.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.volL.Size = new System.Drawing.Size(32, 150);
            this.volL.TabIndex = 58;
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(19, 159);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(46, 17);
            this.lblTime.TabIndex = 57;
            this.lblTime.Text = "label1";
            // 
            // rtbInfo
            // 
            this.rtbInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbInfo.Location = new System.Drawing.Point(31, 452);
            this.rtbInfo.Name = "rtbInfo";
            this.rtbInfo.Size = new System.Drawing.Size(575, 144);
            this.rtbInfo.TabIndex = 56;
            this.rtbInfo.Text = "";
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
            this.waveformPainter2.Location = new System.Drawing.Point(21, 303);
            this.waveformPainter2.Margin = new System.Windows.Forms.Padding(4);
            this.waveformPainter2.Name = "waveformPainter2";
            this.waveformPainter2.Size = new System.Drawing.Size(602, 74);
            this.waveformPainter2.TabIndex = 51;
            this.waveformPainter2.Text = "waveformPainter1";
            // 
            // waveformPainter1
            // 
            this.waveformPainter1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveformPainter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.waveformPainter1.ForeColor = System.Drawing.Color.SaddleBrown;
            this.waveformPainter1.Location = new System.Drawing.Point(21, 225);
            this.waveformPainter1.Margin = new System.Windows.Forms.Padding(4);
            this.waveformPainter1.Name = "waveformPainter1";
            this.waveformPainter1.Size = new System.Drawing.Size(602, 74);
            this.waveformPainter1.TabIndex = 52;
            this.waveformPainter1.Text = "waveformPainter1";
            // 
            // trackBarPosition
            // 
            this.trackBarPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarPosition.LargeChange = 10;
            this.trackBarPosition.Location = new System.Drawing.Point(21, 397);
            this.trackBarPosition.Margin = new System.Windows.Forms.Padding(4);
            this.trackBarPosition.Maximum = 100;
            this.trackBarPosition.Name = "trackBarPosition";
            this.trackBarPosition.Size = new System.Drawing.Size(602, 56);
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
            this.sldVolume.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.sldVolume.ResetValue = 0D;
            this.sldVolume.Size = new System.Drawing.Size(33, 109);
            this.sldVolume.TabIndex = 42;
            this.sldVolume.Value = 1D;
            this.sldVolume.ValueChanged += new System.EventHandler(this.Volume_ValueChanged);
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
        private System.Windows.Forms.Button btnRewind;
        private System.Windows.Forms.CheckBox chkPlay;
        private NAudio.Gui.WaveformPainter waveformPainter2;
        private NAudio.Gui.WaveformPainter waveformPainter1;
        private System.Windows.Forms.TrackBar trackBarPosition;
        private NBagOfTricks.UI.FilTree ftree;
        private System.Windows.Forms.CheckBox chkLoop;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.RichTextBox rtbInfo;
        private System.Windows.Forms.Label lblTime;
        private NBagOfTricks.UI.Meter volL;
        private NBagOfTricks.UI.Meter volR;
    }
}

