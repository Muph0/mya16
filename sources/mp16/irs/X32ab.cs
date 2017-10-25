using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16.irs
{
    class X32ab : Instruction
    {
        public X32ab()
            : base("#32", IRArgsType.Register__Register)
        {

        }

        public override int[] Process(string str_args)
        {
            List<int> bytes = new List<int>();

            string[] args = str_args.Split(' ');
            
            for (int i = 0; i < args.Length; i++) args[i] = args[i].Trim();
            
            int input = 0;
            if (args.Length != 3 ||
                !Registers.ContainsKey(args[0]) || !Registers.ContainsKey(args[1]) ||
                !int.TryParse(args[2], out input))
                Program.PrintError(Error.Arguments(mnem, "<HIreg>, <LOreg>, <number>"));

            int LOpart = input & 65535;
            int HIpart = (input >> 16) & 65535;
            
            bytes.AddRange(Instruction.Get("li").Process(args[0] + "," + HIpart.ToString()));
            bytes.AddRange(Instruction.Get("li").Process(args[1] + "," + LOpart.ToString()));

            return bytes.ToArray();
        }
    }
}
