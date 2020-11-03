# ClipExplorer
Tool for managing audio clips.

WASAPI can be bit perfect and even in shared mode it uses high quality resampler.Generally speaking though, when it comes to     //pure quality, it can't get any better than what exclusive WASAPI can offer. Not entirely sure how Direct Sound is handled in Windows 10 but it's either equal to WASAPI shared mode or worse. 


```
C:\DEV\REPOS\CLIPEXPLORER
|   ss1.png  
|   ss2.png
|   ss3.png
|   TempDlg.cs
|   TempDlg.Designer.cs
|   UserSettings.cs
+---bin
|   +---Debug
|   |       ClipExplorer.exe
|   |       ClipExplorer.exe.config
|   |       ClipExplorer.pdb
|   |       NAudio.dll
|   |       NAudio.xml
|   \---Release
+---lib
|       NBagOfTricks.dll
|       NBagOfTricks.xml
+---obj
|   \---Debug
|       |   .NETFramework,Version=v4.7.2.AssemblyAttributes.cs
|       |   ClipExplorer.csproj.CopyComplete
|       |   ClipExplorer.csproj.CoreCompileInputs.cache
|       \---TempPE
|               Properties.Resources.Designer.cs.dll
+---packages
|   +---NAudio.1.10.0
|   |   |   .signature.p7s
|   |   |   NAudio.1.10.0.nupkg
|   |   |   
|   |   \---lib
\---_todo
        Player.cs
        Player.Designer.cs
        Player.resx
```




You can easily find a series of free C# TreeListView controls with full source code in the Internet. 
Perhaps, the oldest of them is TreeListView developed by Thomas Caudal and published on CodeProject in 2002. 
Another famous and open-source C# TreeListView control is ObjectListView hosted on SourceForge. 
Both controls inherit the .NET ListView control and add the TreeView functionality to it. 
Let’s look a little bit more careful at such solutions by the example of ObjectListView, which is based on an enough interesting ideology.

descend from the base class DataGridViewColumn and create your own visual display.
Derived
System.Windows.Forms.DataGridViewButtonColumn
System.Windows.Forms.DataGridViewCheckBoxColumn
System.Windows.Forms.DataGridViewComboBoxColumn
System.Windows.Forms.DataGridViewImageColumn
System.Windows.Forms.DataGridViewLinkColumn
System.Windows.Forms.DataGridViewTextBoxColumn

https://docs.microsoft.com/en-us/previous-versions/aa730881(v=vs.80)?redirectedfrom=MSDN

You must set DrawMode property of TreeView to TreeViewDrawMode.OwnerDrawAll. Once you do this, treeview's DrawNode event will fire each time a tree node is being drawn. Handle that event and draw your items manually.

You will get DrawTreeNodeEventArgs as the event arguments. State property of it will tell you which state of the tree item you must draw. e.Bounds will help you for determining bounds and you can use e.Graphics for drawing. You can find more detailed information here:

http://msdn.microsoft.com/en-us/library/system.windows.forms.treeview.drawnode.aspx


