using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{

    public class TreeGDBRegion
    {
        public string Name { get; set; }
        public ObservableCollection<TreeGDBObject> TreeGDBObject { get; set; }

        public TreeGDBRegion()
        {
            this.TreeGDBObject = new ObservableCollection<TreeGDBObject>();
        }
    }

    public class TreeGDBObject
    {
        public string Name { get; set; }
        public ObservableCollection<TreeGDBObjectData> TreeGDBObjectData { get; set; }

        public TreeGDBObject()
        {
            this.TreeGDBObjectData = new ObservableCollection<TreeGDBObjectData>();
        }
    }

    public class TreeGDBObjectData
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public ObservableCollection<TreeGDBObject> TreeGDBObject { get; set; }

        public TreeGDBObjectData()
        {
            this.TreeGDBObject = new ObservableCollection<TreeGDBObject>();
        }
    }
}
