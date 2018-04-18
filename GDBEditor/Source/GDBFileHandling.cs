using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace GDBEditor
{
    public class GDBFileHandling
    {
        //Keep some dictionaries for each object, We'll probably want to combine them if we load more than one file
        public Dictionary<uint, Template> TemplateDictionary = new Dictionary<uint, Template>(); //key=offset, value=template, We'll need this to rebuild later
        public Dictionary<uint, uint> ObjectLabels = new Dictionary<uint, uint>(); //key=ObjectHash, values=fnvhash
        public Dictionary<uint, string> FNVHashes = new Dictionary<uint, string>(); //key=fnvhash, value=string


        // = Fable 2 used "GDB\0x00", Fable 3 is "0x00000000"
        public char[] GDB_Tag = new char[4];
        public uint ObjectCount { get; set; }
        public uint ObjectSize { get; set; }
        public uint TemplateSize { get; set; }
        public uint UniqueObjectCount { get; set; }
        //End of header

        //[TDCount]
        public List<TemplateData> TemplateData = new List<TemplateData>();
        public List<uint> ObjectHash = new List<uint>();
        public List<UInt16> Unknown_UInt16 = new List<UInt16>(); //Are these folders or regions or something?

        //Header for the strings
        public HashBlock StringData { get; set; }

        
        public GDBFileHandling(BinaryReader buffer)
        {
            //Get header info
            GDB_Tag = buffer.ReadChars(4);//0x0000
            ObjectCount = buffer.ReadUInt32();
            ObjectSize = buffer.ReadUInt32();
            TemplateSize = buffer.ReadUInt32();
            UniqueObjectCount = buffer.ReadUInt32();
            buffer.ReadUInt32(); //0x0000 End of header 0x18

            //game objects are just raw data that need a template to tell you what it is.
            //Thankfully the object contains an offset to the template.
            for (int i = 0; i < ObjectCount; i++)
            {
                var td = new TemplateData();
                var t = new Template();
                td.OffsetToTemplate = buffer.ReadUInt32();

                var originalpos = buffer.BaseStream.Position; //We're going to come back here after jumping to the template
                var offset = 0x18 + ObjectSize + td.OffsetToTemplate; //This is the offset to this template (templates are shared)

                //Jump to template, The template basically describes what the data is in the base object
                buffer.BaseStream.Position = offset;
                t.NoComponents = buffer.ReadByte();
                t.Count1 = buffer.ReadByte();
                t.Count2 = buffer.ReadUInt16(); //This is the wrong endian I think...
                var items = t.Count1 + (t.Count2 * 256);

                //Data Labels
                t.ObjectHashList = new List<uint>();
                for (int j = 0; j < items; j++)
                {
                    t.ObjectHashList.Add(buffer.ReadUInt32());
                }

                //Data Types
                t.ObjectDatatype = new Dictionary<UInt16, UInt16>();
                for (int k = 0; k < items; k++)
                {
                    var ObjectID = buffer.ReadUInt16(); //This isn't sorted right!!
                    var Datatype = buffer.ReadUInt16();
                    t.ObjectDatatype[(UInt16)k] = Datatype; //ObjectID should have been the key...
                }

                //Jump back to the data now that we know what it is
                buffer.BaseStream.Position = originalpos;
                td.TemplateByteData = new List<Byte[]>();
                for (int v = 0; v < items; v++)
                {
                    td.TemplateByteData.Add(buffer.ReadBytes(4));
                }


                TemplateDictionary[td.OffsetToTemplate] = t;
                TemplateData.Add(td);
            }

            //Jump past templates because we already have them all. We'll need to determine its size later in order to rebuild the .gdb though
            buffer.BaseStream.Position = 0x18 + ObjectSize + TemplateSize;

            //Should be hashed object
            ObjectHash = new List<uint>();
            for (int i = 0; i < ObjectCount; i++)
            {
                ObjectHash.Add(buffer.ReadUInt32());
            }

            //We don't know what these are, but I suspect it has something to do with the game regions or region layers, ie Bowerstone_Market (check the .save xml files)
            Unknown_UInt16 = new List<UInt16>();
            for (int i = 0; i < ObjectCount; i++)
            {
                Unknown_UInt16.Add(buffer.ReadUInt16()); //This is different from sven's gdb dumper...
            }

            //may be padded?
            if (ObjectCount % 2 > 0)
            {
                buffer.ReadBytes(2);
            }

            //Cross references object hashes with label hashes? Sometimes object doesn't exist, and sometimes string doesn't exist. Might've got pulled in from another.gdb or .save??
            ObjectLabels = new Dictionary<uint, uint>();
            for (int i = 0; i < UniqueObjectCount; i++)
            {
                var Label = buffer.ReadUInt32();
                var Object = buffer.ReadUInt32();
                ObjectLabels[Object] = Label;
            }
            
            //A block of fnv hashed strings consisting of the hash and it's string (null terminated).
            StringData = new HashBlock
            {
                Header = buffer.ReadUInt32(),             // = 00 01 00 00 Always
                TableSize = buffer.ReadUInt32(),
                Count = buffer.ReadUInt32()
            };

            for (int i = 0; i < StringData.Count; i++)
            {
                var hash = buffer.ReadUInt32();

                var stringbytes = new List<char>();
                while (buffer.PeekChar() != 0x00)
                {
                    stringbytes.Add(buffer.ReadChar());
                }
                buffer.ReadChar(); //null string termination not handled by ReadString()
                FNVHashes[hash] = new string(stringbytes.ToArray());
            }
            
            StringData.Offsets = new List<uint>();         //Offsets back into StringArray, not sure what these are used for...
            for (int i = 0; i < StringData.Count; i++)
            {
                StringData.Offsets.Add(buffer.ReadUInt32());
            }

        }

        //Just in case I need to convert a string back to hash
        public static uint FnvHash(string str)
        {
            long num = -2128831035;
            for (int i = 0; i <= str.Length - 1; i++)
            {
                num = (num * 16777619 & -1);
                num ^= (65535 & Convert.ToUInt32(str[i]));
            }
            return (uint)num;
        }
    }
}
