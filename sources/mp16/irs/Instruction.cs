using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16.irs
{
    public class Instruction
    {
        public IRArgsType type;
        public string mnem;

        public Instruction(string mnem, IRArgsType type)
        {
            this.mnem = mnem;
            this.type = type;
        }

        // PUBLIC
        public virtual int[] Process(string str_args)
        {
            int OPCODE = Mnemonics[mnem] << 8;

            int[] args = ParseArguments(str_args, type);

            if (args == null)
                return new int[0];

            switch (type)
            {
                case IRArgsType.Nothing:
                    return new int[] { OPCODE };

                case IRArgsType.Number:
                    return new int[] { OPCODE, args[0] };

                case IRArgsType.Register:
                    return new int[] { OPCODE | args[0] | (args[0] << 4) };

                case IRArgsType.Register__Number:
                    return new int[] { OPCODE | (args[0] << 4), args[1] };

                case IRArgsType.Number__Register:
                    return new int[] { OPCODE | args[1], args[0] };

                case IRArgsType.Register__Register:
                    return new int[] { OPCODE | args[1] | (args[0] << 4) };
            }

            throw new NotImplementedException();
        }

        // STATIC
        public static Instruction Get(string mnem)
        {
            IRArgsType type = IRArgsType.Nothing;

            if (Mnemonics.ContainsKey(mnem))
            {
                int value = Mnemonics[mnem];

                if (value >= 0x10 && value <= 0x17)
                    type = IRArgsType.Register__Number; //iadd
                if (value >= 0x00 && value <= 0x07 || mnem == "cmp")
                    return new Alu(mnem); // add
                if ((value >= 0x08 && value <= 0x0f) || (value >= 0x18 && value <= 0x1f))
                    type = IRArgsType.Number; // jump
                if (value >= 0x20 && value <= 0x27)
                    type = IRArgsType.Register__Register;

                if (mnem == "ret")
                    type = IRArgsType.Nothing;
                if (mnem == "call" || mnem == "int")
                    type = IRArgsType.Number;

                if (mnem == "load" || mnem == "store" || mnem == "cmp")
                    type = IRArgsType.Register__Register;

                if (mnem == "li" || mnem == "iload")
                    type = IRArgsType.Register__Number;
                if (mnem == "istore")
                    type = IRArgsType.Number__Register;
            }

            if (mnem == "mov") return new Mov();
            if (mnem == "#string") return new PreString();
            //if (mnem == "#def") return new PreDef();
            if (mnem == "pop" || mnem == "push") return new Stack(mnem);
            if (mnem == "#32") return new X32ab();
            if (mnem == "icmp") type = IRArgsType.Register__Number;

            if (Mnemonics.ContainsKey(mnem))
                return new Instruction(mnem, type);

            return null;
        }
        public static int[] ParseArguments(string args_string, IRArgsType type, bool TRY = false)
        {
            Exception e = new Exception();

            string[] arguments = args_string.Split(',');
            for (int i = 0; i < arguments.Length; i++) arguments[i] = arguments[i].Trim();

            if (type == IRArgsType.Nothing)
            {
                if (arguments.Length == 1 && arguments[0].Length == 0)
                    return new int[0];
            }
            if (type == IRArgsType.Register)
            {
                if (arguments.Length == 1)
                    if (Registers.ContainsKey(arguments[0]))
                    {
                        return new int[] { Registers[arguments[0]] };
                    }
            }
            if (type == IRArgsType.Register__Register)
            {
                if (arguments.Length == 2)
                {
                    if (Registers.ContainsKey(arguments[0]) && Registers.ContainsKey(arguments[1]))
                    {
                        return new int[] { Registers[arguments[0]], Registers[arguments[1]] };
                    }
                }
            }
            if (type == IRArgsType.Number)
            {
                if (arguments.Length == 1)
                    if (TryParseIdentifier(arguments[0]))
                    {
                        return new int[] { ParseIdentifier(arguments[0]) };
                    }
            }
            if (type == IRArgsType.Number__Register)
            {
                if (arguments.Length == 2)
                    if (Registers.ContainsKey(arguments[1]) && TryParseIdentifier(arguments[0]))
                    {
                        return new int[] { ParseIdentifier(arguments[0]), Registers[arguments[1]] };
                    }
            }
            if (type == IRArgsType.Register__Number)
            {
                if (arguments.Length == 2)
                    if (Registers.ContainsKey(arguments[0]) && TryParseIdentifier(arguments[1]))
                    {
                        return new int[] { Registers[arguments[0]], ParseIdentifier(arguments[1]) };
                    }
            }

            if (!TRY)
                Program.PrintError(Error.Expected(type.ToString(), args_string));

            return null;
        }

        public static bool TryParseNumberLiteral(string num_lit)
        {
            try
            {
                ParseNumberLiteral(num_lit);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static int ParseNumberLiteral(string num_lit)
        {
            int i = int.MinValue;
            if (num_lit.Length >= 2)
            {
                if (num_lit.Substring(0, 2) == "0x")
                    return Convert.ToInt32(num_lit.Substring(2), 16);
                if (num_lit.Substring(0, 2) == "0o")
                    return Convert.ToInt32(num_lit.Substring(2), 8);
                if (num_lit.Substring(0, 2) == "0b")
                    return Convert.ToInt32(num_lit.Substring(2), 2);
            }
            if (i == int.MinValue)
                i = Convert.ToInt32(num_lit);

            if (i > ushort.MaxValue)
                throw new Exception();

            return i;
        }

        public static int ParseIdentifier(string name)
        {
            if (Instruction.TryParseNumberLiteral(name))
                return Instruction.ParseNumberLiteral(name);

            if (Program.Identifiers.ContainsKey(name))
                return Program.Identifiers[name];

            if (name.Length == 3 && name.Trim("'".ToArray()).Length == 1)
                return (int)name[1];

            if (name.Length == 4 && name.First() == '\'' && name.Last() == '\'' && name[1] == '\\')
            {
                if (name[2] == 'c')
                    return 12;
                if (name[2] == 'n')
                    return (int)'\n';
                if (name[2] == 'r')
                    return (int)'\r';
                return (int)name[2];
            }


            if (Registers.ContainsKey(name))
                throw new Exception();
            if (name.Length >= 4 && Registers.ContainsKey(name.Substring(1, name.Length - 2)))
                throw new Exception();

            if (Program.CountingPhase)
                return 12345;

            if (name == "$")
                return Program.fwd_bytes;

            throw new NotImplementedException();
        }
        public static bool TryParseIdentifier(string name)
        {
            try
            {
                ParseIdentifier(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region constants
        public static void Init()
        {
            Registers = new Dictionary<string, byte>
            {
                {"ah", 0},
                {"al", 1},
                {"bh", 2},
                {"bl", 3},
                {"ch", 4},
                {"cl", 5},
                {"dh", 6},
                {"dl", 7},
                {"ax", 8},
                {"bx", 9},
                {"cx", 10},
                {"dx", 11},
                {"adr", 12},
                {"bp", 13},
                {"sp", 14},
                {"rm", 15},
            };

            Mnemonics = new Dictionary<string, byte>()
            {
                {"and", 0x00}, // alu family
                {"sub", 0x01},
                {"mul", 0x02},
                {"div", 0x03},
                {"add", 0x04},
                {"or",  0x05},
                {"xor", 0x06},
                {"nor", 0x07},
                {"cmp", 0xe6},

                {"iand", 0x10}, // alu immed
                {"isub", 0x11},
                {"imul", 0x12},
                {"idiv", 0x13},
                {"iadd", 0x14},
                {"ior",  0x15},
                {"ixor", 0x16},
                {"inor", 0x17},
                {"icmp", 0x20},
                
                //{"rand", 0x20}, // alu rem - takes carry IN
                {"rsub", 0x21},
                {"rmul", 0x22},
                {"rdiv", 0x23},
                {"radd", 0x24},
                //{"ror",  0x25},
                //{"rxor", 0x26},
                //{"rnor", 0x27},
            
                {"jmp", 0x08}, // jump if family
                {"je", 0x09},
                {"jl", 0x0a},
                {"jg", 0x0b},
                {"jle", 0x0c},
                {"jge", 0x0d},
                {"jz", 0x0e},
                {"jn", 0x0f}, // if negative  
          
                //{"nil", 0x18}, // jump if not:
                {"jne", 0x19},
                {"jnl", 0x1a},
                {"jng", 0x1b},
                //{"jnle", 0x1c},
                //{"jnge", 0x1d},
                {"jnz", 0x1e},
                {"jp", 0x1f}, // if positive
            
                {"ret", 0xf0}, //call stack family
                {"call", 0xf1},            

                {"int",     0xe5}, // interupt family

                {"iload",   0xe0}, // mov family
                {"istore",  0xe1},
                {"load",    0xe2},  
                {"store",   0xe3},
                {"li",      0xe4},
                {"mov",     0xe7},
            };
        }
        public static Dictionary<string, byte> Registers;
        public static Dictionary<string, byte> Mnemonics;
        #endregion
    }

    public enum IRArgsType
    {
        Register__Register = 0,
        Register,
        Register__Number,
        Number__Register,
        Number,
        Nothing,
    }
}
