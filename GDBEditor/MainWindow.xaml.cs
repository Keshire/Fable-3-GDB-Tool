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
        public GDBFileHandling gdbObject;
        public GDBTreeHandling gdbTree;
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
                        gdbObject = new GDBFileHandling(gdbBuffer);

                        //gdbObject needs to be rebuilt as a readable tree to plug into a tree view.
                        gdbTree = new GDBTreeHandling(gdbObject);

                        /*
                        List<Family> families = new List<Family>();

                        Family family1 = new Family() { Name = "The Doe's" };
                        family1.Members.Add(new FamilyMember() { Name = "John Doe", Age = 42 });
                        family1.Members.Add(new FamilyMember() { Name = "Jane Doe", Age = 39 });
                        family1.Members.Add(new FamilyMember() { Name = "Sammy Doe", Age = 13 });
                        families.Add(family1);

                        Family family2 = new Family() { Name = "The Moe's" };
                        family2.Members.Add(new FamilyMember() { Name = "Mark Moe", Age = 31 });
                        family2.Members.Add(new FamilyMember() { Name = "Norma Moe", Age = 28 });
                        families.Add(family2);
                        */
                        var Folders = new List<TreeGDBRegion>();
                        foreach (var folder in gdbTree.RegionList.Keys)
                        {
                            var root = new TreeGDBRegion() { Name = folder.ToString("X") };
                            foreach (var parent in gdbTree.RegionList[folder])
                            {
                                var node1 = new TreeGDBObject() { Name = parent.ObjectLabel };
                                for(int i = 0; i < parent.ObjectData.TemplateData.Count(); i++)
                                {
                                    var node2 = new TreeGDBObjectData()
                                    {
                                        Name = parent.ObjectDataLabels[i],
                                        Data = parent.ObjectData.TemplateData[i].ToString("X")
                                    };

                                    node1.TreeGDBObjectData.Add(node2);
                                }
                                root.TreeGDBObject.Add(node1);
                            }
                            Folders.Add(root);
                        }
                        trvNodes.ItemsSource = Folders;// families;
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
    }

    public class TreeGDBRegion
    {
        public TreeGDBRegion()
        {
            this.TreeGDBObject = new ObservableCollection<TreeGDBObject>();
        }
        public string Name { get; set; }
        public ObservableCollection<TreeGDBObject> TreeGDBObject { get; set; }
    }

    public class TreeGDBObject
    {
        public TreeGDBObject()
        {
            this.TreeGDBObjectData = new ObservableCollection<TreeGDBObjectData>();
        }
        public string Name { get; set; }
        public ObservableCollection<TreeGDBObjectData> TreeGDBObjectData { get; set; }
    }

    public class TreeGDBObjectData
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }
}