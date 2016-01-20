using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DDDApck
{
    class Program
    {
        static void Main(string[] args)
        {
            // Print header
            Console.WriteLine("DDDApck by MHVuze\n");

            // Assign argument
            string input = args[0];

            // Check if file or folder
            bool pack = false;
            FileAttributes attribute = File.GetAttributes(input);
            if (attribute.HasFlag(FileAttributes.Directory))
                pack = true;

            // Unpack file
            if (pack == false)
            {
                // Read file to memory
                MemoryStream ms_input = new MemoryStream(File.ReadAllBytes(input));
                BinaryReader br_input = new BinaryReader(ms_input);

                // Check file magic
                int magic = br_input.ReadInt32();
                if (magic != 0x58284039)
                {
                    Console.WriteLine("ERROR: File is not a DD:DA (PC) .pck file.");
                    return;
                }

                // Read file count
                int count = br_input.ReadInt32();

                // Delete file index
                if (File.Exists(Path.GetFileNameWithoutExtension(input) + ".log"))
                    File.Delete(Path.GetFileNameWithoutExtension(input) + ".log");

                // Process entries
                for (int i = 0; i < count; i++)
                {
                    int unk = br_input.ReadInt32();
                    int offset = br_input.ReadInt32();
                    int size = br_input.ReadInt32();

                    // Print to console and log
                    Console.WriteLine("0x" + unk.ToString("X8") + ", 0x" + offset.ToString("X8") + ", 0x" + size.ToString("X8"));
                    using (StreamWriter log = new StreamWriter(Path.GetFileNameWithoutExtension(input) + ".log", true, Encoding.UTF8))
                    {
                        log.WriteLine(unk.ToString("X8") + "," + offset.ToString("X8") + "," + size.ToString("X8"));
                    }

                    // Read file to array
                    br_input.BaseStream.Seek(offset, SeekOrigin.Begin);
                    byte[] file_data = br_input.ReadBytes(size);

                    // Extract file
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFileNameWithoutExtension(input) + "\\" + unk.ToString("X8") + ".ext"));

                    using (Stream extract = File.Create(Path.GetFileNameWithoutExtension(input) + "\\" + unk.ToString("X8") + ".ext"))
                    {
                        extract.Write(file_data, 0, size);
                    }

                    // Move to next entry block
                    br_input.BaseStream.Seek(0x08 + (i + 1) * 0x0C, SeekOrigin.Begin);
                }
                Console.WriteLine("\nINFO: .pck files do NOT give details on file name or type/extension.");
                Console.WriteLine("Check the files in a Hex Editor for example. Done.");
            }
            else if (pack == true)
            {
                // Check for file index
                if (!File.Exists(input + ".log"))
                {
                    Console.WriteLine("ERROR: .pck file index for input directory is required but does not exist.");
                    return;
                }

                // Read entry info
                Int32 count = File.ReadLines(input + ".log").Count();
                int first_offset = count * 0x0C + 8;
                List<string> list_unk = new List<string>();

                StreamReader log = new StreamReader(input + ".log", Encoding.UTF8, false);
                while (!log.EndOfStream)
                {
                    string line = log.ReadLine();
                    string[] columns = line.Split(',');
                    list_unk.Add(columns[0]);
                }

                // Write header
                File.WriteAllBytes(input + "_out.pck", new byte[first_offset]);
                BinaryWriter bw_output = new BinaryWriter(File.Open(input + "_out.pck", FileMode.Open));
                bw_output.Write(0x58284039);
                bw_output.Write(count);

                // Process entries
                int offset = first_offset;
                for (int i = 0; i < count; i++)
                {
                    // Print to console
                    Console.WriteLine("Processing " + list_unk[i] + ".ext");

                    // Read file to array
                    byte[] file_data = File.ReadAllBytes(input + "\\" + list_unk[i] + ".ext");

                    // Write entry info
                    bw_output.BaseStream.Seek(i * 0x0C + 0x08, SeekOrigin.Begin);
                    bw_output.Write(Convert.ToInt32(list_unk[i], 16));
                    bw_output.Write(offset);
                    bw_output.Write(file_data.Length);

                    // Write file
                    bw_output.BaseStream.Seek(offset, SeekOrigin.Begin);
                    bw_output.Write(file_data);

                    // Add to offset
                    offset += file_data.Length;
                }
                Console.WriteLine("\nINFO: .pck successfully created.");
            }
        }
    }
}
