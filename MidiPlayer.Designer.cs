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
            this.barBar = new NBagOfTricks.UI.BarBar();
            this.clickGrid = new NBagOfTricks.UI.ClickGrid();
            this.sldTempo = new NBagOfTricks.UI.Slider();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDrumChannel = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // barBar
            // 
            this.barBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.barBar.BeatsPerBar = 4;
            this.barBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.barBar.FontLarge = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.barBar.FontSmall = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.barBar.Location = new System.Drawing.Point(0, 0);
            this.barBar.MarkerColor = System.Drawing.Color.Black;
            this.barBar.Name = "barBar";
            this.barBar.ProgressColor = System.Drawing.Color.White;
            this.barBar.Size = new System.Drawing.Size(498, 50);
            this.barBar.Snap = NBagOfTricks.UI.BarBar.SnapType.Bar;
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
            this.sldTempo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 17);
            this.label1.TabIndex = 71;
            this.label1.Text = "Alt Drum";
            // 
            // txtDrumChannel
            // 
            this.txtDrumChannel.BackColor = System.Drawing.SystemColors.Control;
            this.txtDrumChannel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDrumChannel.Location = new System.Drawing.Point(76, 105);
            this.txtDrumChannel.Name = "txtDrumChannel";
            this.txtDrumChannel.Size = new System.Drawing.Size(25, 22);
            this.txtDrumChannel.TabIndex = 72;
            this.txtDrumChannel.Text = "10";
            this.txtDrumChannel.TextChanged += new System.EventHandler(this.DrumChannel_TextChanged);
            // 
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtDrumChannel);
            this.Controls.Add(this.label1);
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
        private Label label1;
        private TextBox txtDrumChannel;
    }
}
