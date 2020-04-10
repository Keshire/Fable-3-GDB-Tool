using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace GDBEditor
{
    public class GDBFileExport
    {
        public void SaveFile(string filename, GDBFileImport file)
        {
            (new FileInfo(filename)).Directory.Create();
            using (BinaryWriter b = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {

                //Get sizes before writing
                uint recsize = 0;
                foreach (var record in file.RecordDict)
                {
                    recsize += 4;
                    foreach (var data in record.Value.rowdata.RowDataBytes) { recsize += (uint)data.Length; }
                }

                uint rowsize = 0;
                foreach (var type in file.RecordTypeDict)
                {
                    rowsize += 4;
                    foreach (var fnv in type.Value.Column_FNV) { rowsize += 4; }
                    foreach (var data in type.Value.Data_Type) { rowsize += 4; } //This is sorted differently than the base file, will this break?
                }

                uint strsize = 0;
                foreach (var fnvTostr in file.FNVToString)
                {
                    strsize += 4;
                    strsize += (uint)fnvTostr.Value.ToCharArray().Length + 1;
                }


                //Write header
                b.Write(file.header.GDB_Tag);
                b.Write(file.Records.Count);
                b.Write(recsize);
                b.Write(rowsize);
                b.Write(file.RecordToFNV.Count);
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
                    foreach (var data in type.Value.Data_Type) { b.Write(type.Value.Sort_Order[data.Key]); b.Write(data.Value); } //Not sure how dataid is sorted, so let's just grab the original order for now.
                }

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
                b.Write(strsize);
                b.Write(file.FNVToString.Count);

                var string_offsets = new List<uint>();
                var start_offset = (uint)b.BaseStream.Position;
                foreach (var fnvTostr in file.FNVToString)
                {
                    var local_offset = (uint)b.BaseStream.Position - start_offset;
                    string_offsets.Add(local_offset);

                    b.Write(fnvTostr.Key);
                    b.Write(fnvTostr.Value.ToCharArray());
                    b.Write(new Byte[] { 0x00 });
                }
                //foreach (var offset in file.StringData.Offsets) { b.Write(offset); }
                //There's some type of sorting going on here. Not sure what it is though. First come first serve maybe?
                foreach (var offset in string_offsets) { b.Write(offset); }
            }
        }
    }
}
