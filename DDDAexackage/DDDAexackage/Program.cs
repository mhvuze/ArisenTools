using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DDDAexackage
{
    class Program
    {
        static void Main(string[] args)
        {
            // Print header
            Console.WriteLine("DDDAexackage by MHVuze\n");

            // Assign argument
            string input = args[0];

            // Check file
            if (!File.Exists(input))
            {
                Console.WriteLine("ERROR: File does not exist.");
                return;
            }

            // Read file to memory
            MemoryStream ms_input = new MemoryStream(File.ReadAllBytes(input));
            BinaryReader br_input = new BinaryReader(ms_input);

            // Check file magic
            int magic = br_input.ReadInt32();
            if (magic != 0x00000000)
            {
                Console.WriteLine("ERROR: File is not a DD:DA (PS3) exackage.bin file.");
                return;
            }

            // Read file count
            int count = br_input.ReadInt32();

            // Process entries
            for (int i = 0; i < count; i++)
            {
                byte[] name = br_input.ReadBytes(0x20).Where(b => b != 0x00).ToArray();
                string string_name = System.Text.Encoding.UTF8.GetString(name);
                int offset = br_input.ReadInt32();
                int size = br_input.ReadInt32();

                // Read file data to array
                br_input.BaseStream.Seek(offset, SeekOrigin.Begin);
                byte[] file_data = br_input.ReadBytes(size);

                // Extract file
                Console.WriteLine("Processing " + string_name);
                //Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFileNameWithoutExtension(input) + "\\" + string_name));
                string test = Path.GetPathRoot(input);
                using (Stream extract = File.Create(Path.GetDirectoryName(input) + "\\" + string_name))
                {
                    extract.Write(file_data, 0, size);
                }

                // Seek to next entry info block
                br_input.BaseStream.Seek(0x08 + (i + 1) * 0x28, SeekOrigin.Begin);
            }
            File.Delete(input);
        }
    }
}
