using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace GDBEditor
{
    [Serializable]
    public class GDBFileImport
    {
        public GDBHeader header { get; set; }
        public List<Record> Records = new List<Record>();
        public Dictionary<UInt32, UInt32> RecordToFNV = new Dictionary<UInt32, UInt32>(); //key=hash, values=fnv
        public Dictionary<UInt32, string> FNVToString = new Dictionary<UInt32, string>(); //key=fnv, value=string
        
        public HashBlock StringData { get; set; }

        public SortedDictionary<UInt32, RowType> RecordTypeDict = new SortedDictionary<uint, RowType>();//key=offset, values=rowtype
        public Dictionary<UInt32, Record> RecordDict = new Dictionary<uint, Record>(); //For lookup when editing treeview

        public GDBFileImport(BinaryReader buffer)
        {
            //Get header info 0x18
            header = new GDBHeader
            {
                GDB_Tag = buffer.ReadChars(4),//0x0000
                RecordCount = buffer.ReadUInt32(),
                RecordBlockSize = buffer.ReadUInt32(),
                RowTypeSize = buffer.ReadUInt32(),
                UniqueRecordCount = buffer.ReadUInt32(),
                Padding = buffer.ReadUInt32() //0x0000 End of header
            };

            //records are just raw data that need a rowtype to tell you what it is.
            //Thankfully the record contains an offset to the rowtype.
            for (int i = 0; i < header.RecordCount; i++)
            {
                RowData d = new RowData { RowTypeOffset = buffer.ReadUInt32() };
                Int64 RowDataPos = buffer.BaseStream.Position; //We're going to come back here after jumping to the template


                //Jump to rowtype, The rowtype basically describes what the data is in the record
                Int64 RowTypePos = (Int64)(0x18 + header.RecordBlockSize + d.RowTypeOffset);
                buffer.BaseStream.Position = RowTypePos;

                RowType t = new RowType 
                {
                    Components = buffer.ReadByte(),
                    Columns = buffer.ReadByte(),
                    Count2 = buffer.ReadUInt16() //This is the wrong endian I think...
                };
                
                int totalColumns = t.Columns + (t.Count2 * 256);

                //Data Labels
                t.Column_FNV = new List<UInt32>();
                for (int j = 0; j < totalColumns; j++)
                {
                    t.Column_FNV.Add(buffer.ReadUInt32());
                }

                //Data Types
                t.Data_Type = new Dictionary<UInt16, UInt16>();
                for (int j = 0; j < totalColumns; j++)
                {
                    UInt16 DataID = buffer.ReadUInt16(); //This doesn't make any sense... Not in order
                    UInt16 Datatype = buffer.ReadUInt16();
                    t.Data_Type[(UInt16)j] = Datatype; //I assume DataID is supposed to be a key, but it might be a lookup into something else??
                }

                //Jump back to the data now that we know what it is
                buffer.BaseStream.Position = RowDataPos;
                d.RowDataBytes = new List<Byte[]>();
                for (int j = 0; j < totalColumns; j++)
                {
                    d.RowDataBytes.Add(buffer.ReadBytes(4));
                }
                
                //Keeping types just in case...
                RecordTypeDict[d.RowTypeOffset] = t;
                Records.Add(new Record { rowtype = t, rowdata = d });
            }

            //Jump past templates because we already have them all. We'll need to determine its size later in order to rebuild the .gdb though
            buffer.BaseStream.Position = 0x18 + header.RecordBlockSize + header.RowTypeSize;

            //Should be hash
            foreach (Record r in Records)
            {
                r.hash = buffer.ReadUInt32();
            }

            //We don't know what these are, but I suspect it has something to do with the game regions or region layers, 
            //ie Bowerstone_Market (check the .save xml files)
            foreach (Record r in Records) 
            {
                r.partition = buffer.ReadUInt16();
            }

            //may be padded? I don't think it uses a standard block size...
            if (header.RecordCount % 2 > 0)
            {
                buffer.ReadBytes(2);
            }

            //END OF RECORD DATA
            //Build a hashed list for crossing with treeview
            foreach (Record r in Records) { RecordDict[r.hash] = r; }


            //Cross references record hash with label hash? Sometimes object doesn't exist, and sometimes string doesn't exist. 
            //Might've got pulled in from another.gdb or .save??
            RecordToFNV = new Dictionary<uint, uint>();
            for (int i = 0; i < header.UniqueRecordCount; i++)
            {
                uint fnv = buffer.ReadUInt32();
                uint row = buffer.ReadUInt32();
                RecordToFNV[row] = fnv;
            }
            
            //A block of fnv strings consisting of the fnv and string (null terminated).
            StringData = new HashBlock
            {
                Header = buffer.ReadUInt32(),             // = 00 01 00 00 Always
                TableSize = buffer.ReadUInt32(),
                Count = buffer.ReadUInt32()
            };

            for (int i = 0; i < StringData.Count; i++)
            {
                uint fnv = buffer.ReadUInt32();

                var stringbytes = new List<char>();
                while (buffer.PeekChar() != 0x00)
                {
                    stringbytes.Add(buffer.ReadChar());
                }
                buffer.ReadChar(); //null string termination not handled by ReadString()
                FNVToString[fnv] = new string(stringbytes.ToArray());
            }
            
            StringData.Offsets = new List<uint>();         //Offsets back into StringArray, not sure what these are used for...
            for (int i = 0; i < StringData.Count; i++)
            {
                StringData.Offsets.Add(buffer.ReadUInt32());
            }
        }
    }
}
