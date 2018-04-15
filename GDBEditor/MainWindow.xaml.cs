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

                        var Folders = new List<TreeGDBRegion>();
                        foreach (var folder in gdbTree.LayerList.Keys)
                        {
                            var root = new TreeGDBRegion() { Name = folder.ToString("X4") };
                            foreach (var parent in gdbTree.LayerList[folder])
                            {
                                var node1 = new TreeGDBObject() { Name = parent.ObjectLabel };
                                for(int i = 0; i < parent.ObjectData.TemplateData.Count(); i++)
                                {
                                    var node2 = new TreeGDBObjectData();
                                    node2.Name = parent.ObjectDataLabels[i];

                                    var datatype = parent.ObjectDataTemplate.ObjectDatatype[(UInt16)i];
                                    switch (datatype)
                                    {
                                        case 0x0000:
                                            node2.Data = Convert.ToBoolean(BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0)).ToString();  //boolean
                                            break;
                                        case 0x0100:
                                            node2.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(parent.ObjectData.TemplateData[i],0).ToString("X8"); //= dword
                                            break;
                                        case 0x0200:
                                            node2.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0).ToString("X8"); //= dword lots of GroupIndex
                                            break;
                                        case 0x0300:
                                            node2.Data = BitConverter.ToSingle(parent.ObjectData.TemplateData[i], 0).ToString();
                                            break;
                                        case 0x0400:
                                            if (gdbObject.FNVHashes.ContainsKey(BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0)))
                                            {
                                                node2.Data = gdbObject.FNVHashes[BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0)]; //= string hash
                                            }
                                            else
                                            {
                                                node2.Data = BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0).ToString("X8") + " - String not found";
                                            }
                                            break;
                                        case 0x0500:
                                            node2.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0).ToString("X8"); //= numeric indicating an enumerated type
                                            break;
                                        case 0x0600:
                                            node2.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0).ToString("X8"); //= object hash
                                            break;
                                        case 0x0700:
                                            node2.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0).ToString("X8"); //= object hash
                                            break;
                                        default:
                                            node2.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(parent.ObjectData.TemplateData[i], 0).ToString("X8");
                                            break;
                                    }
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