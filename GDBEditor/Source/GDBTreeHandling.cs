﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBEditor
{
    public class GDBTreeHandling
    {
        static public Dictionary<uint, GDBObjectTreeItem> ItemList = new Dictionary<uint, GDBObjectTreeItem>();
        public List<TreeGDBRegion> Folders = new List<TreeGDBRegion>();

        public class GDBObjectTreeItem
        {
            public GDBFileHandling gdbObject;
            public UInt16 ObjectFolder;
            public uint   ObjectHash;
            public string ObjectLabel;

            public TemplateData ObjectData;
            public Template ObjectDataTemplate;
            public List<string> ObjectDataLabels = new List<string>();
            
        }

        public GDBTreeHandling(GDBFileHandling gdbObject)
        {
            var FolderList = new SortedDictionary<UInt16, List<GDBObjectTreeItem>>();
            for (int i = 0; i < gdbObject.ObjectCount; i++)
            {
                var item = new GDBObjectTreeItem();

                item.gdbObject = gdbObject;
                item.ObjectFolder = gdbObject.Unknown_UInt16[i];
                item.ObjectHash = gdbObject.ObjectHash[i];
                item.ObjectData = gdbObject.TemplateData[i];
                item.ObjectDataTemplate = gdbObject.TemplateDictionary[gdbObject.TemplateData[i].OffsetToTemplate];
                
                foreach (var fnvhash in gdbObject.TemplateDictionary[gdbObject.TemplateData[i].OffsetToTemplate].ObjectHashList)
                {
                    item.ObjectDataLabels.Add(gdbObject.FNVHashes[fnvhash]);
                }


                if (gdbObject.ObjectLabels.ContainsKey(item.ObjectHash))
                {
                    if (gdbObject.FNVHashes.ContainsKey(gdbObject.ObjectLabels[item.ObjectHash]))
                    {
                        item.ObjectLabel = gdbObject.FNVHashes[gdbObject.ObjectLabels[item.ObjectHash]];
                    }
                    else
                    {
                        item.ObjectLabel = item.ObjectHash.ToString("X8") + " not in fnv dict";
                    }
                }
                else
                {
                    item.ObjectLabel = item.ObjectHash.ToString("X8") + " not in object dict";
                }


                //Keep a list of items by hash
                ItemList[item.ObjectHash] = item;
                
                //Keep a sorted list of items by unknown
                if (FolderList.ContainsKey(item.ObjectFolder))
                {
                    FolderList[item.ObjectFolder].Add(item);
                }
                else
                {
                    FolderList.Add(item.ObjectFolder, new List<GDBObjectTreeItem> { item });
                }
            }

            Folders = new List<TreeGDBRegion>();
            foreach (var folder in FolderList.Keys)
            {
                var root = new TreeGDBRegion() { Name = folder.ToString("X4") };
                foreach (var parent in FolderList[folder])
                {
                    root.TreeGDBObject.Add(TreeGDBObject(parent));
                }
                Folders.Add(root);
            }
        }

        static public TreeGDBObject TreeGDBObject(GDBObjectTreeItem item)
        {
            var node = new TreeGDBObject() { Name = item.ObjectLabel };
            for (int i = 0; i < item.ObjectData.TemplateByteData.Count(); i++)
            {
                var child = new TreeGDBObjectData() { Name = item.ObjectDataLabels[i] };
                var datatype = item.ObjectDataTemplate.ObjectDatatype[(UInt16)i];
                switch (datatype)
                {
                    case 0x0000:
                        child.Data = Convert.ToBoolean(BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0)).ToString();  //boolean
                        break;
                    case 0x0100:
                        child.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0).ToString("X8"); //= dword
                        break;
                    case 0x0200:
                        child.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0).ToString("X8"); //= dword lots of GroupIndex
                        break;
                    case 0x0300:
                        child.Data = BitConverter.ToSingle(item.ObjectData.TemplateByteData[i], 0).ToString();
                        break;
                    case 0x0400:
                        if (item.gdbObject.FNVHashes.ContainsKey(BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0)))
                        {
                            child.Data = item.gdbObject.FNVHashes[BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0)]; //= string hash
                        }
                        else
                        {
                            child.Data = BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0).ToString("X8") + " not in fnv dict";
                        }
                        break;
                    case 0x0500:
                        child.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0).ToString("X8"); //= numeric indicating an enumerated type
                        break;
                    case 0x0600:
                        if (ItemList.ContainsKey(BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0)))
                        {
                            child.Data = ItemList[BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0)];
                        }
                        else
                        {
                            child.Data = BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0).ToString("X8") + " not in object dict";
                        }
                        break;
                    case 0x0700:
                        if (ItemList.ContainsKey(BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0)))
                        {
                            child.Data = ItemList[BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0)];
                        }
                        else
                        {
                            child.Data = BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0).ToString("X8") + " not in object dict";
                        }
                        break;
                    default:
                        child.Data = datatype.ToString("X4") + " - " + BitConverter.ToUInt32(item.ObjectData.TemplateByteData[i], 0).ToString("X8");
                        break;
                }
                node.TreeGDBObjectData.Add(child);
            }
            return node;
        }
    }
}
