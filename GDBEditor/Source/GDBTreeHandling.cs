using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{
    public class GDBTreeHandling
    {
        public Dictionary<UInt16,List<GDBObjectTreeItem>> RegionList = new Dictionary<ushort, List<GDBObjectTreeItem>>();
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
                    ObjectFolder = gdbObject.Unknown[i],
                    ObjectHash = gdbObject.HashIndex[i],
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
                        item.ObjectDataLabels.Add("Template Label Not Found");
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
                        item.ObjectLabel = "String Not Located";
                    }
                }
                else
                {
                    item.ObjectLabel = "Object Not Located";
                }

                GDBObjectTree.Add(item);
            }

            foreach(var item in GDBObjectTree)
            {
                if (RegionList.ContainsKey(item.ObjectFolder))
                {
                    RegionList[item.ObjectFolder].Add(item); //Change this to label via fnvhash list
                }
                else
                {
                    RegionList.Add(item.ObjectFolder, new List<GDBObjectTreeItem> { item });
                }
            }
        }
    }
}
