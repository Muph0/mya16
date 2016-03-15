using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16.irs
{
    class Stack : Instruction
    {
        public Stack(string mnem)
            : base(mnem, IRArgsType.Register)
        {

        }

        public override int[] Process(string str_args)
        {
            int[] args = null;
            bool number = false;

            if (mnem == "push")
            {
                args = ParseArguments(str_args, IRArgsType.Number, true);
                number = true;
            }

            if (Registers.ContainsKey(str_args))
            {
                args = ParseArguments(str_args, IRArgsType.Register, true);
                number = false;
            }

            if (args == null)
            {
                Program.PrintError(Error.Expected("number or register", str_args));
                return new int[0];
            }

            List<int> bytes = new List<int>();
            if (mnem == "push")
            {
                if (number)
                {
                    bytes.AddRange(Instruction.Get("li").Process("bp, " + str_args));
                    str_args = "bp";
                }
                bytes.AddRange(Instruction.Get("store").Process("sp, " + str_args));
                bytes.AddRange(Instruction.Get("iadd").Process("sp, 1"));
            }
            else if (mnem == "pop")
            {
                bytes.AddRange(Instruction.Get("isub").Process("sp, 1"));
                bytes.AddRange(Instruction.Get("mov").Process(str_args + ", [sp]"));
            }

            if (bytes.Count == 4)
            { }
            return bytes.ToArray();
        }
    }
}
