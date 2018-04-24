using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{

    public class TreeGDBFile
    {
        public TreeGDBFile()
        {
            this.TreeGDBRegion = new ObservableCollection<TreeGDBRegion>();
        }
        public string Name { get; set; }
        public ObservableCollection<TreeGDBRegion> TreeGDBRegion { get; set; }
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
        public TreeGDBObjectData()
        {
            this.TreeGDBObject = new ObservableCollection<TreeGDBObject>();
        }
        public string Name { get; set; }
        public object Data { get; set; }
        public ObservableCollection<TreeGDBObject> TreeGDBObject { get; set; }
    }
}
