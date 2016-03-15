﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using mp16.irs;

namespace mp16
{
    class Program
    {
        public static Dictionary<string, int> Identifiers;
        //static Stack<int> include_fwd_stack;
        //static Stack<int[]> include_stack;
        static List<int> byteOnLine = new List<int>();

        public static string debug_CurrentFilename = "", workingDirectory;
        public static int debug_CurrentLine = -1, fwd_bytes = 0;

        public static bool NoError = true, CountingPhase = true, DebugMode = false;

        static void Main(string[] args)
        {
            Instruction.Init();
            /*
            Console.Write("[");
            Console.Write('"' + Instruction.Mnemonics.Keys.ElementAt(0) + '"');
            for (int i = 1; i < Instruction.Mnemonics.Keys.Count; i++)
            {
                Console.Write(", \"" + Instruction.Mnemonics.Keys.ElementAt(i) + '"');
            }
            Console.WriteLine("]");

            Console.Write("[");
            Console.Write('"' + Instruction.Registers.Keys.ElementAt(0) + '"');
            for (int i = 1; i < Instruction.Registers.Keys.Count; i++)
            {
                Console.Write(", \"" + Instruction.Registers.Keys.ElementAt(i) + '"');
            }
            Console.Write("]");

            Console.ReadLine();
            */
            //include_fwd_stack = new Stack<int>();
            //include_stack = new Stack<int[]>();

            string filename = @"C:\Users\Honky\Documents\Logisim\16-bit\test2.mya16";

            Console.BufferHeight = 1800;

            if (args != null && args.Length > 0)
                filename = args[0];
            if (args.Contains("-debug"))
                DebugMode = true;

            DebugMode = true;
            workingDirectory = filename.Substring(0, filename.Length - filename.Split('\\').Last().Length);

            Identifiers = new Dictionary<string, int>();
            //Identifiers.Add("[STACK]", 0xF000);
            Identifiers.Add("endl", 0x000a);
            Identifiers.Add("cret", 0x000d);
            Identifiers.Add("space", 0x0020);

            string source = File.ReadAllText(filename);
            //source = "mov sp, [STACK]\n" + source;


            List<int> bytes = new List<int>();
            bytes.AddRange(Compile(source, filename.Split('\\').Last()));

            if (NoError)
            {
                mya.Raw20.WriteBytes(bytes.ToArray(), filename.Substring(0, filename.LastIndexOf('.')) + ".bin16");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Compilation sucessfull!");
            }

            Terminate();
        }

        public static void PrintError(Error e)
        {
            if (CountingPhase) return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: '{0}' line {1}: {2}", e.File, e.line + 1, e.Message);
            Console.ResetColor();
            NoError = false;
        }
        public static void PrintWarning(Error e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("'{0}' line {1}: {2}", e.File, e.line + 1, e.Message);
            Console.ResetColor();
        }
        public static void Terminate()
        {
            Console.ReadLine();
            Environment.Exit(1);
        }

