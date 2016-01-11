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
            if (File.Exists(input + ".log"))
                File.Delete(input + ".log");

            // Process entries
            for (int i = 0; i < count; i++)
            {
                int unk = br_input.ReadInt32();
                int offset = br_input.ReadInt32();
                int size = br_input.ReadInt32();

                // Print to console and log
                Console.WriteLine("0x" + unk.ToString("X8") + ", 0x" + offset.ToString("X8") + ", 0x" + size.ToString("X8"));
                using (StreamWriter log = new StreamWriter(input + ".log", true, Encoding.UTF8))
                {
                    log.WriteLine("0x" + unk.ToString("X8") + ", 0x" + offset.ToString("X8") + ", 0x" + size.ToString("X8"));
                }

                // Read file to array
                br_input.BaseStream.Seek(offset, SeekOrigin.Begin);
                byte[] file_data = br_input.ReadBytes(size);

                // Extract file
                Directory.CreateDirectory(Path.GetDirectoryName("out\\" + unk.ToString("X8") + ".gmd"));

                using (Stream extract = File.Create("out\\" + unk.ToString("X8") + ".gmd"))
                {
                    extract.Write(file_data, 0, size);
                }

                // Move to next entry block
                br_input.BaseStream.Seek(0x08 + (i + 1) * 0x0C, SeekOrigin.Begin);
            }
        }
    }
}
