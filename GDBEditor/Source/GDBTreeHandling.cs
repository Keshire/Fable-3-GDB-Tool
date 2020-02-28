using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBEditor
{
    public class GDBTreeHandling
    {
        static public Dictionary<uint, GDBObjectTreeItem> ItemList = new Dictionary<uint, GDBObjectTreeItem>();
        static public Dictionary<uint, string> FNVHashes = new Dictionary<uint, string>();
        public List<TreeGDBRegion> Folders = new List<TreeGDBRegion>();

        public class GDBObjectTreeItem
        {
            public UInt16 partition;
            public uint   hash;
            public string label;

            public RowData data;
            public RowType type;
            public List<string> column_names = new List<string>();
        }

        public GDBTreeHandling(GDBFileHandling gdbObject)
        {

            FNVHashes = MainWindow.FNVHashes;
            var FolderList = new SortedDictionary<UInt16, List<GDBObjectTreeItem>>();
            for (int i = 0; i < gdbObject.header.RecordCount; i++)
            {
                GDBObjectTreeItem item = new GDBObjectTreeItem
                {
                    partition = gdbObject.Records[i].partition,
                    hash = gdbObject.Records[i].hash,
                    data = gdbObject.Records[i].rowdata,
                    type = gdbObject.Records[i].rowtype
                };

                foreach (uint fnvhash in gdbObject.Records[i].rowtype.Column_FNV)
                {
                    item.column_names.Add(FNVHashes[fnvhash]);
                }


                if (gdbObject.RecordToFNV.ContainsKey(item.hash))
                {
                    if (FNVHashes.ContainsKey(gdbObject.RecordToFNV[item.hash]))
                    {
                        item.label = FNVHashes[gdbObject.RecordToFNV[item.hash]];
                    }
                    else if (FNVHashes.ContainsKey(item.hash))
                    {
                        //Comes from outside the gdb
                        item.label = FNVHashes[item.hash];
                    }
                    else
                    {
                        item.label = item.hash.ToString("X8");
                    }
                }
                else if(FNVHashes.ContainsKey(item.hash)) 
                {
                    //Comes from outside the gdb
                    item.label = FNVHashes[item.hash];
                }
                else
                {
                    item.label = item.hash.ToString("X8");
                }


                //Keep a list of items by hash
                ItemList[item.hash] = item;
                
                //Keep a sorted list of items by unknown
                if (FolderList.ContainsKey(item.partition))
                {
                    FolderList[item.partition].Add(item);
                }
                else
                {
                    FolderList.Add(item.partition, new List<GDBObjectTreeItem> { item });
                }
            }

            Folders = new List<TreeGDBRegion>();
            foreach (var folder in FolderList.Keys)
            {
                var root = new TreeGDBRegion() { Name = folder.ToString("X4"), TreeGDBObject = new List<TreeGDBObject>() };
                foreach (var parent in FolderList[folder])
                {
                    root.TreeGDBObject.Add(TreeGDBObject(parent));
                }
                Folders.Add(root);
            }
        }

        static public TreeGDBObject TreeGDBObject(GDBObjectTreeItem item)
        {
            var node = new TreeGDBObject() { Name = item.label, Data = item, TreeGDBObjectData = new List<TreeGDBObjectData>() };
            for (int i = 0; i < item.data.RowDataByteArray.Count(); i++)
            {
                var child = new TreeGDBObjectData() { Name = item.column_names[i] };
                UInt16 datatype = item.type.Data_Type[(UInt16)i];
                switch (datatype)
                {
                    case 0x0000:
                        child.Data = Convert.ToBoolean(BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0)).ToString();  //boolean
                        break;
                    case 0x0100:
                        child.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0).ToString("X8"); //= dword
                        break;
                    case 0x0200:
                        child.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0).ToString("X8"); //= dword lots of GroupIndex
                        break;
                    case 0x0300:
                        child.Data = BitConverter.ToSingle(item.data.RowDataByteArray[i], 0).ToString();
                        break;
                    case 0x0400:
                        if (FNVHashes.ContainsKey(BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0)))
                        {
                            child.Data = FNVHashes[BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0)]; //= string hash
                        }
                        else
                        {
                            child.Data = BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0).ToString("X8");
                        }
                        break;
                    case 0x0500:
                        child.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0).ToString("X8"); //= numeric indicating an enumerated type
                        break;
                    case 0x0600:
                        if (ItemList.ContainsKey(BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0)))
                        {
                            child.Data = ItemList[BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0)];
                        }
                        else
                        {
                            child.Data = BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0).ToString("X8");
                        }
                        break;
                    case 0x0700:
                        if (ItemList.ContainsKey(BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0)))
                        {
                            child.Data = ItemList[BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0)];
                        }
                        else
                        {
                            child.Data = BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0).ToString("X8");
                        }
                        break;
                    default:
                        child.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(item.data.RowDataByteArray[i], 0).ToString("X8");
                        break;
                }
                node.TreeGDBObjectData.Add(child);
            }
            return node;
        }
    }
}
