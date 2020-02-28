using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{
    [Serializable]
    public class RowData
    {
        public uint             RowTypeOffset { get; set; }
        public List<byte[]>     RowDataByteArray    { get; set; }
    }

    public class Record
    {
        public RowType rowtype { get; set; }
        public RowData rowdata { get; set; }

        public UInt32 hash { get; set; }

        public UInt16 partition { get; set; }
    }

    [Serializable]
    public class RowType
    {
        public Byte     Components      { get; set; }   //Boolean I think
        public Byte     Columns         { get; set; }
        public UInt16   Count2          { get; set; }   //This should be little_endian!! WTF,seems to be used for animation states?

        public List<uint>                   Column_FNV { get; set; }  //fnv hash list, should be column labels
        public Dictionary<UInt16, UInt16>   Data_Type { get; set; }  //[(Count2 * 256) + Count1];    //This looks to be controlling datatypes used.
        //0000 = boolean
        //0100 = dword
        //0200 = dword lots of GroupIndex
        //0300 = float
        //0400 = string hash
        //0500 = numeric indicating an enumerated type
        //0600 = object hash
        //0700 = object hash
    }

    [Serializable]
    public class HashBlock
    {
        public uint Header          { get; set; }   // = 00 01 00 00 Always
        public uint TableSize   { get; set; }
        public uint Count       { get; set; }
        public List<uint> Offsets   { get; set; }   //[HashCount];  //Offsets back into StringArray
    }

    [Serializable]
    public class GDBHeader
    {
        // = Fable 2 used "GDB\0x00", Fable 3 is "0x00000000"
        public char[] GDB_Tag = new char[4];
        public UInt32 RecordCount { get; set; }
        public UInt32 RecordBlockSize { get; set; }
        public UInt32 RowTypeSize { get; set; }
        public UInt32 UniqueRecordCount { get; set; }
        public UInt32 Padding { get; set; }
    }
}
