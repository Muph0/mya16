using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mya
{
    class Raw20
    {
        public static void WriteBytes(int[] bytes, string file)
        {
            string result = "v2.0 raw";

            for (int i = 0; i < bytes.Length; i++)
            {
                if (i % 8 == 0)
                    result += (char)10;
                else
                    result += " ";

                result += bytes[i].ToString("x");
            }

            File.WriteAllText(file, result);
        }
    }
}
