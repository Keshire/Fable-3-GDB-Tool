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
        // = Fable 2 used "GDB\0x00", Fable 3 is "0x00000000"
        public char[] GDB_Tag = new char[4];
        public uint TDCount { get; set; }
        public uint TDStart { get; set; }
        public uint TDSize { get; set; }
        public uint Count1 { get; set; }
        //End of header


        public List<TData> TemplateData { get; set; }   //[TDCount]
        public List<uint> HashIndex { get; set; }       //[TDCount];
        public List<UInt16> Unknown { get; set; }       //[TDCount];    //Offset or 16bit Hashes or...??
        public List<HashStruct> Hashes { get; set; }    //[Count1];
        public HashBlock StringData { get; set; }


        public Dictionary<uint, Template> TemplateDictionary = new Dictionary<uint, Template>(); //We'll need this to rebuild later
        public Dictionary<uint, string> fnvhashes = new Dictionary<uint, string>(); //Keep the hashes handy

        public GDBFileHandling(BinaryReader buffer)
        {
            //Get header info
            GDB_Tag = buffer.ReadChars(4);//0x0000
            TDCount = buffer.ReadUInt32();
            TDStart = buffer.ReadUInt32();
            TDSize = buffer.ReadUInt32();
            Count1 = buffer.ReadUInt32();
            buffer.ReadUInt32(); //0x0000 End of header 0x18

            //game objects are just raw data that need a template to tell you what it is.
            //Thankfully the object contains an offset to the template.
            TemplateData = new List<TData>();
            for (int i = 0; i < TDCount; i++)
            {
                var tdata = new TData();
                var template = new Template();
                tdata.OffsetToTemplate = buffer.ReadUInt32();

                var originalpos = buffer.BaseStream.Position; //We're going to come back here after jumping to the template
                var offset = 0x18 + TDStart + tdata.OffsetToTemplate; //This is the offset to this template (templates are shared)

                //Jump to template, The template basically describes what the data is in the base object
                buffer.BaseStream.Position = offset;
                template.NoComponents = buffer.ReadByte();
                template.Count1 = buffer.ReadByte();
                template.Count2 = buffer.ReadUInt16(); //This is the wrong endian I think...
                var items = template.Count1 + (template.Count2 * 256);

                //Data Labels
                template.ObjectHashList = new List<uint>();
                for (int j = 0; j < items; j++)
                {
                    template.ObjectHashList.Add(buffer.ReadUInt32());
                }

                //Data Types
                template.ObjectDatatype = new Dictionary<ushort, ushort>();
                for (int k = 0; k < items; k++)
                {
                    var ObjectID = buffer.ReadUInt16();
                    var Datatype = buffer.ReadUInt16();
                    template.ObjectDatatype[ObjectID] = Datatype;
                }

                //Jump back to the data now that we know what it is
                buffer.BaseStream.Position = originalpos;
                tdata.TemplateData = new List<uint>();
                for (int v = 0; v < items; v++)
                {
                    tdata.TemplateData.Add(buffer.ReadUInt32());
                }


                TemplateDictionary[tdata.OffsetToTemplate] = template;
                TemplateData.Add(tdata);
            }

            //Jump past templates because we already have them all. We'll need to determine its size later in order to rebuild the .gdb though
            buffer.BaseStream.Position = 0x18 + TDStart + TDSize;

            //I think these are the base object labels, fnv hashed of course.
            HashIndex = new List<uint>();
            for (int i = 0; i < TDCount; i++)
            {
                HashIndex.Add(buffer.ReadUInt32());
            }

            //We don't know what these are, but I suspect it has something to do with the game regions or region layers, ie Bowerstone_Market (check the .save xml files)
            Unknown = new List<UInt16>();
            for (int i = 0; i < TDCount; i++)
            {
                Unknown.Add(buffer.ReadUInt16());
            }

            //Don't remember what these are for.
            Hashes = new List<HashStruct>();
            for (int i = 0; i < Count1; i++)
            {
                Hashes.Add(new HashStruct { Hash1 = buffer.ReadUInt32(), Hash2 = buffer.ReadUInt32() });
            }


            //A block of fnv hashed strings consisting of the hash and it's string (null terminated).
            StringData = new HashBlock
            {
                Header = buffer.ReadUInt32(),             // = 00 01 00 00 Always
                HashTableSize = buffer.ReadUInt32(),
                HashCount = buffer.ReadUInt32()
            };

            for (int i = 0; i < StringData.HashCount; i++)
            {
                var hash = buffer.ReadUInt32();

                var stringbytes = new List<char>();
                while (buffer.PeekChar() != 0x00)
                {
                    stringbytes.Add(buffer.ReadChar());
                }
                buffer.ReadChar(); //null termination
                fnvhashes[hash] = new string(stringbytes.ToArray());
            }
            
            StringData.HashPointerArray = new List<uint>();         //[HashCount];  //Offsets back into StringArray
            for (int i = 0; i < StringData.HashCount; i++)
            {
                StringData.HashPointerArray.Add(buffer.ReadUInt32());
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
