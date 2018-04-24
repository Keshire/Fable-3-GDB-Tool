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
        public List<GDBFileHandling> gdbObjects = new List<GDBFileHandling>();
        public Dictionary<string, GDBTreeHandling> gdbTrees = new Dictionary<string, GDBTreeHandling>();
        public List<string> openfiles = new List<string>();
        //public FileIO efile = new FileIO();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "GDB Files|*.gdb|All Files|*.*", FilterIndex = 1 };

            // Call the ShowDialog method to show the dialog box.
            var userClickedOK = ofd.ShowDialog();

            if (userClickedOK == true) // Test result.
            {
                file = ofd.FileName;
                filename = System.IO.Path.GetFileName(file);
                try
                {
                    using (BinaryReader gdbBuffer = new BinaryReader(File.Open(file, FileMode.Open)))
                    {
                        //This should be a list for when we switch to opening more files.
                        //gdbObjects.Add(new GDBFileHandling(gdbBuffer));

                        //gdbObject needs to be rebuilt as a readable tree to plug into a tree view.
                        gdbTrees[filename] = new GDBTreeHandling(new GDBFileHandling(gdbBuffer));

                        //Root
                        trv.Items.Add(new TreeViewItem() { Header = filename, Tag = new TreeGDBFile(), Items = { "Loading..." } });

                    }
                }
                catch (IOException)
                {
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //Eventually allow editing and saving.
        }

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
                    foreach (var gdbv in (item.Tag as TreeGDBObject).TreeGDBObjectData)
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
    }
}