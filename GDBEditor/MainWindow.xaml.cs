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

        public ContextMenu mnu;

        public string file;
        public string filename;
        public Dictionary<string, GDBFileImport> gdbFiles = new Dictionary<string, GDBFileImport>();
        public Dictionary<string, GDBTreeHandling> gdbTrees = new Dictionary<string, GDBTreeHandling>();

        //Global list so we can just keep piling into it.
        static public Dictionary<uint, string> FNVHashes = new Dictionary<uint, string>();

        public MainWindow()
        {
            InitializeComponent();

            //Hide it till we need it.
            mnu = trv.ContextMenu;
            trv.ContextMenu = null;
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
                                if (!FNVHashes.ContainsKey(Convert.ToUInt32(node.Value, 16)))
                                {
                                    FNVHashes[Convert.ToUInt32(node.Value, 16)] = node.FirstAttribute.Value;
                                }
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
            trv.ContextMenu = null;
            if (item != null && item.Tag is TreeGDBFile)
            {
                trv.ContextMenu = mnu;
            }
        }

        public void TreeViewItem_DoubleClick(object sender, RoutedEventArgs e)
        {
            //For Editing
            if (sender is TreeViewItem)
                if (!((TreeViewItem)sender).IsSelected)
                    return;
            TreeViewItem item = trv.SelectedItem as TreeViewItem;
            if (item != null && item.Tag is TreeGDBObjectData)
            {
                //We want to edit the base gdbfile here.
                TreeGDBObjectData child = item.Tag as TreeGDBObjectData;


                //TextBox nd = new TextBox();
                //var od = child.Data;
                //child.Data = "0.5";
                //child.Data = GDB_Util.ConvertToData(child.Type, child.Data);

                //Need to get the index of the data node so we can update it directly.
                TreeViewItem root = trv.Items[0] as TreeViewItem;
                TreeViewItem p = item.Parent as TreeViewItem;
                TreeGDBObject parent = p.Tag as TreeGDBObject;

                //Dictionary Shenanigans
                //gdbFiles[(string)root.Header].RecordDict[parent.Data.hash].rowdata.RowDataBytes[child.Index] = (Byte[])child.Data;
                //refresh_view();
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
            TreeViewItem itemToSave = trv.SelectedItem as TreeViewItem;

            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog { Filter = "GDB Files|*.gdb|All Files|*.*", FilterIndex = 1 };
            FileIO fileUtil = new FileIO();

            // Call the ShowDialog method to show the dialog box.
            var userClickedOK = sfd.ShowDialog();
            if (userClickedOK == true)
            {
                string filepath = sfd.FileName;
                GDBFileImport fileStream = gdbFiles[(string)itemToSave.Header];
                fileUtil.SaveFile(filepath, fileStream);
            }
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
                //Rebuild Tree with just searched item
                foreach (KeyValuePair<string, GDBTreeHandling> file in gdbTrees)
                {
                    var root = new TreeViewItem { Header = file.Key, Tag = new TreeGDBFile() };
                    foreach (var gdbtree in file.Value.partitions)
                    {
                        var folder = new TreeViewItem { Header = gdbtree.Name, Tag = gdbtree };
                        foreach (var gdbobject in gdbtree.TreeGDBObject)
                        {
                            if (Regex.IsMatch(gdbobject.Name,textbox.Text))
                            {
                                folder.Items.Add(new TreeViewItem() { Header = gdbobject.Name, Tag = gdbobject, Items = { "Loading..." } });
                            }
                            else
                            {
                                Boolean add_folder = false;
                                foreach (TreeGDBObjectData gbdobject_parameter in gdbobject.TreeGDBObjectData)
                                {
                                    if (Regex.IsMatch(gdbobject.Name, textbox.Text) && !add_folder)
                                    {
                                        add_folder = true;
                                        folder.Items.Add(new TreeViewItem() { Header = gdbobject.Name, Tag = gdbobject, Items = { "Loading..." } });
                                    }
                                }
                            }
                        }
                        root.Items.Add(folder);
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
    }
}