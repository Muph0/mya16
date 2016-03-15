using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16.irs
{
    class PreDef : Instruction
    {
        public PreDef()
            : base ("#def", IRArgsType.Nothing)
        {

        }

        public override int[] Process(string str_args)
        {
            int breakchr = 5;

            if (Program.CountingPhase)
            {
                string cast = str_args.Substring(0, breakchr);
                if (!Program.Identifiers.ContainsKey(cast))
                {
                    Program.Identifiers.Add(cast, Program.fwd_bytes + 2);
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
