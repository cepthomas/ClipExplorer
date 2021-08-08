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
            this.txtPatchChannel = new System.Windows.Forms.TextBox();
            this.cmbPatchList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
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
            this.label1.Size = new System.Drawing.Size(79, 17);
            this.label1.TabIndex = 71;
            this.label1.Text = "Drum Chan";
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
            // txtPatchChannel
            // 
            this.txtPatchChannel.BackColor = System.Drawing.SystemColors.Control;
            this.txtPatchChannel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPatchChannel.Location = new System.Drawing.Point(67, 136);
            this.txtPatchChannel.Name = "txtPatchChannel";
            this.txtPatchChannel.Size = new System.Drawing.Size(34, 22);
            this.txtPatchChannel.TabIndex = 73;
            this.txtPatchChannel.TextChanged += new System.EventHandler(this.PatchChannel_TextChanged);
            // 
            // cmbPatchList
            // 
            this.cmbPatchList.BackColor = System.Drawing.SystemColors.Control;
            this.cmbPatchList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPatchList.DropDownWidth = 150;
            this.cmbPatchList.FormattingEnabled = true;
            this.cmbPatchList.Location = new System.Drawing.Point(3, 166);
            this.cmbPatchList.Name = "cmbPatchList";
            this.cmbPatchList.Size = new System.Drawing.Size(98, 24);
            this.cmbPatchList.TabIndex = 74;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 17);
            this.label2.TabIndex = 75;
            this.label2.Text = "Repatch";
            // 
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbPatchList);
            this.Controls.Add(this.txtPatchChannel);
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
        private TextBox txtPatchChannel;
        private ComboBox cmbPatchList;
        private Label label2;
    }
}
