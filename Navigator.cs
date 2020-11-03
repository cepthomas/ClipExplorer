using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace ClipExplorer
{
    //TODOC modes:
    // Mode: tree selection displays files in dir with selectable filtertags - click to play
    // Mode: select ftag(s) and display all entries with full paths - click to play
    // Mode: edit ftags, check for invalid or in use.

    public partial class Navigator : UserControl
    {
        #region Lifecycle
        /// <summary>
        /// 
        /// </summary>
        public Navigator()
        {
            InitializeComponent();
        }

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Navigator_Load(object sender, EventArgs e)
        {
           // PopulateTreeView();

            // Add tags containing alert messages to a few nodes 
            // and set the node background color to highlight them.
            //treeView.Nodes[1].Nodes[0].Tag = "urgent!";
            //treeView.Nodes[1].Nodes[0].BackColor = Color.Yellow;
            //treeView.SelectedNode = treeView.Nodes[1].Nodes[0];
            //treeView.Nodes[2].Nodes[1].Tag = "urgent!";
            //treeView.Nodes[2].Nodes[1].BackColor = Color.Yellow;

            // Configure the TreeView control for owner-draw and add a handler for the DrawNode event.

            //treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            //treeView.DrawNode += new DrawTreeNodeEventHandler(treeView_DrawNode);

            // Add a handler for the MouseDown event so that a node can be 
            // selected by clicking the tag text as well as the node text.
            treeView.MouseDown += new MouseEventHandler(treeView_MouseDown);
            treeView.NodeMouseClick += new TreeNodeMouseClickEventHandler(TreeView_NodeMouseClick);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void Init(string path)
        {
            PopulateTreeView(path);
        }

        /// <summary>
        /// 
        /// </summary>
        void PopulateTreeView(string path)
        {
            TreeNode rootNode;

            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name)
                {
                    Tag = info
                };

                GetDirectories(info.GetDirectories(), rootNode);
                treeView.Nodes.Add(rootNode);
            }
            // else error...
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subDirs"></param>
        /// <param name="nodeToAddTo"></param>
        void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode subDirNode;
            DirectoryInfo[] subSubDirs;

            foreach (DirectoryInfo subDir in subDirs)
            {
                subDirNode = new TreeNode(subDir.Name, 0, 0)
                {
                    Tag = subDir,
                    ImageKey = "folder"
                };

                // Get the files.
 //               subDir.GetFiles()

                // Recurse.
                subSubDirs = subDir.GetDirectories();
                GetDirectories(subSubDirs, subDirNode);
                nodeToAddTo.Nodes.Add(subDirNode);
            }
        }

        // Draws a node.
        private void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // Draw the background and node text for a selected node.
            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                // Draw the background of the selected node. The NodeBounds
                // method makes the highlight rectangle large enough to
                // include the text of a node tag, if one is present.
                e.Graphics.FillRectangle(Brushes.Green, NodeBounds(e.Node));

                // Retrieve the node font. If the node font has not been set,
                // use the TreeView font.
                Font nodeFont = e.Node.NodeFont;
                if (nodeFont == null)
                {
                    nodeFont = ((TreeView)sender).Font;
                }

                // Draw the node text.
                e.Graphics.DrawString(e.Node.Text, nodeFont, Brushes.White, Rectangle.Inflate(e.Bounds, 2, 0));
            }
            // Use the default background and node text.
            else
            {
                e.DrawDefault = true;
            }

            // If a node tag is present, draw its string representation to the right of the label text.
            if (e.Node.Tag != null)
            {
                e.Graphics.DrawString(e.Node.Tag.ToString(), this.Font, Brushes.Yellow, e.Bounds.Right + 2, e.Bounds.Top);
            }

            // If the node has focus, draw the focus rectangle large, making it large enough to include the text of the node tag, if present.
            if ((e.State & TreeNodeStates.Focused) != 0)
            {
                using (Pen focusPen = new Pen(Color.Black))
                {
                    focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    Rectangle focusBounds = NodeBounds(e.Node);
                    focusBounds.Size = new Size(focusBounds.Width - 1,
                    focusBounds.Height - 1);
                    e.Graphics.DrawRectangle(focusPen, focusBounds);
                }
            }
        }

        // Selects a node that is clicked on its label or tag text.
        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode clickedNode = treeView.GetNodeAt(e.X, e.Y);
            if (NodeBounds(clickedNode).Contains(e.X, e.Y))
            {
                treeView.SelectedNode = clickedNode;
            }
        }

        // Returns the bounds of the specified node, including the region 
        // occupied by the node label and any node tag displayed.
        private Rectangle NodeBounds(TreeNode node)
        {
            // Set the return value to the normal node bounds.
            Rectangle bounds = node.Bounds;
            if (node.Tag != null)
            {
                // Retrieve a Graphics object from the TreeView handle
                // and use it to calculate the display width of the tag.
                Graphics g = treeView.CreateGraphics();
                int tagWidth = (int)g.MeasureString(node.Tag.ToString(), this.Font).Width + 6;

                // Adjust the node bounds using the calculated value.
                bounds.Offset(tagWidth / 2, 0);
                bounds = Rectangle.Inflate(bounds, tagWidth / 2, 0);
                g.Dispose();
            }

            return bounds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listViewFiles.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"),
                      new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listViewFiles.Items.Add(item);
            }

            listViewFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }
}
