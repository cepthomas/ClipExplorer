using AudioLib;

namespace ClipExplorer
{
    partial class AudioExplorer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioExplorer));
            this.waveViewerL = new AudioLib.WaveViewer();
            this.waveViewerR = new AudioLib.WaveViewer();
            this.timeBar = new AudioLib.TimeBar();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.fileDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // waveViewerL
            // 
            this.waveViewerL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewerL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewerL.DrawColor = System.Drawing.Color.Black;
            this.waveViewerL.Location = new System.Drawing.Point(0, 110);
            this.waveViewerL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewerL.Marker1 = -1;
            this.waveViewerL.Marker2 = -1;
            this.waveViewerL.MarkerColor = System.Drawing.Color.Black;
            this.waveViewerL.Mode = AudioLib.WaveViewer.DrawMode.Envelope;
            this.waveViewerL.Name = "waveViewerL";
            this.waveViewerL.Size = new System.Drawing.Size(586, 62);
            this.waveViewerL.TabIndex = 67;
            this.toolTip.SetToolTip(this.waveViewerL, "Left waveform");
            // 
            // waveViewerR
            // 
            this.waveViewerR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewerR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewerR.DrawColor = System.Drawing.Color.Black;
            this.waveViewerR.Location = new System.Drawing.Point(0, 180);
            this.waveViewerR.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewerR.Marker1 = -1;
            this.waveViewerR.Marker2 = -1;
            this.waveViewerR.MarkerColor = System.Drawing.Color.Black;
            this.waveViewerR.Mode = AudioLib.WaveViewer.DrawMode.Envelope;
            this.waveViewerR.Name = "waveViewerR";
            this.waveViewerR.Size = new System.Drawing.Size(586, 62);
            this.waveViewerR.TabIndex = 68;
            this.toolTip.SetToolTip(this.waveViewerR, "Right waveform");
            // 
            // timeBar
            // 
            this.timeBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timeBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.timeBar.FontLarge = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.timeBar.FontSmall = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.timeBar.ForeColor = System.Drawing.Color.Silver;
            this.timeBar.Location = new System.Drawing.Point(0, 41);
            this.timeBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.timeBar.MarkerColor = System.Drawing.Color.Black;
            this.timeBar.Name = "timeBar";
            this.timeBar.ProgressColor = System.Drawing.Color.White;
            this.timeBar.Size = new System.Drawing.Size(586, 62);
            this.timeBar.SnapMsec = 0;
            this.timeBar.TabIndex = 69;
            this.toolTip.SetToolTip(this.timeBar, "Time in min:sec:msec");
            this.timeBar.CurrentTimeChanged += new System.EventHandler(this.TimeBar_CurrentTimeChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileDropDown,
            this.toolStripSeparator1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(589, 27);
            this.toolStrip1.TabIndex = 70;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // fileDropDown
            // 
            this.fileDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.textToolStripMenuItem});
            this.fileDropDown.Image = ((System.Drawing.Image)(resources.GetObject("fileDropDown.Image")));
            this.fileDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileDropDown.Name = "fileDropDown";
            this.fileDropDown.Size = new System.Drawing.Size(66, 24);
            this.fileDropDown.Text = "Export";
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.textToolStripMenuItem.Text = "Text";
            this.textToolStripMenuItem.Click += new System.EventHandler(this.Export_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // AudioExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.timeBar);
            this.Controls.Add(this.waveViewerR);
            this.Controls.Add(this.waveViewerL);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "AudioExplorer";
            this.Size = new System.Drawing.Size(589, 248);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WaveViewer waveViewerL;
        private WaveViewer waveViewerR;
        private TimeBar timeBar;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton fileDropDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
    }
}
