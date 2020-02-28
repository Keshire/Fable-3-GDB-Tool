using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBEditor
{
    class DataTypes
    {
        //0000 = boolean
        //0100 = dword
        //0200 = dword lots of GroupIndex
        //0300 = float
        //0400 = string hash
        //0500 = numeric indicating an enumerated type
        //0600 = object hash
        //0700 = object hash

        public bool data_type_0000 { get; set; }
        public UInt32 data_type_0100 { get; set; }
        public UInt32 data_type_0200 { get; set; }
        public float data_type_0300 { get; set; }
        public UInt32 data_type_0400 { get; set; }
        public UInt32 data_type_0500 { get; set; }
        public UInt32 data_type_0600 { get; set; }
        public UInt32 data_type_0700 { get; set; }

    }
}
