using System;
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

                // Delete file index - Needed for DD:DA?
                if (File.Exists(output + ".log"))
                    File.Delete(output + ".log");

                // Process remaining header
                int count = br_input.ReadInt16();

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

                    // Move to next entry block
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

                // Check for file index - Needed for DD:DA?
                if (!File.Exists(input + ".log"))
                {
                    Console.WriteLine("ERROR: .arc file index is required but does not exist.");
                    return;
                }

                // Read info from index
                // TBC

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
