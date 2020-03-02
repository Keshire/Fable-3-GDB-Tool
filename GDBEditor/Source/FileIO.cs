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

        public void SaveFile(string filename, GDBFileImport file)
        {
            (new FileInfo(filename)).Directory.Create();
            using (BinaryWriter b = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                //Write header
                b.Write(file.header.GDB_Tag);
                b.Write(file.header.RecordCount);
                b.Write(file.header.RecordBlockSize);
                b.Write(file.header.RowTypeSize);
                b.Write(file.header.UniqueRecordCount);
                b.Write(file.header.Padding);

                //RecordBlockSize
                foreach (var record in file.RecordDict)
                {
                    b.Write(record.Value.rowdata.RowTypeOffset);
                    foreach (var data in record.Value.rowdata.RowDataBytes) { b.Write(data); }
                }

                //RowTypeSize
                foreach (var type in file.RecordTypeDict)
                {
                    b.Write(type.Value.Components);
                    b.Write(type.Value.Columns);
                    b.Write(type.Value.Count2);
                    foreach (var fnv in type.Value.Column_FNV) { b.Write(fnv); }
                    foreach (var data in type.Value.Data_Type) { b.Write(data.Key); b.Write(data.Value); } //This is sorted differently than the base file, will this break?
                }

                //Something isn't getting written correctly, because the objects aren't picking up strings that the original has.
                //then hashes
                foreach (var record in file.RecordDict) { b.Write(record.Value.hash); }
                //then partitions
                foreach (var record in file.RecordDict) { b.Write(record.Value.partition); }
                //may be padded? I don't think it uses a standard block size...
                if (file.header.RecordCount % 2 > 0) { b.Write(new Byte[] { 0x00, 0x00 }); }

                //UniqueRecordCount
                foreach (var recTofnv in file.RecordToFNV) { b.Write(recTofnv.Value); b.Write(recTofnv.Key); }

                //not sure how this is controlled
                b.Write(file.StringData.Header);
                b.Write(file.StringData.TableSize);
                b.Write(file.StringData.Count);
                foreach (var fnvTostr in file.FNVToString) { b.Write(fnvTostr.Key); b.Write(fnvTostr.Value.ToCharArray()); b.Write(new Byte[] { 0x00 }); }
                foreach (var offset in file.StringData.Offsets) { b.Write(offset); }
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
