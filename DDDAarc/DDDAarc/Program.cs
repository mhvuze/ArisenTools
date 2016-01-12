using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DDDAarc
{
    class Program
    {
        static void Main(string[] args)
        {
            // Print header
            Console.WriteLine("DDDAarc by MHVuze\nUses zlib.NET\n");

            // Check arguments
            if (args.Length != 3)
            {
                Console.WriteLine("ERROR: Invalid amount of arguments.\n");
                Console.WriteLine("Use DDDAarc <mode> <input> <output>");
                Console.WriteLine("Available modes: -x for extract, -p for pack");
                return;
            }

            // Assign arguments
            string mode = args[0];
            string input = args[1];
            string output = args[2];

            // Handle extracting
            if (mode == "-x")
            {
                // Check input
                if (!File.Exists(input))
                {
                    Console.WriteLine("ERROR: Input file does not exist.");
                    return;
                }

                // Read file to memory
                MemoryStream ms_input = new MemoryStream(File.ReadAllBytes(input));
                BinaryReader br_input = new BinaryReader(ms_input);

                // Check file magic
                int magic = br_input.ReadInt32();
                int version = br_input.ReadInt16();
                if (magic != 0x00435241 || version != 0x07)
                {
                    Console.WriteLine("ERROR: File is not a DD:DA (PC) .arc file.");
                    return;
                }

                // Delete file index
                if (File.Exists(output + ".log"))
                    File.Delete(output + ".log");

                // Process remaining header
                int count = br_input.ReadInt16();

                // Process first file offset for faithful rebuilding
                br_input.BaseStream.Seek(0x54, SeekOrigin.Begin);
                int first_offset = br_input.ReadInt32();
                br_input.BaseStream.Seek(0x08, SeekOrigin.Begin);

                using (StreamWriter log = new StreamWriter(output + ".log", true, Encoding.UTF8))
                {
                    log.WriteLine(first_offset);
                }

                // Process entries
                Console.WriteLine("INFO: " + Path.GetFileName(input) + ", ARC version " + version + ", file count " + count + "\n");

                for (int i = 0; i < count; i++)
                {
                    // Process entry block
                    byte[] name = br_input.ReadBytes(0x40).Where(b => b != 0x00).ToArray();
                    int extension = br_input.ReadInt32();
                    int comp_size = br_input.ReadInt32();
                    byte[] full_size_array = br_input.ReadBytes(3);
                    byte constant = br_input.ReadByte();
                    int offset = br_input.ReadInt32();

                    // Process entry info
                    string string_name = System.Text.Encoding.UTF8.GetString(name);
                    ExtensionHandler.IntToString(extension);

                    byte[] full_size_array2 = new byte[4];
                    full_size_array.CopyTo(full_size_array2, 0);
                    int full_size = BitConverter.ToInt32(full_size_array2, 0);

                    // Read compressed data to array
                    br_input.BaseStream.Seek(offset, SeekOrigin.Begin);
                    byte[] comp_data = br_input.ReadBytes(comp_size);
                    int comp_flag = Helper.ReverseBytesInt16(BitConverter.ToInt16(comp_data, 0));

                    // Print to console and log
                    Console.WriteLine("Processing " + string_name + "." + ExtensionHandler.string_extension);
                    using (StreamWriter log = new StreamWriter(output + ".log", true, Encoding.UTF8))
                    {
                        log.WriteLine(comp_flag.ToString("X4") + "," + string_name + "," + ExtensionHandler.string_extension + "," + constant);
                    }

                    // Extract file
                    MemoryStream comp_data_stream = new MemoryStream(comp_data);
                    byte[] full_data = new byte[full_size];
                    Directory.CreateDirectory(Path.GetDirectoryName(output + "\\" + string_name + "." + ExtensionHandler.string_extension));

                    using (Stream extract = File.Create(output + "\\" + string_name + "." + ExtensionHandler.string_extension))
                    {
                        Helper.DecompressData(comp_data, out full_data);
                        extract.Write(full_data, 0, full_size);
                    }

                    // Seek to next entry info block
                    br_input.BaseStream.Seek(0x08 + (i + 1) * 0x50, SeekOrigin.Begin);
                }
                Console.WriteLine("\nINFO: .arc successfully extracted.");
            }

            // Handle packing
            else if (mode == "-p")
            {
                // Check input
                if (!Directory.Exists(input))
                {
                    Console.WriteLine("ERROR: Input directory does not exist.");
                    return;
                }

                // Check for file index
                if (!File.Exists(input + ".log"))
                {
                    Console.WriteLine("ERROR: .arc file index for input directory is required but does not exist.");
                    return;
                }

                // Read basic info from file index
                StreamReader log = new StreamReader(input + ".log", Encoding.UTF8, false);
                int offset = Convert.ToInt32(log.ReadLine());
                int count = Convert.ToInt16(File.ReadLines(input + ".log").Count() - 1);
                Int16 version = 7;

                // Read entry info from file index
                List<string> list_comp_flag = new List<string>();
                List<string> list_name = new List<string>();
                List<string> list_extension = new List<string>();
                List<string> list_constant = new List<string>();

                while (!log.EndOfStream)
                {
                    string line = log.ReadLine();
                    string[] columns = line.Split(',');

                    list_comp_flag.Add(columns[0]);
                    list_name.Add(columns[1]);
                    list_extension.Add(columns[2]);
                    list_constant.Add(columns[3]);
                }

                // Write header
                System.IO.File.WriteAllBytes(output, new byte[offset]);
                BinaryWriter bw_output = new BinaryWriter(File.Open(output, FileMode.Open));
                bw_output.Write(0x00435241);
                bw_output.Write(version);
                bw_output.Write(count);

                // Process entries
                for (int i = 0; i < count; i++)
                {
                    // Print to console
                    Console.WriteLine("Processing " + list_name[i] + "." + list_extension[i]);

                    // Seek to target entry info offset
                    bw_output.BaseStream.Seek(i * 0x50 + 0x08, SeekOrigin.Begin);

                    // Copy file name to array
                    byte[] array_block_name = new byte[0x40];
                    byte[] array_name = Encoding.UTF8.GetBytes(list_name[i]);
                    Buffer.BlockCopy(array_name, 0, array_block_name, 0, array_name.Length);

                    // Compress file
                    byte[] comp_data = new byte[0];
                    byte[] full_data = File.ReadAllBytes(input + "\\" + list_name[i] + "." + list_extension[i]);
                    Helper.CompressData(list_comp_flag[i], full_data, out comp_data);

                    // Write entry info to file
                    bw_output.Write(array_block_name);
                    bw_output.Write(ExtensionHandler.StringToInt(list_extension[i]));
                    bw_output.Write(comp_data.Length);
                    bw_output.Write(full_data.Length);
                    bw_output.BaseStream.Seek(-1, SeekOrigin.Current);
                    bw_output.Write(Convert.ToByte(list_constant[i]));
                    bw_output.Write(offset);

                    // Write compressed data to file
                    bw_output.BaseStream.Seek(offset, SeekOrigin.Begin);
                    bw_output.Write(comp_data);

                    // Calculate next offset
                    offset = offset + comp_data.Length;
                }
                Console.WriteLine("\nINFO: .arc successfully packed.");
            }

            // Handle invalid mode
            else
            {
                Console.WriteLine("ERROR: Invalid mode specified.");
                Console.WriteLine("Available modes: -x for extract, -p for pack");
                return;
            }
        }
    }
}
