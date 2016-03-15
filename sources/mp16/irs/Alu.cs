using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16.irs
{
    class Alu : Instruction
    {
        public Alu(string mnem)
            : base(mnem, IRArgsType.Nothing)
        {
            this.mnem = mnem;
        }

        public override int[] Process(string arguments)
        {
            Instruction ir = new Instruction(mnem, IRArgsType.Register__Register);
            if (Instruction.ParseArguments(arguments, IRArgsType.Register__Number, true) != null)
                return new Instruction(mnem, IRArgsType.Register__Number).Process(arguments);

            return ir.Process(arguments);
        }
    }
}
