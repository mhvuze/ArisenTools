using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAtexlist
{
    class Program
    {
        static void Main(string[] args)
        {
            // Print header
            Console.WriteLine("DDDAtexlist by MHVuze\n");

            // Check arguments
            if (args.Length < 1)
            {
                Console.WriteLine("ERROR: Invalid amount of arguments.\n");
                Console.WriteLine("Use DDDAtexlist <input_folder> [flag]");
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
            StreamWriter csv = new StreamWriter("TexInfo.csv", false);
            csv.WriteLine("File,Mips,Width,Height,Type,Type ID");
            string file_info;

            // Recursively work through textures
            foreach (string texture in Directory.EnumerateFiles(folder, "*.tex", SearchOption.AllDirectories))
            {
                // Read header information
                // From https://raw.githubusercontent.com/FrozenFish24/TurnaboutTools/master/TEXporter/TEXporter/Program.cs
                byte[] array_input = File.ReadAllBytes(texture);
                Int32 magic = BitConverter.ToInt32(array_input, 0);

                uint[] header = new uint[3];
                for (int i = 0; i < 3; i++)
                    header[i] = BitConverter.ToUInt32(array_input, i * 4 + 4);

                int version = (int)(header[0] & 0xfff);         // First dword
                int alpha_flag = (int)((header[0] >> 12) & 0xfff);
                int shift = (int)((header[0] >> 24) & 0xf);
                int unk2 = (int)((header[0] >> 28) & 0xf);
                int mip_count = (int)(header[1] & 0x3f);        // Second dword
                int width = (int)((header[1] >> 6) & 0x1fff);
                int height = (int)((header[1] >> 19) & 0x1fff);
                int unk3 = (int)(header[2] & 0xff);             // Third dword
                int type = (int)((header[2] >> 8) & 0xff);
                int unk4 = (int)((header[2] >> 16) & 0x1fff);

                // Check file magic
                if (magic != 0x00584554 || version != 0x99)
                {
                    Console.WriteLine("ERROR: File is not a DD:DA (PC) .tex file.");
                    return;
                }

                // Assign type to string
                string string_type = "";
                if (type == 20)
                    string_type = "DXT1";
                else if (type == 24)
                    string_type = "DXT5";
                else if (type == 25)
                    string_type = "DXT1";
                else if (type == 31)
                    string_type = "DXT5";
                else if (type == 47)
                    string_type = "DXT5";
                else
                    string_type = type.ToString();
                    
                // Write info to csv
                string path = texture.Replace((folder + "\\"), "");

                if (flag == "-full")
                    path = texture;

                file_info =
                    path + "," +
                    mip_count + "," +
                    width + "," +
                    height + "," +
                    string_type + "," +
                    type;
                csv.WriteLine(file_info);

                Console.WriteLine("INFO: Successfully processed " + texture);
            }
            csv.Close();
        }
    }
}
