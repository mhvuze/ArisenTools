using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAarclist
{
    class Program
    {
        static void Main(string[] args)
        {
            // Print header
            Console.WriteLine("DDDAarclist by MHVuze\n");

            // Check arguments
            if (args.Length < 1)
            {
                Console.WriteLine("ERROR: Invalid amount of arguments.\n");
                Console.WriteLine("Use DDDAarclist <input_folder> [flag]");
                Console.WriteLine("Available flag: -full for full path name");
                return;
            }

            // Assign arguments
            string folder = args[0];
            string flag = "";
            if (args.Length > 1)
                flag = args[1];

            // Check for folder / file
            if (!Directory.Exists(folder))
            {
                Console.WriteLine("ERROR: Input folder does not exist.");
                return;
            }

            // Setup csv writer
            StreamWriter csv = new StreamWriter("ArcInfo.csv", false);
            csv.WriteLine("Archive,File,Type,CSize,FSize,Constant,Offset");
            string file_info;

            // Recursively work through archives
            foreach (string archive in Directory.EnumerateFiles(folder, "*.arc", SearchOption.AllDirectories))
            {
                // Read file to memory
                MemoryStream ms_input = new MemoryStream(File.ReadAllBytes(archive));
                BinaryReader br_input = new BinaryReader(ms_input);

                // Check file magic
                int magic = br_input.ReadInt32();
                int version = br_input.ReadInt16();
                if (magic != 0x00435241 || version != 0x07)
                {
                    Console.WriteLine("ERROR: File is not a DD:DA (PC) .arc file.");
                    return;
                }

                // Process remaining header
                int count = br_input.ReadInt16();

                // Process entries
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

                    // Write info to csv
                    string path = archive.Replace((folder + "\\"), "");

                    if (flag == "-full")
                        path = archive;

                    file_info =
                        path + "," +
                        string_name + "." + ExtensionHandler.string_extension + "," +
                        ExtensionHandler.string_extension + "," +
                        comp_size + "," +
                        full_size + "," +
                        constant + "," +
                        offset;
                    csv.WriteLine(file_info);

                    // Seek to next entry info block
                    br_input.BaseStream.Seek(0x08 + (i + 1) * 0x50, SeekOrigin.Begin);
                }
                Console.WriteLine("INFO: Successfully processed " + archive);
            }
            csv.Close();
        }
    }
}
