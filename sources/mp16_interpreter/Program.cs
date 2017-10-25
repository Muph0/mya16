using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace mp16_interpreter
{
    class Program
    {
        public static bool TERMINAL_RESIZE_ENABLED = false;
        static Mobo emulation;
        static void Main(string[] args)
        {
            try
            {
                Console.BufferWidth = Console.BufferWidth;
                TERMINAL_RESIZE_ENABLED = true;
            }
            catch (Exception e)
            {
                TERMINAL_RESIZE_ENABLED = false;
            }

            emulation = new Mobo();
            new Thread(delegate() { emulation.readKeys(); }).Start();

            if (args.Length == 0)
                args = new string[] { @"C:\Users\Honky\Documents\Logisim\16-bit\mya16\os.bin16" };
            List<string> args_list = args.ToList();

            if (args.Length == 1)
            {
                string file = args[0];
                if (file.Contains(".mya16"))
                {
                    Console.WriteLine(file);
                    file = file.Replace(".mya16", ".bin16");                
                }
                if (File.Exists(file))
                {
                    string data_str = File.ReadAllText(file);
                    data_str = data_str.Replace((char)10, ' ');
                    data_str = data_str.Replace("v2.0 raw ", "");

                    string[] bytes = data_str.Split(' ');

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        emulation.memory[i] = (ushort)Convert.ToInt32(bytes[i], 16);
                    }

                    Console.BackgroundColor = ConsoleColor.Blue;
                    if (TERMINAL_RESIZE_ENABLED)
                    {
                        Console.WindowWidth = 50;
                        Console.BufferWidth = 50;
                        Console.WindowHeight = 12;
                    }
                    Console.Clear();
                    emulation.run();
                }
            }
            Environment.Exit(0);
        }
    }
}
