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
            this.waveViewer = new NBagOfTricks.UI.WaveViewer();
            this.levelR = new NBagOfTricks.UI.Meter();
            this.levelL = new NBagOfTricks.UI.Meter();
            this.waveformPainterR = new NAudio.Gui.WaveformPainter();
            this.waveformPainterL = new NAudio.Gui.WaveformPainter();
            this.SuspendLayout();
            // 
            // waveViewer
            // 
            this.waveViewer.DrawColor = System.Drawing.Color.Green;
            this.waveViewer.Location = new System.Drawing.Point(6, 49);
            this.waveViewer.Name = "waveViewer";
            this.waveViewer.Size = new System.Drawing.Size(601, 47);
            this.waveViewer.TabIndex = 67;
            // 
            // levelR
            // 
            this.levelR.DrawColor = System.Drawing.Color.Fuchsia;
            this.levelR.Label = "R";
            this.levelR.Location = new System.Drawing.Point(106, 9);
            this.levelR.Maximum = 3D;
            this.levelR.MeterType = NBagOfTricks.UI.MeterType.Log;
            this.levelR.Minimum = -60D;
            this.levelR.Name = "levelR";
            this.levelR.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.levelR.Size = new System.Drawing.Size(94, 28);
            this.levelR.TabIndex = 65;
            // 
            // levelL
            // 
            this.levelL.DrawColor = System.Drawing.Color.Fuchsia;
            this.levelL.Label = "L";
            this.levelL.Location = new System.Drawing.Point(6, 9);
            this.levelL.Maximum = 3D;
            this.levelL.MeterType = NBagOfTricks.UI.MeterType.Log;
            this.levelL.Minimum = -60D;
            this.levelL.Name = "levelL";
            this.levelL.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.levelL.Size = new System.Drawing.Size(94, 28);
            this.levelL.TabIndex = 64;
            // 
            // waveformPainterR
            // 
            this.waveformPainterR.BackColor = System.Drawing.Color.LightSkyBlue;
            this.waveformPainterR.ForeColor = System.Drawing.Color.SaddleBrown;
            this.waveformPainterR.Location = new System.Drawing.Point(6, 171);
            this.waveformPainterR.Margin = new System.Windows.Forms.Padding(4);
            this.waveformPainterR.Name = "waveformPainterR";
            this.waveformPainterR.Size = new System.Drawing.Size(602, 48);
            this.waveformPainterR.TabIndex = 62;
            this.waveformPainterR.Text = "waveformPainterL";
            // 
            // waveformPainterL
            // 
            this.waveformPainterL.BackColor = System.Drawing.Color.LightSkyBlue;
            this.waveformPainterL.ForeColor = System.Drawing.Color.SaddleBrown;
            this.waveformPainterL.Location = new System.Drawing.Point(6, 111);
            this.waveformPainterL.Margin = new System.Windows.Forms.Padding(4);
            this.waveformPainterL.Name = "waveformPainterL";
            this.waveformPainterL.Size = new System.Drawing.Size(602, 52);
            this.waveformPainterL.TabIndex = 63;
            this.waveformPainterL.Text = "waveformPainterL";
            // 
            // WavePlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.waveViewer);
            this.Controls.Add(this.levelR);
            this.Controls.Add(this.levelL);
            this.Controls.Add(this.waveformPainterR);
            this.Controls.Add(this.waveformPainterL);
            this.Name = "WavePlayer";
            this.Size = new System.Drawing.Size(619, 231);
            this.ResumeLayout(false);

        }

        #endregion

        private WaveViewer waveViewer;
        private Meter levelR;
        private Meter levelL;
        private NAudio.Gui.WaveformPainter waveformPainterR;
        private NAudio.Gui.WaveformPainter waveformPainterL;
    }
}
