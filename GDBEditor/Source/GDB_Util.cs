using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GDBEditor
{
    public class GDB_Util
    {
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
