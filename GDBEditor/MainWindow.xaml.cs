using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace GDBEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string[] args = Environment.GetCommandLineArgs();

        ContextMenu save;
        ContextMenu edit;
        MenuItem menu_save;
        MenuItem menu_edit;

        public string file;
        public string filename;
        public Dictionary<string, GDBFileImport> gdbFiles = new Dictionary<string, GDBFileImport>();
        public Dictionary<string, GDBTreeHandling> gdbTrees = new Dictionary<string, GDBTreeHandling>();

        //Global list so we can just keep piling into it.
        static public Dictionary<uint, string> FNVHashes = new Dictionary<uint, string>();

        public MainWindow()
        {
            InitializeComponent();
            trv.ContextMenu = null;

            menu_save = new MenuItem();
            menu_save.Header = "Save";
            menu_save.Click += Button_Save;

            menu_edit = new MenuItem();
            menu_edit.Header = "Edit";
            menu_edit.Click += Button_Edit;

            save = new ContextMenu();
            save.Items.Add(menu_save);

            edit = new ContextMenu();
            edit.Items.Add(menu_edit);

            
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "GDB Files|*.gdb|SAVE Files|*.save|All Files|*.*", FilterIndex = 1 };

            // Call the ShowDialog method to show the dialog box.
            var userClickedOK = ofd.ShowDialog();
            if (userClickedOK == true)
            {
                file = ofd.FileName;
                filename = System.IO.Path.GetFileName(file);
                try
                {
                    //load the names and hashes from save file into global list, but otherwise do nothing.
                    if (System.IO.Path.GetExtension(file) == ".save")
                    {
                        XElement xmlHashList = XElement.Load(file);
                        foreach (var node in xmlHashList.Descendants())
                        {
                            if (node.Name == "Entity")
                            {
                                var k = Convert.ToUInt32(node.Value, 16);
                                FNVHashes[k] = node.FirstAttribute.Value;
                            }
                        }
                    }
                    else if (System.IO.Path.GetExtension(file) == ".gdb")
                    {
                        using (BinaryReader gdbBuffer = new BinaryReader(File.Open(file, FileMode.Open)))
                        {
                            trv.Items.Clear();
                            //Load the gdb into memory
                            gdbFiles[filename] = new GDBFileImport(gdbBuffer);

                            //Add the strings to a global list
                            foreach (KeyValuePair<uint, string> fnv in gdbFiles[filename].FNVToString)
                            {
                                if (!FNVHashes.ContainsKey(fnv.Key))
                                {
                                    FNVHashes[fnv.Key] = fnv.Value;
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                }

                //refresh the treeview tree to account for new fnv's each time.
                refresh_view();
            }
        }

        public void TreeViewItem_RightClick(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem)
                if (!((TreeViewItem)sender).IsSelected)
                    return;
            TreeViewItem item = trv.SelectedItem as TreeViewItem;
            if (item != null && item.Tag is TreeGDBFile)
            {
                trv.ContextMenu = save;
            }
            if (item != null && item.Tag is TreeGDBObjectData)
            {
                trv.ContextMenu = edit;
            }
        }

        //For Lazy Loading
        public void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();
                if (item.Tag is TreeGDBFile)
                {
                    foreach (var folder in gdbTrees[item.Header.ToString()].partitions)
                        item.Items.Add(new TreeViewItem() { Header = folder.Name, Tag = folder, Items = { "Loading..." } });
                }
                if (item.Tag is TreeGDBPartition)
                {
                    foreach (var gdbo in (item.Tag as TreeGDBPartition).TreeGDBObject)
                        item.Items.Add(new TreeViewItem() { Header = gdbo.Name, Tag = gdbo, Items = { "Loading..." } });
                }
                if (item.Tag is TreeGDBObject)
                {
                    foreach (TreeGDBObjectData gdbv in (item.Tag as TreeGDBObject).TreeGDBObjectData)
                    {
                        if (gdbv.Data is GDBTreeHandling.GDBTreeItem)
                        {
                            var gdbnode = GDBTreeHandling.GetTreeNode((GDBTreeHandling.GDBTreeItem)gdbv.Data);
                            item.Items.Add(new TreeViewItem() { Header = "parent ["+gdbnode.Name+"]", Tag = gdbnode, Items = { "Loading..." } });
                        }
                        else
                        {
                            item.Items.Add(new TreeViewItem() { Header = gdbv.Name + " [" + gdbv.Data.ToString() + "] ", Tag = gdbv });
                        }
                    }   
                }
            }
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            //quick sanity checking
            if (sender is TreeViewItem)
                if (!((TreeViewItem)sender).IsSelected)
                    return;
            TreeViewItem item = trv.SelectedItem as TreeViewItem;

            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog { Filter = "GDB Files|*.gdb|All Files|*.*", FilterIndex = 1 };
            GDBFileExport fileUtil = new GDBFileExport();

            // Call the ShowDialog method to show the dialog box.
            var userClickedOK = sfd.ShowDialog();
            if (userClickedOK == true)
            {
                string filepath = sfd.FileName;
                GDBFileImport fileStream = gdbFiles[(string)item.Header];
                fileUtil.SaveFile(filepath, fileStream);
            }
        }

        private void Button_Edit(object sender, RoutedEventArgs e)
        {
            //quick sanity checking
            if (sender is TreeViewItem)
                if (!((TreeViewItem)sender).IsSelected)
                    return;
            TreeViewItem item = trv.SelectedItem as TreeViewItem;

            TreeGDBObjectData child = item.Tag as TreeGDBObjectData;
            var textbox = new TextBox();
            textbox.Text = (string)child.Data;

            item.Header = textbox;
            textbox.KeyDown += (o, a) =>
            {
                if (a.Key == Key.Enter)
                {
                    child.Data = textbox.Text;
                    var converted = GDB_Util.ConvertToData(child.Type, child.Data);

                    //Need to get the index of the data node so we can update it directly.
                    TreeViewItem root = trv.Items[0] as TreeViewItem;
                    TreeViewItem p = item.Parent as TreeViewItem;
                    TreeGDBObject parent = p.Tag as TreeGDBObject;

                    //Dictionary Shenanigans
                    gdbFiles[(string)root.Header].RecordDict[parent.Data.hash].rowdata.RowDataBytes[child.Index] = converted;
                    refresh_view();
                }
            };
        }

        //String Search
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            trv.Items.Clear();

            if (textbox.Text == "")
            {
                foreach (KeyValuePair<string, GDBTreeHandling> file in gdbTrees)
                {
                    trv.Items.Add(new TreeViewItem() { Header = file.Key, Tag = new TreeGDBFile(), Items = { "Loading..." } });
                }
            }
            else
            {
                List<TreeGDBObject> children = new List<TreeGDBObject>();
                List<TreeGDBObject> parent = new List<TreeGDBObject>();

                //Rebuild Tree with just searched item
                foreach (KeyValuePair<string, GDBTreeHandling> file in gdbTrees)
                {
                    var root = new TreeViewItem { Header = file.Key, Tag = new TreeGDBFile() };

                    //Get the found object
                    foreach (var gdbtree in file.Value.partitions)
                    {
                        foreach (var gdbobject in gdbtree.TreeGDBObject)
                        {
                            if ((Regex.IsMatch(gdbobject.Name, textbox.Text) || 
                                 Regex.IsMatch(gdbobject.Data.fnv.ToString(), textbox.Text) || 
                                 Regex.IsMatch(gdbobject.Data.hash.ToString(), textbox.Text) || 
                                 Regex.IsMatch(gdbobject.Data.name,textbox.Text) || 
                                 gdbobject.Data.column_string.Contains(textbox.Text)))
                            {
                                children.Add(gdbobject);
                            }
                        }
                    }

                    //Let's get it's parent
                    foreach (var gdbtree in file.Value.partitions)
                    {
                        foreach (var gdbobject in gdbtree.TreeGDBObject)
                        {
                            foreach (TreeGDBObjectData gbdobject_parameter in gdbobject.TreeGDBObjectData)
                            {
                                foreach(var child in children)
                                {
                                    if(gbdobject_parameter.Data is GDBTreeHandling.GDBTreeItem)
                                    {
                                        var temp = (GDBTreeHandling.GDBTreeItem)gbdobject_parameter.Data;
                                        if (temp.hash == child.Data.hash)
                                        {
                                            if (!parent.Contains(gdbobject)) { parent.Add(gdbobject); }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //No parent, so just give me the root
                    if (parent.Count == 0)
                    {
                        foreach (var c in children)
                            root.Items.Add(new TreeViewItem() { Header = c.Name, Tag = c, Items = { "Loading..." } });
                    }
                    else
                    {
                        foreach (var p in parent) { root.Items.Add(new TreeViewItem() { Header = p.Name, Tag = p, Items = { "Loading..." } }); }
                    }

                    trv.Items.Add(root);
                }
            }
        }

        private void refresh_view()
        {
            trv.Items.Clear();
            foreach (var file in gdbFiles.Keys)
            {
                gdbTrees[file] = new GDBTreeHandling(gdbFiles[file]);
                trv.Items.Add(new TreeViewItem() { Header = file, Tag = new TreeGDBFile(), Items = { "Loading..." } });
            }
        }

        public uint SwapBytes(uint x)
        {
            // swap adjacent 16-bit blocks
            x = (x >> 16) | (x << 16);
            // swap adjacent 8-bit blocks
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }
    }
}