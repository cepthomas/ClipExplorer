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


namespace ClipExplorer // TODOC make generic -> nbot
{
    // TODOC: tree selection displays files in dir with selectable filtertags

    // TODOC: select filtertag(s) and display all entries with full paths


    /// <summary>
    /// 
    /// </summary>
    public partial class Navigator : UserControl
    {
        #region Fields
        /// <summary>
        /// Base path(s) for the tree.
        /// </summary>
        List<string> _rootPaths = new List<string>();

        /// <summary>
        /// Show only these file types.
        /// </summary>
        List<string> _filterExts = new List<string>();
        #endregion

        #region Properties
        /// <summary>
        /// Generate event for single or double click.
        /// </summary>
        public bool DoubleClickSelect { get; set; } = false;

        /// <summary>
        /// All possible tags - client supplies and persists.
        /// </summary>
        public HashSet<string> AllTags = new HashSet<string>();
        #endregion

        #region Events
        /// <summary>
        /// User has selected a file.
        /// </summary>
        public event EventHandler<string> FileSelectedEvent = null;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Default constructor.
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
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootPaths">Where the files be.</param>
        /// <param name="filterExts">Oly these kinds.</param>
        public void Init(List<string> rootPaths, List<string> filterExts)
        {
            _rootPaths = rootPaths;
            _filterExts = filterExts;
            PopulateTreeView();
        }

        /// <summary>
        /// 
        /// </summary>
        void PopulateTreeView()
        {
            foreach(string path in _rootPaths)
            {
                TreeNode rootNode;

                DirectoryInfo info = new DirectoryInfo(path);
                if (info.Exists)
                {
                    rootNode = new TreeNode(info.Name)
                    {
                        Tag = info
                    };

                    ShowDirectories(info.GetDirectories(), rootNode);
                    treeView.Nodes.Add(rootNode);
                }
                // else error... TODOC

            }

            foreach (TreeNode n in treeView.Nodes)
            {
                n.Expand();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirs"></param>
        /// <param name="parentNode"></param>
        void ShowDirectories(DirectoryInfo[] dirs, TreeNode parentNode)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                TreeNode subDirNode = new TreeNode(dir.Name, 0, 0)
                {
                    Tag = dir,
                    ImageKey = "folder"
                };

                // Go a little lower now.
                DirectoryInfo[] subDirs = dir.GetDirectories();
                ShowDirectories(subDirs, subDirNode);
                parentNode.Nodes.Add(subDirNode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_MouseClick(object sender, MouseEventArgs e)
        {
            TreeNode clickedNode = treeView.GetNodeAt(e.X, e.Y);

            lvFiles.Items.Clear();
            var nodeDirInfo = clickedNode.Tag as DirectoryInfo;

            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                if(_filterExts.Contains(Path.GetExtension(file.Name)))
                {
                    var item = new ListViewItem(new[] { file.Name, "TODOC tags" });
                    item.Tag = file.FullName;
                    lvFiles.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListFiles_Click(object sender, EventArgs e)
        {
            if(!DoubleClickSelect && FileSelectedEvent != null)
            {
                FileSelectedEvent.Invoke(this, lvFiles.SelectedItems[0].Tag.ToString());
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListFiles_DoubleClick(object sender, EventArgs e)
        {
            if (DoubleClickSelect && FileSelectedEvent != null)
            {
                FileSelectedEvent.Invoke(this, lvFiles.SelectedItems[0].Tag.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Navigator_Resize(object sender, EventArgs e)
        {
            lvFiles.Columns[0].Width = lvFiles.Width / 2;
            lvFiles.Columns[1].Width = -2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditTags_Click(object sender, EventArgs e)
        {
            // TODOC: edit filtertags, check for invalid or in use.

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterByTags_DropDownOpening(object sender, EventArgs e)
        {
            //I'm doing this in two places. In one of them, it turns out (I forgot) that every one was a check box. I was able to get this working by:
            //Instead of putting the dropdowns directly into the button, creating a context menu and loading that as the list; then calling the closing event for that context menu, and not allowing a close on ItemClick.

            //   Add all the ToolStripMenuItems to the NoCloseItems Array in the Form.Load event that you want to make the menu stay open when they are clicked.Then you iterate through all those items and add a single Paint event handler sub to them.
            //That Paint event will be raised every time one of them is highlighted (selected) or un-highlighted (not selected).  In the Paint even you can cast the sender Object to a ToolStripMenuItem and find if it is Selected or not.
            //If it is selected then, set its OwnerItem.DropDown.AutoClose property to False so it will not close if the item is clicked.If it is not selected then set it to True so the DropDown will close when something else on the form gets the focus.
            //You will want to use the Paint event instead of the Mouse events because, the user may be using the keyboard to navigate and select the menu items instead of the mouse.

        }
    }
}
