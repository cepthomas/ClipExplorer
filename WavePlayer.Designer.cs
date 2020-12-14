using NBagOfTricks.UI;

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
            this.waveViewerL = new NBagOfTricks.UI.WaveViewer();
            this.levelR = new NBagOfTricks.UI.Meter();
            this.levelL = new NBagOfTricks.UI.Meter();
            this.waveViewerR = new NBagOfTricks.UI.WaveViewer();
            this.SuspendLayout();
            // 
            // waveViewerL
            // 
            this.waveViewerL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewerL.DrawColor = System.Drawing.Color.Green;
            this.waveViewerL.Location = new System.Drawing.Point(0, 0);
            this.waveViewerL.Name = "waveViewerL";
            this.waveViewerL.Size = new System.Drawing.Size(525, 50);
            this.waveViewerL.TabIndex = 67;
            // 
            // levelR
            // 
            this.levelR.DrawColor = System.Drawing.Color.Fuchsia;
            this.levelR.Label = "R";
            this.levelR.Location = new System.Drawing.Point(119, 122);
            this.levelR.Maximum = 3D;
            this.levelR.MeterType = NBagOfTricks.UI.MeterType.Log;
            this.levelR.Minimum = -60D;
            this.levelR.Name = "levelR";
            this.levelR.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.levelR.Size = new System.Drawing.Size(100, 40);
            this.levelR.TabIndex = 65;
            // 
            // levelL
            // 
            this.levelL.DrawColor = System.Drawing.Color.Fuchsia;
            this.levelL.Label = "L";
            this.levelL.Location = new System.Drawing.Point(0, 122);
            this.levelL.Maximum = 3D;
            this.levelL.MeterType = NBagOfTricks.UI.MeterType.Log;
            this.levelL.Minimum = -60D;
            this.levelL.Name = "levelL";
            this.levelL.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.levelL.Size = new System.Drawing.Size(100, 40);
            this.levelL.TabIndex = 64;
            // 
            // waveViewerR
            // 
            this.waveViewerR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewerR.DrawColor = System.Drawing.Color.Green;
            this.waveViewerR.Location = new System.Drawing.Point(0, 56);
            this.waveViewerR.Name = "waveViewerR";
            this.waveViewerR.Size = new System.Drawing.Size(525, 50);
            this.waveViewerR.TabIndex = 68;
            // 
            // WavePlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.waveViewerR);
            this.Controls.Add(this.waveViewerL);
            this.Controls.Add(this.levelR);
            this.Controls.Add(this.levelL);
            this.Name = "WavePlayer";
            this.Size = new System.Drawing.Size(525, 164);
            this.Load += new System.EventHandler(this.WavePlayer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private WaveViewer waveViewerL;
        private WaveViewer waveViewerR;
        private Meter levelR;
        private Meter levelL;
        //private NAudio.Gui.WaveformPainter waveformPainterR;
        //private NAudio.Gui.WaveformPainter waveformPainterL;
    }
}
