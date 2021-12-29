using NBagOfUis;

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
            this.waveViewerL = new NBagOfUis.WaveViewer();
            this.levelR = new NBagOfUis.Meter();
            this.levelL = new NBagOfUis.Meter();
            this.waveViewerR = new NBagOfUis.WaveViewer();
            this.timeBar = new NBagOfUis.TimeBar();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // waveViewerL
            // 
            this.waveViewerL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewerL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewerL.DrawColor = System.Drawing.Color.Black;
            this.waveViewerL.Location = new System.Drawing.Point(0, 55);
            this.waveViewerL.Marker1 = -1;
            this.waveViewerL.Marker2 = -1;
            this.waveViewerL.MarkerColor = System.Drawing.Color.Black;
            this.waveViewerL.Mode = NBagOfUis.WaveViewer.DrawMode.Envelope;
            this.waveViewerL.Name = "waveViewerL";
            this.waveViewerL.Size = new System.Drawing.Size(589, 50);
            this.waveViewerL.TabIndex = 67;
            this.toolTip.SetToolTip(this.waveViewerL, "Left waveform");
            // 
            // levelR
            // 
            this.levelR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.levelR.DrawColor = System.Drawing.Color.White;
            this.levelR.Label = "R";
            this.levelR.Location = new System.Drawing.Point(119, 177);
            this.levelR.Maximum = 3D;
            this.levelR.MeterType = NBagOfUis.MeterType.Log;
            this.levelR.Minimum = -60D;
            this.levelR.Name = "levelR";
            this.levelR.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.levelR.Size = new System.Drawing.Size(100, 40);
            this.levelR.TabIndex = 65;
            this.toolTip.SetToolTip(this.levelR, "Right level");
            // 
            // levelL
            // 
            this.levelL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.levelL.DrawColor = System.Drawing.Color.White;
            this.levelL.Label = "L";
            this.levelL.Location = new System.Drawing.Point(0, 177);
            this.levelL.Maximum = 3D;
            this.levelL.MeterType = NBagOfUis.MeterType.Log;
            this.levelL.Minimum = -60D;
            this.levelL.Name = "levelL";
            this.levelL.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.levelL.Size = new System.Drawing.Size(100, 40);
            this.levelL.TabIndex = 64;
            this.toolTip.SetToolTip(this.levelL, "Left level");
            // 
            // waveViewerR
            // 
            this.waveViewerR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewerR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewerR.DrawColor = System.Drawing.Color.Black;
            this.waveViewerR.Location = new System.Drawing.Point(0, 111);
            this.waveViewerR.Marker1 = -1;
            this.waveViewerR.Marker2 = -1;
            this.waveViewerR.MarkerColor = System.Drawing.Color.Black;
            this.waveViewerR.Mode = NBagOfUis.WaveViewer.DrawMode.Envelope;
            this.waveViewerR.Name = "waveViewerR";
            this.waveViewerR.Size = new System.Drawing.Size(589, 50);
            this.waveViewerR.TabIndex = 68;
            this.toolTip.SetToolTip(this.waveViewerR, "Right waveform");
            // 
            // timeBar
            // 
            this.timeBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timeBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.timeBar.FontLarge = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timeBar.FontSmall = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timeBar.ForeColor = System.Drawing.Color.Silver;
            this.timeBar.Location = new System.Drawing.Point(0, 0);
            this.timeBar.MarkerColor = System.Drawing.Color.Black;
            this.timeBar.Name = "timeBar";
            this.timeBar.ProgressColor = System.Drawing.Color.White;
            this.timeBar.Size = new System.Drawing.Size(589, 50);
            this.timeBar.SnapMsec = 0;
            this.timeBar.TabIndex = 69;
            this.toolTip.SetToolTip(this.timeBar, "Time in min:sec:msec");
            this.timeBar.CurrentTimeChanged += new System.EventHandler(this.TimeBar_CurrentTimeChanged);
            // 
            // WavePlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.timeBar);
            this.Controls.Add(this.waveViewerR);
            this.Controls.Add(this.waveViewerL);
            this.Controls.Add(this.levelR);
            this.Controls.Add(this.levelL);
            this.Name = "WavePlayer";
            this.Size = new System.Drawing.Size(589, 222);
            this.Load += new System.EventHandler(this.WavePlayer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private WaveViewer waveViewerL;
        private WaveViewer waveViewerR;
        private Meter levelR;
        private Meter levelL;
        private TimeBar timeBar;
        private System.Windows.Forms.ToolTip toolTip;
        //private NAudio.Gui.WaveformPainter waveformPainterR;
        //private NAudio.Gui.WaveformPainter waveformPainterL;
    }
}
