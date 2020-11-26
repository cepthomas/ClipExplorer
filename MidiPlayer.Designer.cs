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
            this.sldTempo = new NBagOfTricks.UI.Slider();
            this.clickGrid = new NBagOfTricks.UI.ClickGrid();
            this.barBar = new NBagOfTricks.UI.BarBar();
            this.SuspendLayout();
            // 
            // sldTempo
            // 
            this.sldTempo.DecPlaces = 0;
            this.sldTempo.DrawColor = System.Drawing.Color.PaleGreen;
            this.sldTempo.Label = "BPM";
            this.sldTempo.Location = new System.Drawing.Point(17, 14);
            this.sldTempo.Maximum = 250D;
            this.sldTempo.Minimum = 50D;
            this.sldTempo.Name = "sldTempo";
            this.sldTempo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldTempo.ResetValue = 50D;
            this.sldTempo.Size = new System.Drawing.Size(164, 43);
            this.sldTempo.TabIndex = 63;
            this.sldTempo.Value = 100D;
            this.sldTempo.ValueChanged += new System.EventHandler(this.Tempo_ValueChanged);
            // 
            // clickGrid
            // 
            this.clickGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.clickGrid.Location = new System.Drawing.Point(17, 84);
            this.clickGrid.Name = "clickGrid";
            this.clickGrid.Size = new System.Drawing.Size(480, 214);
            this.clickGrid.TabIndex = 69;
            this.clickGrid.IndicatorEvent += new System.EventHandler<NBagOfTricks.UI.IndicatorEventArgs>(this.ClickGrid_IndicatorEvent);
            // 
            // barBar
            // 
            this.barBar.BeatsPerBar = 4;
            this.barBar.CurrentTick = 0;
            this.barBar.Length = 0;
            this.barBar.Location = new System.Drawing.Point(202, 14);
            this.barBar.Name = "barBar";
            this.barBar.ProgressColor = System.Drawing.Color.White;
            this.barBar.Size = new System.Drawing.Size(295, 43);
            this.barBar.Snap = NBagOfTricks.UI.BarBar.SnapType.None;
            this.barBar.TabIndex = 70;
            this.barBar.TicksPerBeat = 8;
            // 
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.barBar);
            this.Controls.Add(this.clickGrid);
            this.Controls.Add(this.sldTempo);
            this.Name = "MidiPlayer";
            this.Size = new System.Drawing.Size(523, 434);
            this.Load += new System.EventHandler(this.MidiPlayer_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private NBagOfTricks.UI.Slider sldTempo;
        private NBagOfTricks.UI.ClickGrid clickGrid;
        private NBagOfTricks.UI.BarBar barBar;
    }
}
