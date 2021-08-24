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
            this.components = new System.ComponentModel.Container();
            this.barBar = new NBagOfTricks.UI.BarBar();
            this.cgChannels = new NBagOfTricks.UI.ClickGrid();
            this.sldTempo = new NBagOfTricks.UI.Slider();
            this.txtPatchChannel = new System.Windows.Forms.TextBox();
            this.cmbPatchList = new System.Windows.Forms.ComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.chkLogMidi = new System.Windows.Forms.CheckBox();
            this.chkDrumsOn1 = new System.Windows.Forms.CheckBox();
            this.btnPatch = new System.Windows.Forms.Button();
            this.lbPatterns = new System.Windows.Forms.ListBox();
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
            this.barBar.Location = new System.Drawing.Point(4, 0);
            this.barBar.MarkerColor = System.Drawing.Color.Black;
            this.barBar.Name = "barBar";
            this.barBar.ProgressColor = System.Drawing.Color.White;
            this.barBar.Size = new System.Drawing.Size(677, 50);
            this.barBar.Snap = NBagOfTricks.UI.BarBar.SnapType.Bar;
            this.barBar.SubdivsPerBeat = 8;
            this.barBar.TabIndex = 70;
            this.toolTip.SetToolTip(this.barBar, "Time in bar:beat:subdivision");
            // 
            // cgChannels
            // 
            this.cgChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cgChannels.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cgChannels.Location = new System.Drawing.Point(259, 56);
            this.cgChannels.Name = "cgChannels";
            this.cgChannels.Size = new System.Drawing.Size(422, 178);
            this.cgChannels.TabIndex = 69;
            this.toolTip.SetToolTip(this.cgChannels, "Midi channels with mute/solo");
            this.cgChannels.IndicatorEvent += new System.EventHandler<NBagOfTricks.UI.IndicatorEventArgs>(this.Channels_IndicatorEvent);
            // 
            // sldTempo
            // 
            this.sldTempo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sldTempo.DecPlaces = 0;
            this.sldTempo.DrawColor = System.Drawing.Color.PaleGreen;
            this.sldTempo.Label = "BPM";
            this.sldTempo.Location = new System.Drawing.Point(4, 56);
            this.sldTempo.Maximum = 200D;
            this.sldTempo.Minimum = 50D;
            this.sldTempo.Name = "sldTempo";
            this.sldTempo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldTempo.ResetValue = 50D;
            this.sldTempo.Size = new System.Drawing.Size(98, 43);
            this.sldTempo.TabIndex = 63;
            this.sldTempo.Value = 100D;
            this.sldTempo.ValueChanged += new System.EventHandler(this.Tempo_ValueChanged);
            // 
            // txtPatchChannel
            // 
            this.txtPatchChannel.BackColor = System.Drawing.SystemColors.Control;
            this.txtPatchChannel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPatchChannel.Location = new System.Drawing.Point(66, 136);
            this.txtPatchChannel.Name = "txtPatchChannel";
            this.txtPatchChannel.Size = new System.Drawing.Size(34, 22);
            this.txtPatchChannel.TabIndex = 73;
            this.toolTip.SetToolTip(this.txtPatchChannel, "Patch channel number");
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
            this.toolTip.SetToolTip(this.cmbPatchList, "Patch name");
            // 
            // chkLogMidi
            // 
            this.chkLogMidi.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkLogMidi.AutoSize = true;
            this.chkLogMidi.Location = new System.Drawing.Point(4, 212);
            this.chkLogMidi.Name = "chkLogMidi";
            this.chkLogMidi.Size = new System.Drawing.Size(71, 27);
            this.chkLogMidi.TabIndex = 76;
            this.chkLogMidi.Text = "Log Midi";
            this.toolTip.SetToolTip(this.chkLogMidi, "Enable logging midi events");
            this.chkLogMidi.UseVisualStyleBackColor = true;
            // 
            // chkDrumsOn1
            // 
            this.chkDrumsOn1.AutoSize = true;
            this.chkDrumsOn1.Location = new System.Drawing.Point(4, 109);
            this.chkDrumsOn1.Name = "chkDrumsOn1";
            this.chkDrumsOn1.Size = new System.Drawing.Size(77, 21);
            this.chkDrumsOn1.TabIndex = 77;
            this.chkDrumsOn1.Text = "Dr on 1";
            this.toolTip.SetToolTip(this.chkDrumsOn1, "Drums are on channel 1");
            this.chkDrumsOn1.UseVisualStyleBackColor = true;
            this.chkDrumsOn1.CheckedChanged += new System.EventHandler(this.DrumsOn1_CheckedChanged);
            // 
            // btnPatch
            // 
            this.btnPatch.Location = new System.Drawing.Point(3, 136);
            this.btnPatch.Name = "btnPatch";
            this.btnPatch.Size = new System.Drawing.Size(57, 23);
            this.btnPatch.TabIndex = 75;
            this.btnPatch.Text = "Patch";
            this.toolTip.SetToolTip(this.btnPatch, "Send the patch to channel");
            this.btnPatch.UseVisualStyleBackColor = true;
            this.btnPatch.Click += new System.EventHandler(this.Patch_Click);
            // 
            // lbPatterns
            // 
            this.lbPatterns.BackColor = System.Drawing.SystemColors.Control;
            this.lbPatterns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbPatterns.FormattingEnabled = true;
            this.lbPatterns.ItemHeight = 16;
            this.lbPatterns.Location = new System.Drawing.Point(122, 56);
            this.lbPatterns.Name = "lbPatterns";
            this.lbPatterns.Size = new System.Drawing.Size(131, 178);
            this.lbPatterns.TabIndex = 78;
            this.lbPatterns.SelectedIndexChanged += new System.EventHandler(this.Patterns_SelectedIndexChanged);
            // 
            // MidiPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbPatterns);
            this.Controls.Add(this.chkDrumsOn1);
            this.Controls.Add(this.chkLogMidi);
            this.Controls.Add(this.btnPatch);
            this.Controls.Add(this.cmbPatchList);
            this.Controls.Add(this.txtPatchChannel);
            this.Controls.Add(this.barBar);
            this.Controls.Add(this.cgChannels);
            this.Controls.Add(this.sldTempo);
            this.Name = "MidiPlayer";
            this.Size = new System.Drawing.Size(695, 249);
            this.Load += new System.EventHandler(this.MidiPlayer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private NBagOfTricks.UI.Slider sldTempo;
        private NBagOfTricks.UI.ClickGrid cgChannels;
        private NBagOfTricks.UI.BarBar barBar;
        private TextBox txtPatchChannel;
        private ComboBox cmbPatchList;
        private ToolTip toolTip;
        private CheckBox chkLogMidi;
        private CheckBox chkDrumsOn1;
        private Button btnPatch;
        private ListBox lbPatterns;
    }
}
