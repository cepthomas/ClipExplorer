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
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(110, 31);
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
            this.listView1.Location = new System.Drawing.Point(132, 111);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(311, 183);
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
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sldTempo);
            this.Name = "MidiPlayer";
            this.Size = new System.Drawing.Size(523, 316);
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
    }
}
