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
            this.chkMapDrums = new System.Windows.Forms.CheckBox();
            this.barBar = new NBagOfTricks.UI.BarBar();
            this.clickGrid = new NBagOfTricks.UI.ClickGrid();
            this.sldTempo = new NBagOfTricks.UI.Slider();
            this.SuspendLayout();
            // 
            // chkMapDrums
            // 
            this.chkMapDrums.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkMapDrums.AutoSize = true;
            this.chkMapDrums.FlatAppearance.BorderSize = 0;
            this.chkMapDrums.Location = new System.Drawing.Point(0, 112);
            this.chkMapDrums.Name = "chkMapDrums";
            this.chkMapDrums.Size = new System.Drawing.Size(57, 27);
            this.chkMapDrums.TabIndex = 71;
            this.chkMapDrums.Text = "DMAP";
            this.chkMapDrums.UseVisualStyleBackColor = true;
            this.chkMapDrums.CheckedChanged += new System.EventHandler(this.MapDrums_CheckedChanged);
            // 
            // barBar
            // 
            this.barBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.barBar.BeatsPerBar = 4;
            this.barBar.Current = 0;
            this.barBar.Length = 0;
            this.barBar.Location = new System.Drawing.Point(0, 0);
            this.barBar.Name = "barBar";
            this.barBar.ProgressColor = System.Drawing.Color.White;
            this.barBar.Size = new System.Drawing.Size(498, 50);
            this.barBar.Snap = NBagOfTricks.UI.BarBar.SnapType.Tick;
            this.barBar.TabIndex = 70;
            this.barBar.TicksPerBeat = 8;
            // 
            // clickGrid
            // 
            this.clickGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clickGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.clickGrid.Location = new System.Drawing.Point(108, 56);
            this.clickGrid.Name = "clickGrid";
            this.clickGrid.Size = new System.Drawing.Size(390, 178);
            this.clickGrid.TabIndex = 69;
            this.clickGrid.IndicatorEvent += new System.EventHandler<NBagOfTricks.UI.IndicatorEventArgs>(this.ClickGrid_IndicatorEvent);
            // 
            // sldTempo
            // 
            this.sldTempo.DecPlaces = 0;
            this.sldTempo.DrawColor = System.Drawing.Color.PaleGreen;
            this.sldTempo.Label = "BPM";
            this.sldTempo.Location = new System.Drawing.Point(0, 56);
            this.sldTempo.Maximum = 250D;
            this.sldTempo.Minimum = 50D;
            this.sldTempo.Name = "sldTempo";
            this.sldTempo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldTempo.ResetValue = 50D;
            this.sldTempo.Size = new System.Drawing.Size(102, 43);
            this.sldTempo.TabIndex = 63;
            this.sldTempo.Value = 100D;
            this.sldTempo.ValueChanged += new System.EventHandler(this.Tempo_ValueChanged);
            // 
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkMapDrums);
            this.Controls.Add(this.barBar);
            this.Controls.Add(this.clickGrid);
            this.Controls.Add(this.sldTempo);
            this.Name = "MidiPlayer";
            this.Size = new System.Drawing.Size(498, 249);
            this.Load += new System.EventHandler(this.MidiPlayer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private NBagOfTricks.UI.Slider sldTempo;
        private NBagOfTricks.UI.ClickGrid clickGrid;
        private NBagOfTricks.UI.BarBar barBar;
        private CheckBox chkMapDrums;
    }
}
