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

        public static object ConvertToType(UInt16 datatype, byte[] data)
        {
            object d = null;
            switch (datatype)
            {
                case 0x0000:
                    d = Convert.ToBoolean(BitConverter.ToUInt32(data, 0));  //bool
                    break;
                case 0x0100:
                    d = BitConverter.ToUInt32(data, 0); //dword
                    break;
                case 0x0200:
                    d = BitConverter.ToUInt32(data, 0); //dword lots of GroupIndex
                    break;
                case 0x0300:
                    d = BitConverter.ToSingle(data, 0); //Float
                    break;
                case 0x0400:
                    d = BitConverter.ToUInt32(data, 0);
                    break;
                case 0x0500:
                    d = BitConverter.ToUInt32(data, 0); //enumerated type
                    break;
                case 0x0600:
                    d = BitConverter.ToUInt32(data, 0);
                    break;
                case 0x0700:
                    d = BitConverter.ToUInt32(data, 0);
                    break;
                default:
                    d = BitConverter.ToUInt32(data, 0);
                    break;
            }

            return d;
        }

        public static byte[] ConvertToData(UInt16 datatype, object data)
        {
            Byte[] d = new Byte[4];
            switch (datatype)
            {
                case 0x0000:
                    //d = Convert.ToBoolean(BitConverter.ToUInt32(data, 0));  //bool
                    d = BitConverter.GetBytes(bool.Parse((string)data)).Concat(new byte[] { 0x00}).Concat(new byte[] { 0x00 }).Concat(new byte[] { 0x00 }).ToArray();
                    break;
                case 0x0100:
                    //d = BitConverter.ToUInt32(data, 0); //dword
                    d = BitConverter.GetBytes(UInt32.Parse((string)data));
                    break;
                case 0x0200:
                    //d = BitConverter.ToUInt32(data, 0); //dword lots of GroupIndex
                    d = BitConverter.GetBytes(UInt32.Parse((string)data));
                    break;
                case 0x0300:
                    //d = BitConverter.ToSingle(data, 0); //Float
                    d = BitConverter.GetBytes(float.Parse((string)data));
                    break;
                case 0x0400:
                    //d = BitConverter.ToUInt32(data, 0);
                    d = BitConverter.GetBytes(UInt32.Parse((string)data));
                    break;
                case 0x0500:
                    //d = BitConverter.ToUInt32(data, 0); //enumerated type
                    d = BitConverter.GetBytes(UInt32.Parse((string)data));
                    break;
                case 0x0600:
                    //d = BitConverter.ToUInt32(data, 0);
                    d = BitConverter.GetBytes(UInt32.Parse((string)data));
                    break;
                case 0x0700:
                    //d = BitConverter.ToUInt32(data, 0);
                    d = BitConverter.GetBytes(UInt32.Parse((string)data));
                    break;
                default:
                    //d = BitConverter.ToUInt32(data, 0);
                    d = BitConverter.GetBytes(UInt32.Parse((string)data));
                    break;
            }
            return d;
        }
    }
}
