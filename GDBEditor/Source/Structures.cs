﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{
    public class TemplateData
    {
        public uint             OffsetToTemplate { get; set; }
        public List<byte[]>     TemplateByteData    { get; set; }
    }

    public class Template
    {
        public Byte     NoComponents    { get; set; }   //Boolean I think
        public Byte     Count1          { get; set; }
        public UInt16   Count2          { get; set; }   //This should be little_endian!! WTF,seems to be used for animation states?

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

    public class ObjectToLabel
    {
        public uint Object { get; set; }
        public uint Label { get; set; }
    }

    public class HashBlock
    {
        public uint Header          { get; set; }   // = 00 01 00 00 Always
        public uint TableSize   { get; set; }
        public uint Count       { get; set; }
        //public Dictionary<uint, string> fnvhashes { get; set; }   //[HashCount];
        public List<uint> Offsets   { get; set; }   //[HashCount];  //Offsets back into StringArray
    }
}
