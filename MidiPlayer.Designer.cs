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
            this.sldBar = new NBagOfTricks.UI.Slider();
            this.sldBeat = new NBagOfTricks.UI.Slider();
            this.sldSubBeat = new NBagOfTricks.UI.Slider();
            this.clickGrid = new NBagOfTricks.UI.ClickGrid();
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
            this.sldTempo.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.sldTempo.ResetValue = 50D;
            this.sldTempo.Size = new System.Drawing.Size(45, 150);
            this.sldTempo.TabIndex = 63;
            this.sldTempo.Value = 100D;
            this.sldTempo.ValueChanged += new System.EventHandler(this.Tempo_ValueChanged);
            // 
            // sldBar
            // 
            this.sldBar.DecPlaces = 0;
            this.sldBar.DrawColor = System.Drawing.Color.LemonChiffon;
            this.sldBar.Label = "";
            this.sldBar.Location = new System.Drawing.Point(292, 64);
            this.sldBar.Maximum = 100D;
            this.sldBar.Minimum = 0D;
            this.sldBar.Name = "sldBar";
            this.sldBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.sldBar.ResetValue = 0D;
            this.sldBar.Size = new System.Drawing.Size(68, 114);
            this.sldBar.TabIndex = 66;
            this.sldBar.Value = 0D;
            // 
            // sldBeat
            // 
            this.sldBeat.DecPlaces = 0;
            this.sldBeat.DrawColor = System.Drawing.Color.LemonChiffon;
            this.sldBeat.Label = "";
            this.sldBeat.Location = new System.Drawing.Point(366, 64);
            this.sldBeat.Maximum = 4D;
            this.sldBeat.Minimum = 1D;
            this.sldBeat.Name = "sldBeat";
            this.sldBeat.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.sldBeat.ResetValue = 1D;
            this.sldBeat.Size = new System.Drawing.Size(68, 114);
            this.sldBeat.TabIndex = 67;
            this.sldBeat.Value = 2D;
            // 
            // sldSubBeat
            // 
            this.sldSubBeat.DecPlaces = 0;
            this.sldSubBeat.DrawColor = System.Drawing.Color.LemonChiffon;
            this.sldSubBeat.Label = "";
            this.sldSubBeat.Location = new System.Drawing.Point(440, 64);
            this.sldSubBeat.Maximum = 96D;
            this.sldSubBeat.Minimum = 1D;
            this.sldSubBeat.Name = "sldSubBeat";
            this.sldSubBeat.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.sldSubBeat.ResetValue = 1D;
            this.sldSubBeat.Size = new System.Drawing.Size(68, 114);
            this.sldSubBeat.TabIndex = 68;
            this.sldSubBeat.Value = 48D;
            // 
            // clickGrid
            // 
            this.clickGrid.Location = new System.Drawing.Point(17, 214);
            this.clickGrid.Name = "clickGrid";
            this.clickGrid.Size = new System.Drawing.Size(343, 179);
            this.clickGrid.TabIndex = 69;
            this.clickGrid.IndicatorEvent += new System.EventHandler<NBagOfTricks.UI.IndicatorEventArgs>(this.ClickGrid_IndicatorEvent);
            // 
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.clickGrid);
            this.Controls.Add(this.sldSubBeat);
            this.Controls.Add(this.sldBeat);
            this.Controls.Add(this.sldBar);
            this.Controls.Add(this.sldTempo);
            this.Name = "MidiPlayer";
            this.Size = new System.Drawing.Size(523, 434);
            this.Load += new System.EventHandler(this.MidiPlayer_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private NBagOfTricks.UI.Slider sldTempo;
        private NBagOfTricks.UI.Slider sldBar;
        private NBagOfTricks.UI.Slider sldBeat;
        private NBagOfTricks.UI.Slider sldSubBeat;
        private NBagOfTricks.UI.ClickGrid clickGrid;
    }
}