        public static int[] Compile(string source, string curr_file)
        {
            debug_CurrentFilename = curr_file;
            List<int> bytes = new List<int>();
            CountingPhase = true;

            int started_fwd_bytes = fwd_bytes;
            source = removeComments(source);

            while (true)
            {
                List<string> files_included = new List<string>();
                Regex includeRegex = new Regex("\\#include \"(.*)\"", RegexOptions.Compiled);
                int matches = 0;
                source = includeRegex.Replace(source, new MatchEvaluator(delegate(Match m)
                  {
                      matches++;
                      string file = m.Groups[1].Value;

                      if (!files_included.Contains(file))
                      {
                          files_included.Add(file);
                      }
                      else
                      {
                          PrintError(new Error() { Message = "'" + file + "' was already included." });
                          return "";
                      }

                      string ext = ".mya16";
                      if (!File.Exists(file) && (file.Length < ext.Length || file.Substring(file.Length - 6) != ext))
                      {
                          file += ext;
                      }

                      if (!File.Exists(file))
                      {
                          file = workingDirectory + file;
                      }

                      if (File.Exists(file))
                      {
                          string complete = "#included \"" + m.Groups[1].Value + "\" \n" + removeComments(File.ReadAllText(file))
                              + "\n#end_of_include \"" + m.Groups[1].Value + "\"";
                          return complete;
                      }

                      PrintError(new Error() { Message = "Atempted to include nonexistent file '" + file + "'." });
                      return m.Value;
                  }));
                if (matches == 0)
                    break;
            }

            string[] lines = source.Split('\n');
            int[] computedBytes = new int[lines.Length], compiledBytes = new int[lines.Length];

            for (int line_n = 0; line_n < lines.Length; line_n++)
            {
                string line = lines[line_n].Trim();
                debug_CurrentLine = line_n;

                CountingPhase = false;
                string[] line_tokens = line.Split(' ');
                if (!(line.Length > 0)) continue;

                if (line_tokens[0].Last() == ':')
                {
                    string curr_cast = line.Split(' ')[0].TrimEnd(":".ToArray());
                    if (!Identifiers.ContainsKey(curr_cast))
                        Identifiers.Add(curr_cast, fwd_bytes);
                    else
                        PrintError(Error.DeclaredAlready("Cast " + curr_cast));

                    lines[line_n] = line.Substring(curr_cast.Length + 1).Trim();
                    line = lines[line_n];
                }
                if (!(line.Length > 0)) continue;

                // preprocessors
                /*
                if (line.Length > 8 && line.Substring(0, 8) == "#include") // include preproc
                {
                    string str_lit = line.Substring(8).Trim();  // get the string literal
                    if (ValidateStringLiteral(str_lit))         // if it is valid, go on, otherwise print error msg
                    {
                        str_lit = str_lit.Trim('"');                // get rid of the quotes ""

                        string ext = ".mya16";
                        if (!File.Exists(str_lit) && (str_lit.Length < ext.Length || str_lit.Substring(str_lit.Length - 6) != ext))
                        {
                            str_lit += ext;
                        }

                        if (!File.Exists(str_lit))
                        {
                            str_lit = workingDirectory + str_lit;
                        }

                        if (File.Exists(str_lit))
                        {
                            string included_source = File.ReadAllText(str_lit);    // load the source file
                            included_source = included_source.Replace("\r", "").Replace("\t", "    ");
                            included_source = Regex.Replace(included_source, ";.*\n", "\n");

                            int[] compiled_source = Compile(included_source, str_lit);
                            include_fwd_stack.Push(fwd_bytes);
                            include_stack.Push(compiled_source);

                            // debug info
                            debug_CurrentFilename = curr_file;
                            debug_CurrentLine = line_n;
                        }
                        else
                        {
                            PrintError(Error.FileNotFound(str_lit));
                            include_stack.Push(new int[0]);
                            include_fwd_stack.Push(fwd_bytes);
                        }
                    }
                    else
                    {
                        PrintError(Error.InvalidStringLiteral(str_lit));
                        include_stack.Push(new int[0]);
                        include_fwd_stack.Push(fwd_bytes);
                    }
                    continue;
                }
                */
                CountingPhase = true;

                string mnem = line.Split(' ')[0];           // mnemonic is the first word
                Instruction ir = Instruction.Get(mnem);     // construct the ir object
                if (ir != null)
                {
                    int[] range = ir.Process(line.Substring(mnem.Length).Trim());

                    fwd_bytes += range.Length;
                    computedBytes[line_n] = range.Length;
                }
            }

            CountingPhase = false; // END OF COUNTING PHASE
            fwd_bytes = started_fwd_bytes;
            for (int line_n = 0; line_n < lines.Length; line_n++)
            {
                string line = lines[line_n].Trim();
                if (line == "") continue;
                if (line.Length > 9 && line.Substring(0, 9) == "#included")
                {
                    if (DebugMode)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(line);
                        Console.ResetColor();
                    }
                    continue;
                }//*/
                if (line.Length > 15 && line.Substring(0, 15) == "#end_of_include")
                {
                    if (DebugMode)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(line);
                        Console.ResetColor();
                    }
                    continue;
                }//*/
                debug_CurrentLine = line_n;
                string mnem = line.Split(' ')[0];           // mnemonic is the first word
                Instruction ir = Instruction.Get(mnem);     // construct the ir object
                if (ir != null)
                {
                    bool consistent = true;

                    int[] range = ir.Process(line.Substring(mnem.Length).Trim());
                    bytes.AddRange(range);
                    compiledBytes[line_n] = range.Length;

                    if (DebugMode)
                    {
                        bool first = true;
                        foreach (int i in range)
                        {
                            foreach (KeyValuePair<string, int> pair in Identifiers)
                            {
                                if (pair.Value == fwd_bytes)
                                {
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine(pair.Key + ":     0x" + fwd_bytes.ToString("x"));
                                    Console.ResetColor();
                                }
                            }

                            if (first)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("0x" + fwd_bytes.ToString("x") + ": " + line);
                                if (compiledBytes[line_n] != computedBytes[line_n])
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write(" " + computedBytes[line_n]);
                                    consistent = false;
                                }
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine(" L:" + compiledBytes[line_n]);
                                Console.ResetColor();

                                first = false;
                            }

                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.WriteLine("0x" + fwd_bytes.ToString("x") + ": 0x" + i.ToString("x"));
                            Console.ResetColor();
                            byteOnLine.Add(fwd_bytes);
                            fwd_bytes++;
                        }
                    }
                    else fwd_bytes += range.Length;

                    if (!consistent)
                    {
                        PrintError(Error.Empty("Inconsistent compilation! Compiler crashed.\n" +
                               "(This is compiler's fault, your sources may be ok)"));
                        return new int[0];
                    }
                }
                else PrintError(Error.InvalidIr(mnem));
            }

            if (DebugMode)
            {
                Console.WriteLine("\n Casts:");
                foreach (KeyValuePair<string, int> pair in Identifiers)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write('"' + pair.Key + '"');
                    Console.ResetColor();
                    Console.Write(" = ");
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.WriteLine("0x" + pair.Value.ToString("X"));
                    Console.ResetColor();
                }
            }

            return bytes.ToArray();
        }

        private static string removeComments(string s)
        {
            s = s.Replace("\r", "").Replace("\t", "    ");
            s = Regex.Replace(s, ";.*\n", "\n");

            return s;
        }

        public static bool ValidateStringLiteral(string with_quotes)
        {
            return "\"" + with_quotes.Substring(1, with_quotes.Length - 2) + "\"" == with_quotes;
        }

    }
}
