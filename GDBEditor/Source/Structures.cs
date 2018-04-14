using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{

    //File Structures gleamed from staring at a hex editor for too long.
    public class GDB
    {

    }


    public class TData
    {
        public uint         OffsetToTemplate { get; set; }
        public List<uint>   TemplateData { get; set; }
    }

    public class Template
    {
        public Byte     NoComponents    { get; set; }   //Boolean I think
        public Byte     Count1          { get; set; }
        public UInt16   Count2          { get; set; }   //This should be little_endian!! WTF,seems to be used for animation states

        public List<uint>                   ObjectHashList { get; set; }  //[(Count2 * 256) + Count1];    //(Count2*256) fucking endian...
        public Dictionary<UInt16, UInt16>   ObjectDatatype { get; set; }  //[(Count2 * 256) + Count1];    //This looks to be controlling datatypes used.
        //0000 = boolean
        //0100 = dword
        //0200 = dword lots of GroupIndex
        //0300 = float
        //0400 = string hash
        //0500 = numeric indicating an enumerated type
        //0600 = object hash
        //0700 = object hash
    }

    public class HashStruct
    {
        public uint Hash1 { get; set; }
        public uint Hash2 { get; set; }
    }

    public class HashBlock
    {
        public uint Header          { get; set; }   // = 00 01 00 00 Always
        public uint HashTableSize   { get; set; }
        public uint HashCount       { get; set; }
        //public Dictionary<uint, string> fnvhashes { get; set; }   //[HashCount];
        public List<uint>       HashPointerArray    { get; set; }   //[HashCount];  //Offsets back into StringArray
    }
}
