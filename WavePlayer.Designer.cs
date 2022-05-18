using AudioLib;

namespace ClipExplorer
{
    partial class WavePlayer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WavePlayer));
            this.waveViewerL = new AudioLib.WaveViewer();
            this.levelR = new AudioLib.Meter();
            this.levelL = new AudioLib.Meter();
            this.waveViewerR = new AudioLib.WaveViewer();
            this.timeBar = new AudioLib.TimeBar();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // waveViewerL
            // 
            this.waveViewerL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewerL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewerL.DrawColor = System.Drawing.Color.Black;
            this.waveViewerL.Location = new System.Drawing.Point(0, 240);
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
            // levelR
            // 
            this.levelR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.levelR.DrawColor = System.Drawing.Color.White;
            this.levelR.Label = "R";
            this.levelR.Location = new System.Drawing.Point(119, 392);
            this.levelR.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.levelR.Maximum = 3D;
            this.levelR.MeterType = AudioLib.MeterType.Log;
            this.levelR.Minimum = -60D;
            this.levelR.Name = "levelR";
            this.levelR.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.levelR.Size = new System.Drawing.Size(100, 50);
            this.levelR.TabIndex = 65;
            this.toolTip.SetToolTip(this.levelR, "Right level");
            // 
            // levelL
            // 
            this.levelL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.levelL.DrawColor = System.Drawing.Color.White;
            this.levelL.Label = "L";
            this.levelL.Location = new System.Drawing.Point(0, 392);
            this.levelL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.levelL.Maximum = 3D;
            this.levelL.MeterType = AudioLib.MeterType.Log;
            this.levelL.Minimum = -60D;
            this.levelL.Name = "levelL";
            this.levelL.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.levelL.Size = new System.Drawing.Size(100, 50);
            this.levelL.TabIndex = 64;
            this.toolTip.SetToolTip(this.levelL, "Left level");
            // 
            // waveViewerR
            // 
            this.waveViewerR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewerR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewerR.DrawColor = System.Drawing.Color.Black;
            this.waveViewerR.Location = new System.Drawing.Point(0, 310);
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
            this.timeBar.Location = new System.Drawing.Point(0, 171);
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
            this.toolStripButton1,
            this.toolStripSeparator1,
            this.toolStripButton2,
            this.toolStripSeparator2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(589, 27);
            this.toolStrip1.TabIndex = 70;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // WavePlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.timeBar);
            this.Controls.Add(this.waveViewerR);
            this.Controls.Add(this.waveViewerL);
            this.Controls.Add(this.levelR);
            this.Controls.Add(this.levelL);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "WavePlayer";
            this.Size = new System.Drawing.Size(589, 454);
            this.Load += new System.EventHandler(this.WavePlayer_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WaveViewer waveViewerL;
        private WaveViewer waveViewerR;
        private Meter levelR;
        private Meter levelL;
        private TimeBar timeBar;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        //private NAudio.Gui.WaveformPainter waveformPainterR;
        //private NAudio.Gui.WaveformPainter waveformPainterL;
    }
}
