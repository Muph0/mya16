using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mp16
{
    class Error
    {
        public Error()
        {

        }
        public Error(string msg, string file, int line)
        {
            this.Message = msg;
            this.File = file;
            this.line = line;
        }

        public string Message, File;
        public int line;

        public static Error FileNotFound(string filename)
        {
            return new Error("File '" + filename + "' not found.",
                Program.debug_CurrentFilename, Program.debug_CurrentLine);
        }
        public static Error InvalidStringLiteral(string what)
        {
            return new Error("Unsupported string format. '" + what + "' is not a valid string literal.",
                Program.debug_CurrentFilename, Program.debug_CurrentLine);
        }
        public static Error InvalidIr(string what)
        {
            return new Error("'" + what + "' is not a valid instruction.",
                Program.debug_CurrentFilename, Program.debug_CurrentLine);
        }
        public static Error Expected(string what, string instead_of)
        {
            return new Error("Expected " + what + " instead of '" + instead_of + "'",
                Program.debug_CurrentFilename, Program.debug_CurrentLine);
        }
        public static Error DeclaredAlready(string what)
        {
            return new Error(what + " cannot be declared twice.",
                Program.debug_CurrentFilename, Program.debug_CurrentLine);
        }
        public static Error Arguments( string ir,string correct)
        {
            return new Error("Wrong arguments of " +ir+ ". Syntax: " + correct,
                Program.debug_CurrentFilename, Program.debug_CurrentLine);
        }
        public static Error Empty(string content)
        {
            return new Error(content,
                Program.debug_CurrentFilename, Program.debug_CurrentLine);
        }
    }
}
