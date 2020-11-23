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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "111"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.Red, null);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "222"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.CornflowerBlue, null);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "333"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.SpringGreen, null);
            this.sldTempo = new NBagOfTricks.UI.Slider();
            this.label1 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.chdrName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sldBar = new NBagOfTricks.UI.Slider();
            this.sldBeat = new NBagOfTricks.UI.Slider();
            this.sldSubBeat = new NBagOfTricks.UI.Slider();
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(110, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(214, 17);
            this.label1.TabIndex = 64;
            this.label1.Text = "Mute/Solo/Name for 16 channels";
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chdrName,
            this.columnHeader1});
            this.listView1.HideSelection = false;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.listView1.Location = new System.Drawing.Point(104, 37);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(168, 183);
            this.listView1.TabIndex = 65;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // chdrName
            // 
            this.chdrName.Text = "Name";
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 125;
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
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.sldSubBeat);
            this.Controls.Add(this.sldBeat);
            this.Controls.Add(this.sldBar);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sldTempo);
            this.Name = "MidiPlayer";
            this.Size = new System.Drawing.Size(523, 264);
            this.Load += new System.EventHandler(this.MidiPlayer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private NBagOfTricks.UI.Slider sldTempo;
        private Label label1;
        private ListView listView1;
        private ColumnHeader chdrName;
        private ColumnHeader columnHeader1;
        private NBagOfTricks.UI.Slider sldBar;
        private NBagOfTricks.UI.Slider sldBeat;
        private NBagOfTricks.UI.Slider sldSubBeat;
    }
}
