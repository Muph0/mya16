using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16.irs
{
    class PreString : Instruction
    {
        public PreString()
            : base("#string", IRArgsType.Nothing)
        {

        }

        public override int[] Process(string str_args)
        {
            List<int> result = new List<int>();

            result.Add(0);
            result.Add(0);

            int breakchr = 0;
            for (breakchr = 0; breakchr < str_args.Length; breakchr++)
            {
                if (str_args[breakchr] == ' ')
                    break;
            }

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

            str_args = str_args.Substring(breakchr + 1);
            if (Program.ValidateStringLiteral(str_args))
            {
                str_args = str_args.Trim('"');
                str_args += (char)0x03;
                for (int i = 0; i < str_args.Length; i++)
                {
                    char c = str_args[i];

                    if (i < str_args.Length - 1)
                    {
                        if (c == '\\')
                        {
                            c = str_args[i + 1];
                            switch (c)
                            {
                                case 'n':
                                    result.Add((int)'\n');
                                    break;
                                case 'c':
                                    result.Add((int)0x80);
                                    break;
                                default:
                                    result.Add((int)c);
                                    break;
                            }
                            i++;
                            continue;
                        }
                    }

                    result.Add(((int)c) % (int)ushort.MaxValue);
                }
            }

            if (result.Count <= 2)
            {
                Program.PrintError(Error.InvalidStringLiteral(str_args));
            }

            int[] skip =
                new Instruction("jmp", IRArgsType.Number).Process((Program.fwd_bytes + result.Count).ToString());

            result[0] = skip[0];
            result[1] = skip[1];

            return result.ToArray();
        }
    }
}
