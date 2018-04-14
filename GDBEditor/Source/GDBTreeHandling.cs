using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{
    public class GDBTreeHandling
    {
        public Dictionary<UInt16,List<TData>> RegionList = new Dictionary<ushort, List<TData>>();
        public List<GDBObjectTreeItem> GDBObjectTree = new List<GDBObjectTreeItem>();

        public class GDBObjectTreeItem
        {
            public UInt16 Folder;
            public string LabelHash;
            public string Label;
            public TData Data;
        }

        public GDBTreeHandling(GDBFileHandling gdbObject)
        {
            for (int i = 0; i < gdbObject.TDCount; i++)
            {
                var item = new GDBObjectTreeItem { Folder = gdbObject.Unknown[i], LabelHash = gdbObject.HashIndex[i].ToString("X") };

                if (gdbObject.fnvhashes.ContainsKey(gdbObject.HashIndex[i]))
                {
                    item.Label = gdbObject.fnvhashes[gdbObject.HashIndex[i]];
                }
                item.Data = gdbObject.TemplateData[i];
                GDBObjectTree.Add(item);
            }

            foreach(var item in GDBObjectTree)
            {
                if (RegionList.ContainsKey(item.Folder))
                {
                    RegionList[item.Folder].Add(item.Data); //Change this to label via fnvhash list
                }
                else
                {
                    RegionList.Add(item.Folder, new List<TData> { item.Data });
                }
            }
        }
    }
}
