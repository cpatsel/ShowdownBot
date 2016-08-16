using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowdownBot
{
    public static class GlobalConstants
    {
        public const string SDB_VERSION = "0.3.1-unreleased";
        public const string SDB_TITLEBAR = "Showdown Bot v" + SDB_VERSION;
        //Colors
        public static ConsoleColor COLOR_WARN = ConsoleColor.Yellow;
        public static ConsoleColor COLOR_OK = ConsoleColor.Green;
        public static ConsoleColor COLOR_SYS = ConsoleColor.Cyan;
        public static ConsoleColor COLOR_ERR = ConsoleColor.Red;
        
#if LINUX
        public static ConsoleColor COLOR_BOT = ConsoleColor.Blue;
#else
        public static ConsoleColor COLOR_BOT = ConsoleColor.Magenta;
#endif
        public static ConsoleColor COLOR_DEFAULT = ConsoleColor.White;
    }
}
