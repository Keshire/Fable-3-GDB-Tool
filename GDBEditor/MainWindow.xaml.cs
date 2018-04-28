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
        public string file;
        public string filename;
        public Dictionary<string, GDBFileHandling> gdbObjects = new Dictionary<string, GDBFileHandling>();
        public Dictionary<string, GDBTreeHandling> gdbTrees = new Dictionary<string, GDBTreeHandling>();

        static public Dictionary<uint, string> FNVHashes = new Dictionary<uint, string>();
        //public FileIO efile = new FileIO();

        public MainWindow()
        {
            InitializeComponent();
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
                            gdbObjects[filename] = new GDBFileHandling(gdbBuffer);

                            //Add the strings to a global list
                            foreach (KeyValuePair<uint, string> fnv in gdbObjects[filename].FNVHashes)
                            {
                                if (!FNVHashes.ContainsKey(fnv.Key))
                                {
                                    FNVHashes[fnv.Key] = fnv.Value;
                                }
                            }

                            //Build a tree for the view
                            gdbTrees[filename] = new GDBTreeHandling(gdbObjects[filename]);
                        }
                    }
                }
                catch (IOException)
                {
                }

                //rebuild the treeview tree to account for new fnv's each time.
                foreach (var tree in gdbObjects.Keys)
                {
                    gdbTrees[tree] = new GDBTreeHandling(gdbObjects[tree]);
                    trv.Items.Add(new TreeViewItem() { Header = tree, Tag = new TreeGDBFile(), Items = { "Loading..." } });
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //Eventually allow editing and saving.
        }

        public void TreeViewItem_DoubleClick(object sender, RoutedEventArgs e)
        {
            //For Editing
            if (sender is TreeViewItem)
                if (!((TreeViewItem)sender).IsSelected)
                    return;
            TreeViewItem item = trv.SelectedItem as TreeViewItem;
            if (item.Tag is TreeGDBObjectData)
            {
                TreeViewItem parent = item.Parent as TreeViewItem;
                TreeGDBObject parentgdb = parent.Tag as TreeGDBObject;
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
                    foreach (var folder in gdbTrees[item.Header.ToString()].Folders)
                        item.Items.Add(new TreeViewItem() { Header = folder.Name, Tag = folder, Items = { "Loading..." } });
                }
                if (item.Tag is TreeGDBRegion)
                {
                    foreach (var gdbo in (item.Tag as TreeGDBRegion).TreeGDBObject)
                        item.Items.Add(new TreeViewItem() { Header = gdbo.Name, Tag = gdbo, Items = { "Loading..." } });
                }
                if (item.Tag is TreeGDBObject)
                {
                    foreach (TreeGDBObjectData gdbv in (item.Tag as TreeGDBObject).TreeGDBObjectData)
                    {
                        if (gdbv.Data is GDBTreeHandling.GDBObjectTreeItem)
                        {
                            var gdbnode = GDBTreeHandling.TreeGDBObject((GDBTreeHandling.GDBObjectTreeItem)gdbv.Data);
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
                    foreach (var gdbtree in file.Value.Folders)
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
    }
}