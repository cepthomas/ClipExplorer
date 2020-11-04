namespace ClipExplorer
{
    partial class Navigator
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeView = new System.Windows.Forms.TreeView();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.dvcolFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dvcolTags = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnEditTags = new System.Windows.Forms.ToolStripButton();
            this.btnFilterByTags = new System.Windows.Forms.ToolStripDropDownButton();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(257, 585);
            this.treeView.TabIndex = 0;
            this.treeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseClick);
            // 
            // lvFiles
            // 
            this.lvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.dvcolFile,
            this.dvcolTags});
            this.lvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFiles.FullRowSelect = true;
            this.lvFiles.HideSelection = false;
            this.lvFiles.Location = new System.Drawing.Point(0, 0);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(327, 585);
            this.lvFiles.TabIndex = 1;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            this.lvFiles.Click += new System.EventHandler(this.ListFiles_Click);
            this.lvFiles.DoubleClick += new System.EventHandler(this.ListFiles_DoubleClick);
            // 
            // dvcolFile
            // 
            this.dvcolFile.Text = "File";
            this.dvcolFile.Width = 377;
            // 
            // dvcolTags
            // 
            this.dvcolTags.Text = "Tags";
            this.dvcolTags.Width = 365;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnFilterByTags,
            this.btnEditTags});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(588, 27);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnEditTags
            // 
            this.btnEditTags.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnEditTags.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditTags.Name = "btnEditTags";
            this.btnEditTags.Size = new System.Drawing.Size(72, 24);
            this.btnEditTags.Text = "Edit Tags";
            this.btnEditTags.Click += new System.EventHandler(this.EditTags_Click);
            // 
            // btnFilterByTags
            // 
            this.btnFilterByTags.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFilterByTags.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFilterByTags.Name = "btnFilterByTags";
            this.btnFilterByTags.Size = new System.Drawing.Size(109, 24);
            this.btnFilterByTags.Text = "Filter by Tags";
            this.btnFilterByTags.DropDownOpening += new System.EventHandler(this.FilterByTags_DropDownOpening);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 27);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.lvFiles);
            this.splitContainer2.Size = new System.Drawing.Size(588, 585);
            this.splitContainer2.SplitterDistance = 257;
            this.splitContainer2.TabIndex = 4;
            // 
            // Navigator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Navigator";
            this.Size = new System.Drawing.Size(588, 612);
            this.Load += new System.EventHandler(this.Navigator_Load);
            this.Resize += new System.EventHandler(this.Navigator_Resize);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ListView lvFiles;
        private System.Windows.Forms.ColumnHeader dvcolFile;
        private System.Windows.Forms.ColumnHeader dvcolTags;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnEditTags;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripDropDownButton btnFilterByTags;
    }
}
