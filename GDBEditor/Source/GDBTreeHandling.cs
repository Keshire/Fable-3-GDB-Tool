using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{
    public class GDBTreeHandling
    {
        public SortedDictionary<UInt16,List<GDBObjectTreeItem>> LayerList = new SortedDictionary<ushort, List<GDBObjectTreeItem>>();
        public List<GDBObjectTreeItem> GDBObjectTree = new List<GDBObjectTreeItem>();

        public class GDBObjectTreeItem
        {
            public UInt16 ObjectFolder;
            public uint   ObjectHash;
            public string ObjectLabel;

            public TData ObjectData;
            public Template ObjectDataTemplate;
            public List<string> ObjectDataLabels = new List<string>();
            
        }

        public GDBTreeHandling(GDBFileHandling gdbObject)
        {
            for (int i = 0; i < gdbObject.TDCount; i++)
            {
                var item = new GDBObjectTreeItem
                {
                    ObjectFolder = gdbObject.Layer[i],
                    ObjectHash = gdbObject.ObjectHash[i],
                    ObjectData = gdbObject.TemplateData[i],
                    ObjectDataTemplate = gdbObject.TemplateDictionary[gdbObject.TemplateData[i].OffsetToTemplate]
                };

                foreach (var fnvhash in gdbObject.TemplateDictionary[gdbObject.TemplateData[i].OffsetToTemplate].ObjectHashList)
                {
                    if (gdbObject.FNVHashes.ContainsKey(fnvhash))
                    {
                        item.ObjectDataLabels.Add(gdbObject.FNVHashes[fnvhash]);
                    }
                    else
                    {
                        item.ObjectDataLabels.Add("Label not found");
                    }
                }


                if (gdbObject.ObjectLabels.ContainsKey(item.ObjectHash))
                {
                    if (gdbObject.FNVHashes.ContainsKey(gdbObject.ObjectLabels[item.ObjectHash]))
                    {
                        item.ObjectLabel = gdbObject.FNVHashes[gdbObject.ObjectLabels[item.ObjectHash]];
                    }
                    else
                    {
                        item.ObjectLabel = item.ObjectHash.ToString("X8") + " String not found";
                    }
                }
                else
                {
                    item.ObjectLabel = item.ObjectHash.ToString("X8") + " Object not found";
                }

                GDBObjectTree.Add(item);
            }

            foreach(var item in GDBObjectTree)
            {
                if (LayerList.ContainsKey(item.ObjectFolder))
                {
                    LayerList[item.ObjectFolder].Add(item); //Change this to label via fnvhash list
                }
                else
                {
                    LayerList.Add(item.ObjectFolder, new List<GDBObjectTreeItem> { item });
                }
            }
        }
    }
}
