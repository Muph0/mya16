using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using mp16.irs;

namespace mp16_interpreter
{
    class Mobo
    {
        private System.Diagnostics.Stopwatch stopky = new System.Diagnostics.Stopwatch();
        private long lastUpdate = -1, sleepTime = 0, cycles = 0;
        string sleepestring = "SLEEEEEEP TAJM 00000";

        public Mobo()
        {
            try
            {
                sleepTime = int.Parse(sleepestring.Split(' ')[2].TrimStart("0".ToArray()));
            }
            catch { }
        }

        private byte ah
        { get { return (byte)getReg("ah"); } }
        private byte bh
        { get { return (byte)getReg("bh"); } }
        private byte ch
        { get { return (byte)getReg("ch"); } }
        private byte dh
        { get { return (byte)getReg("dh"); } }

        public Dictionary<string, ushort> Registers = new Dictionary<string, ushort>
        {
            {"ax", 0},
            {"bx", 0},
            {"cx", 0},
            {"dx", 0},
            {"adr", 0},
            {"sp", 0},
            {"bp", 0},
            {"rm", 0},
        };

        public ushort pc;

        public ushort[] memory = new ushort[ushort.MaxValue + 1];
        public Stack<ushort> callStack = new Stack<ushort>();
        public Queue<char> keyboardBuffer = new Queue<char>();

        public ushort getReg(string sreg)
        {
            if (sreg.Last() == 'l')
            {
                sreg = sreg.First() + "x";
                return (ushort)(Registers[sreg] & 0x00ff);
            }
            if (sreg.Last() == 'h')
            {
                sreg = sreg.First() + "x";
                return (ushort)((Registers[sreg] & 0xff00) >> 8);
            }
            return Registers[sreg];
        }
        public void setReg(string dreg, ushort val)
        {
            if (dreg.Last() == 'l')
            {
                dreg = dreg.First() + "x";
                Registers[dreg] = (ushort)((Registers[dreg] & 0xff00) | (val & 0x00ff));
                return;
            }
            if (dreg.Last() == 'h')
            {
                dreg = dreg.First() + "x";
                Registers[dreg] = (ushort)((Registers[dreg] & 0x00ff) | ((val & 0x00ff) << 8));
                return;
            }
            Registers[dreg] = val;
        }
        public void interrupt(ushort immed)
        {
            if (immed == 1) // TTY
            {
                byte asc = (byte)getReg("ah");
                char c = Encoding.ASCII.GetChars(new byte[] { asc })[0];
                Console.Write(c);
                if (asc == 12 || asc == 128)
                    Console.Clear();
            }
            else if (immed == 2) // Keyboard
            {
                byte ah = (byte)getReg("ah");
                char c = '\0';
                if (keyboardBuffer.Count > 0)
                    c = keyboardBuffer.Dequeue();
                byte asc = Encoding.ASCII.GetBytes(new char[] { c })[0];
                if ((ah & 1) != 1)
                {
                    setReg("ah", asc);
                    interrupt(1);
                    setReg("ah", ah);
                }
                setReg("al", asc);
            }
        }
        public void readKeys()
        {
            while (true)
            {
                char c = Console.ReadKey(true).KeyChar;
                keyboardBuffer.Enqueue(c);
            }
        }

        public bool flags_equal = false;
        public bool flags_grater = false;
        public bool flags_zero = false;
        public bool flags_negative = false;

