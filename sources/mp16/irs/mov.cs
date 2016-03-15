using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16.irs
{
    class Mov : Instruction
    {
        public Mov()
            : base("mov", IRArgsType.Register__Register)
        {

        }

        public override int[] Process(string str_args)
        {
            int[] args = null;

            args = Instruction.ParseArguments(str_args, IRArgsType.Register__Register, true);
            if (args != null)
            {
                return new Instruction("mov", IRArgsType.Register__Register).Process(str_args);
            }

            args = Instruction.ParseArguments(str_args, IRArgsType.Register__Number, true);
            if (args != null)
            {
                return new Instruction("li", IRArgsType.Register__Number).Process(str_args);
            }

            // handling extra []
            string[] proc_args = str_args.Trim().Split(',');
            if (proc_args.Length != 2)
            {
                Program.PrintError
                    (Error.InvalidIr("mov " + str_args));
                return null;
            }
            int adr = -1;
            for (int i = 0; i < proc_args.Length; i++)
            {
                proc_args[i] = proc_args[i].Trim();
                string arg = proc_args[i];

                if ('[' + arg.Substring(1, arg.Length - 2) + ']' == arg)
                {
                    adr = i;
                    break;
                }
            }

            if (adr == 0)
            {
                string s = proc_args[0].Substring(1, proc_args[0].Length - 2) + "," + proc_args[1];

                if (ParseArguments(s, IRArgsType.Register__Register, true) != null)
                    return new Instruction("store", IRArgsType.Register__Register).Process(s);
                if (ParseArguments(s, IRArgsType.Number__Register, true) != null)
                    return new Instruction("istore", IRArgsType.Number__Register).Process(s);
            }
            if (adr == 1)
            {
                string s = proc_args[0] + "," + proc_args[1].Substring(1, proc_args[1].Length - 2);

                if (ParseArguments(s, IRArgsType.Register__Register, true) != null)
                    return new Instruction("load", IRArgsType.Register__Register).Process(s);
                if (ParseArguments(s, IRArgsType.Register__Number, true) != null)
                    return new Instruction("iload", IRArgsType.Register__Number).Process(s);
            }

            Program.PrintError(Error.InvalidIr("mov " + str_args));
            return new int[0];
        }
    }
}
