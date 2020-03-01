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
        public string Name { get; set; }
        public List<TreeGDBPartition> TreeGDBRegion { get; set; }
    }

    public class TreeGDBPartition
    {
        public string Name { get; set; }
        public List<TreeGDBObject> TreeGDBObject { get; set; }
    }

    public class TreeGDBObject
    {
        public string Name { get; set; }
        public GDBTreeHandling.GDBTreeItem Data { get; set; }
        public List<TreeGDBObjectData> TreeGDBObjectData { get; set; }
    }

    public class TreeGDBObjectData
    {
        public string Name { get; set; }
        public object Data { get; set; }
        public UInt16 Type { get; set; }
        public int Index { get; set; }
    }
}
