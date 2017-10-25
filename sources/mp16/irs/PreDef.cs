using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16.irs
{
    class PreDef : Instruction
    {
        public PreDef()
            : base("#var", IRArgsType.Nothing)
        {

        }

        public override int[] Process(string str_args)
        {
            string[] args = str_args.Trim().Split(' ');
            if (args.Length != 2)
            {
                Program.PrintError(Error.Arguments("#def", "<pointer> <address>"));
                return null;
            }

            string cast = args[0].Trim();
            int address = -1;
            if (!int.TryParse(args[1].Trim(), out address))
            {
                Program.PrintError(Error.Expected("number", args[1].Trim()));
            }

            if (Program.CountingPhase)
            {
                if (!Program.Identifiers.ContainsKey(cast))
                {
                    Program.Identifiers.Add(cast, address);
                }
                else
                {
                    Program.PrintError(Error.DeclaredAlready(cast));
                }
            }

            return new int[0];
        }
    }
}
