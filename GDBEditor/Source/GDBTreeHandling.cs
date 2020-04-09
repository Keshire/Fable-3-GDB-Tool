using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBEditor
{
    public class GDBTreeHandling
    {
        static public Dictionary<uint, GDBTreeItem> ItemList = new Dictionary<uint, GDBTreeItem>();
        static public Dictionary<uint, string> FNVHashes = new Dictionary<uint, string>();
        public List<TreeGDBPartition> partitions = new List<TreeGDBPartition>();

        public class GDBTreeItem
        {
            public UInt16 partition;
            public uint hash;
            public uint fnv;
            public string name;

            public RowData data;
            public RowType type;
            public List<string> column_string = new List<string>();
        }

        public GDBTreeHandling(GDBFileImport gdbObject)
        {

            //Get the global fnv list
            FNVHashes = MainWindow.FNVHashes;
            var partitionList = new SortedDictionary<UInt16, List<GDBTreeItem>>();
            foreach (var record in  gdbObject.Records)
            {
                //get main data
                GDBTreeItem item = new GDBTreeItem
                {
                    partition = record.partition,
                    hash = record.hash,
                    data = record.rowdata,
                    type = record.rowtype
                };

                //Convert hashes to strings for the treeview
                foreach (uint fnvhash in item.type.Column_FNV)
                {
                    item.column_string.Add(FNVHashes[fnvhash]);
                }

                if (gdbObject.RecordToFNV.ContainsKey(item.hash))
                {
                    item.fnv = gdbObject.RecordToFNV[item.hash];
                    if (FNVHashes.ContainsKey(item.fnv))
                    {
                        //Found an actual string
                        item.name = FNVHashes[item.fnv];
                    }
                    else
                    {
                        item.name = "FNV: " + item.fnv.ToString("X8");
                    }
                }
                else if (FNVHashes.ContainsKey(item.fnv))
                {
                    item.name = "EXT: " + FNVHashes[item.fnv];
                }
                else if (FNVHashes.ContainsKey(item.hash))
                {
                    item.name = "EXT: " + FNVHashes[item.hash];
                }
                else
                {
                    item.name = item.name = "HASH: " + item.hash.ToString("X8");
                }

                //Keep a list of items by hash in order to populate from other gdbs or saves
                ItemList[item.hash] = item;
                
                //Keep a sorted list of items by partition, for research purposes. We don't know what this does normally.
                if (partitionList.ContainsKey(item.partition))
                {
                    partitionList[item.partition].Add(item);
                }
                else
                {
                    partitionList.Add(item.partition, new List<GDBTreeItem> { item });
                }
            }

            partitions = new List<TreeGDBPartition>();
            foreach (var partition in partitionList.Keys)
            {
                TreeGDBPartition root = new TreeGDBPartition() { Name = partition.ToString("X4"), TreeGDBObject = new List<TreeGDBObject>() };
                foreach (GDBTreeItem child in partitionList[partition])
                {
                    root.TreeGDBObject.Add(GetTreeNode(child));
                }
                partitions.Add(root);
            }
        }

        static public TreeGDBObject GetTreeNode(GDBTreeItem item)
        {
            var node = new TreeGDBObject() { Name = item.name, Data = item, TreeGDBObjectData = new List<TreeGDBObjectData>() };
            for (int i = 0; i < item.data.RowDataBytes.Count(); i++)
            {
                var child = new TreeGDBObjectData() { Name = item.column_string[i] };
                UInt16 datatype = item.type.Data_Type[(UInt16)i];
                child.Type = datatype;
                child.Index = i;

                var data = GDB_Util.ConvertToType(datatype, item.data.RowDataBytes[i]);
                switch (Type.GetTypeCode(data.GetType()))
                {
                    case TypeCode.UInt32:
                        UInt32 temp = (UInt32)data;
                        if (FNVHashes.ContainsKey(temp)) { child.Data = FNVHashes[temp]; }
                        else if (ItemList.ContainsKey(temp)) { child.Data = ItemList[temp]; }
                        else { child.Data = temp.ToString("X8"); }
                        break;
                    default:
                        child.Data = data.ToString();
                        break;
                }
                node.TreeGDBObjectData.Add(child);
            }
            return node;
        }
    }
}
