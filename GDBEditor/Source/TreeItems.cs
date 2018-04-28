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
        public List<TreeGDBRegion> TreeGDBRegion { get; set; }
    }

    public class TreeGDBRegion
    {
        public string Name { get; set; }
        public List<TreeGDBObject> TreeGDBObject { get; set; }
    }

    public class TreeGDBObject
    {
        public string Name { get; set; }
        public GDBTreeHandling.GDBObjectTreeItem Data { get; set; }
        public List<TreeGDBObjectData> TreeGDBObjectData { get; set; }
    }

    public class TreeGDBObjectData
    {
        public string Name { get; set; }
        public object Data { get; set; }
    }
}