        public void run()
        {
            Instruction.Init();
            stopky.Restart();

            Dictionary<int, string> rev_mnemonics = new Dictionary<int, string>();
            foreach (KeyValuePair<string, byte> kp in Instruction.Mnemonics)
            {
                rev_mnemonics.Add(kp.Value, kp.Key);
            }
            Dictionary<int, string> rev_regnames = new Dictionary<int, string>();
            foreach (KeyValuePair<string, byte> kp in Instruction.Registers)
            {
                rev_regnames.Add(kp.Value, kp.Key);
            }

            while (true)
            {
                if (sleepTime > 0)
                    Thread.Sleep((int)sleepTime);

                ushort ir = memory[pc];
                pc++;
                ushort immed = memory[pc];

                int pc_readOnly = pc;

                int opcode = (ir & 0xff00) >> 8;
                if (!rev_mnemonics.ContainsKey(opcode))
                    throw new NotImplementedException("Instruction '" + opcode.ToString("X") + "' not implemented");

                string mnem = rev_mnemonics[opcode];
                string dreg = rev_regnames[(ir & 0x00f0) >> 4];
                string sreg = rev_regnames[(ir & 0x000f)];

                string readlineStack = "";
                for (int i = 0; i < 40; i++)
                {
                    readlineStack += (char)memory[i + 4];
                }
                System.Diagnostics.Debug.Print(readlineStack);

                if (pc > 200)
                { }

                ushort sregVal = getReg(sreg);
                ushort dregVal = getReg(dreg);

                switch (mnem)
                {
                    case "and":     // alu family
                        setReg(dreg, (ushort)(dregVal & sregVal));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "sub":
                        setReg(dreg, (ushort)(dregVal - sregVal));
                        setReg("rm", (ushort)((dregVal - sregVal) >> 16));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "mul":
                        setReg(dreg, (ushort)(dregVal * sregVal));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "div":
                        if (sregVal == 0)
                        {
                            setReg(dreg, 0);
                            setReg("rm", 0);
                        }
                        else
                        {
                            setReg(dreg, (ushort)(dregVal / sregVal));
                            setReg("rm", (ushort)(dregVal % sregVal));
                        }
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "add":
                        setReg(dreg, (ushort)(dregVal + sregVal));
                        setReg("rm", (ushort)(((dregVal + sregVal) >> 16) & 1));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "or":
                        setReg(dreg, (ushort)(dregVal | sregVal));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "xor":
                        setReg(dreg, (ushort)(dregVal ^ sregVal));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "nor":
                        setReg(dreg, (ushort)((0xffff ^ dregVal) | sregVal));
                        COMPARE(getReg(dreg), sregVal);
                        break;

                    case "iand":    // alu immed family
                        pc++;
                        setReg(dreg, (ushort)(dregVal & immed));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "isub":
                        pc++;
                        setReg(dreg, (ushort)(dregVal - immed));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "imul":
                        COMPARE(dregVal, sregVal);
                        pc++;
                        setReg(dreg, (ushort)(dregVal * immed));
                        break;
                    case "idiv":
                        pc++;
                        setReg(dreg, (ushort)(dregVal / immed));
                        setReg("rm", (ushort)(dregVal % immed));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "iadd":
                        pc++;
                        setReg(dreg, (ushort)(dregVal + immed));
                        setReg("rm", (ushort)(((dregVal + immed) >> 16) & 1));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "ior":
                        pc++;
                        setReg(dreg, (ushort)(dregVal | immed));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "ixor":
                        pc++;
                        setReg(dreg, (ushort)(dregVal ^ immed));
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "inor":
                        pc++;
                        setReg(dreg, (ushort)((0xffff ^ dregVal) | immed));
                        COMPARE(getReg(dreg), sregVal);
                        break;

                    case "rdiv":
                        if (sregVal == 0)
                        {
                            setReg(dreg, 0);
                            setReg("rm", 0);
                        }
                        else
                        {
                            setReg(dreg, (ushort)(((Registers["rm"] << 16) + dregVal) / sregVal));
                            setReg("rm", (ushort)(((Registers["rm"] << 16) + dregVal) % sregVal));
                        }
                        COMPARE(getReg(dreg), sregVal);
                        break;
                    case "radd":
                        setReg(dreg, (ushort)(dregVal + sregVal + (1 & Registers["rm"])));
                        setReg("rm", (ushort)(((dregVal + sregVal + (1 & Registers["rm"])) >> 16) & 1));
                        COMPARE(getReg(dreg), sregVal);
                        break;


                    case "jmp":     // jump if family
                        pc = immed;
                        break;
                    case "je":
                        pc++;
                        if (flags_equal)
                            pc = immed;
                        break;
                    case "jl":
                        pc++;
                        if (!flags_equal && !flags_grater)
                            pc = immed;
                        break;
                    case "jg":
                        pc++;
                        if (flags_grater)
                            pc = immed;
                        break;
                    case "jle":
                        pc++;
                        if (!flags_grater)
                            pc = immed;
                        break;
                    case "jge":
                        pc++;
                        if (flags_grater | flags_equal)
                            pc = immed;
                        break;
                    case "jz":
                        pc++;
                        if (flags_zero)
                            pc = immed;
                        break;
                    case "jn":
                        pc++;
                        if (flags_negative)
                            pc = immed;
                        break;
                    case "jne":
                        pc++;
                        if (!flags_equal)
                            pc = immed;
                        break;
                    case "jnz":
                        pc++;
                        if (!flags_zero)
                            pc = immed;
                        break;
                    case "jp":
                        pc++;
                        if (!flags_negative)
                            pc = immed;
                        break;

                    case "iload":
                        pc++;
                        setReg(dreg, memory[immed]);
                        break;
                    case "istore":
                        pc++;
                        memory[immed] = sregVal;
                        break;
                    case "load":
                        setReg(dreg, memory[sregVal]);
                        break;
                    case "store":
                        memory[dregVal] = sregVal;
                        break;
                    case "li":
                        pc++;
                        setReg(dreg, immed);
                        break;
                    case "mov":
                        setReg(dreg, sregVal);
                        break;

                    case "call":
                        pc++;
                        callStack.Push(pc);
                        pc = immed;
                        break;
                    case "ret":
                        pc = callStack.Pop();
                        break;

                    case "cmp":
                        COMPARE(dregVal, sregVal);
                        break;
                    case "icmp":
                        pc++;
                        COMPARE(dregVal, immed);
                        break;
                    case "int":
                        pc++;
                        if (immed == 0xffff) // PWR box
                        {
                            if (getReg("ah") == 0)
                                break;
                            if (getReg("ah") == 1)
                            {
                                this.run();
                                return;
                            }
                        }
                        interrupt(immed);
                        break;

                    default:
                        throw new NotImplementedException("This instruction (" + mnem + ") is not implemented. Address:" + pc + ":" + pc.ToString("x"));
                }

                if (getReg("ah") == 0 && immed == 0xffff && mnem == "int")
                    break;
                if (immed == pc_readOnly - 1 && mnem == "jmp")
                    break;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nProgram halted.");
            Console.ResetColor();
            Console.ReadLine();
            Environment.Exit(0);
        }

        void COMPARE(int dregVal, int sregVal)
        {
            flags_grater = false;
            flags_equal = false;
            if (dregVal > sregVal)
                flags_grater = true;
            if (dregVal == sregVal)
            {
                flags_equal = true;
                flags_grater = false;
            }
            flags_zero = false;
            flags_negative = false;
            if (dregVal >> 15 == 1)
                flags_negative = true;
            if (dregVal == 0)
                flags_zero = true;
        }
    }
}
