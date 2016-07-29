using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowdownBot
{
    public static class GlobalConstants
    {
        public const string SDB_VERSION = "0.3.0-unreleased";
        public const string SDB_TITLEBAR = "Showdown Bot v" + SDB_VERSION;
        //Colors
        public static ConsoleColor COLOR_WARN = ConsoleColor.Yellow;
        public static ConsoleColor COLOR_OK = ConsoleColor.Green;
        public static ConsoleColor COLOR_SYS = ConsoleColor.Cyan;
        public static ConsoleColor COLOR_ERR = ConsoleColor.Red;
        public static ConsoleColor COLOR_BOT = ConsoleColor.Blue;
        public static ConsoleColor COLOR_DEFAULT = ConsoleColor.White;
    }
}
