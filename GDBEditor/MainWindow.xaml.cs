﻿using System;
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
}