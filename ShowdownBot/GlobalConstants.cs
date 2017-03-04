using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowdownBot
{
    public static class GlobalConstants
    {
        public const string SDB_VERSION = "0.6.0-unreleased";
        public const string SDB_TITLEBAR = "Showdown Bot v" + SDB_VERSION;
        public const string FORMAT_RANDOMSINGLE = "gen7randombattle";
        public const string FORMAT_OU = "gen7ou";
        public const string PERSONAL_PRE = "my_";
        //Colors
        public static ConsoleColor COLOR_WARN = ConsoleColor.Yellow;
        public static ConsoleColor COLOR_OK = ConsoleColor.Green;
        public static ConsoleColor COLOR_SYS = ConsoleColor.Cyan;
        public static ConsoleColor COLOR_ERR = ConsoleColor.Red;

        public const int MAX_WAIT_TIME_S = 15;
        public const string VERSIONFILE_URL = "https://raw.githubusercontent.com/Deviach/ShowdownBot/master/VERSION";
        /// <summary>
        /// Time in seconds to wait for a player challenge response.
        /// </summary>
        public const int MAX_WAIT_FOR_PLAYER_RESPONSE = 120;

        /// <summary>
        /// The upper bound on hits-to-KO a move can have.
        /// </summary>
        public const int MAX_HKO = 10;

        /// <summary>
        /// The highest rank for a move.
        /// </summary>
        public const int MAX_MOVE_RANK = 15;
#if LINUX
        public static ConsoleColor COLOR_BOT = ConsoleColor.Blue;
#else
        public static ConsoleColor COLOR_BOT = ConsoleColor.Magenta;
#endif
        public static ConsoleColor COLOR_DEFAULT = ConsoleColor.Gray;
    }
}
