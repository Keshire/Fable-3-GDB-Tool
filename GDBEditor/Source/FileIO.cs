using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GDBEditor
{
    //Helper Class for Import/Export functions
    public class FileIO
    {
        public void ExtractFile(byte[] newFile, string filename)
        {
            (new FileInfo(filename)).Directory.Create();
            using (BinaryWriter b = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                foreach (var i in newFile) { b.Write(i); }
            }
        }

        // Convert an object to a byte array
        public byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    //writer.Write(obj);
                }
                return ms.ToArray();
            }
        }
    }
}
