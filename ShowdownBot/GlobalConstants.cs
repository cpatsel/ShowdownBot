using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowdownBot
{
    public static class GlobalConstants
    {
        public const string SDB_VERSION = "1.0.1";
        public const string SDB_TITLEBAR = "Showdown Bot v" + SDB_VERSION;
        /// <summary>
        /// The format name for random singles as defined internally by Pokemon Showdown.
        /// </summary>
        public const string FORMAT_RANDOMSINGLE = "gen7randombattle";

        /// <summary>
        /// Format name for OU singles as defined internally by Pokemon Showdown.
        /// </summary>
        public const string FORMAT_OU = "gen7ou";

        /// <summary>
        /// Prefix denoting that this pokemon's stats have been defined in roleOverride.js
        /// </summary>
        public const string PERSONAL_PRE = "my_";

        /* Colors */
        public static ConsoleColor COLOR_WARN = ConsoleColor.Yellow;
        public static ConsoleColor COLOR_OK = ConsoleColor.Green;
        public static ConsoleColor COLOR_SYS = ConsoleColor.Cyan;
        public static ConsoleColor COLOR_ERR = ConsoleColor.Red;

        /// <summary>
        /// Maximum wait time in seconds for the waitFind methods.
        /// </summary>
        public const int MAX_WAIT_TIME_S = 15;

        public const string VERSIONFILE_URL = "https://raw.githubusercontent.com/Deviach/ShowdownBot/master/VERSION";

        /// <summary>
        /// Time in seconds to wait for a player challenge response.
        /// </summary>
        public const int MAX_WAIT_FOR_PLAYER_RESPONSE = 120;

        /// <summary>
        /// The upper bound on hits-to-KO a move can have before being deemed useless.
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
