using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
namespace ShowdownBot
{
    /// <summary>
    /// Contains the in-console help documentation for commands.
    /// </summary>
    public partial class Consol
    {
        private void DisplayHelp()
        {

            writef("Available commands are: challenge, clear, dump, exit, info,\n kill, module, " +
                    "start, startf, stop, version", "[SYSTEM]", COLOR_SYS);

        }

        private void help(string cmnd)
        {
            string desc = cmnd;
            string alias = "None";
            string arguments = "None";
            if (cmnd == "learn")
            {
                desc = "learn: Starts the bot's replay learning and downloading features";
                alias = "l";
                arguments = "\t [d][#] - Download # replays.\n" +
                         "\t \t [b] - Build database from replays.";
            }
            else if (cmnd == "search" || cmnd == "ladder")
            {
                desc = "search: The bot will search the ladder for an opponent";
                alias = "ladder";

            }
            else if (cmnd == "challenge" || cmnd == "cp")
            {
                desc = "challenge: Have the bot challenge a player";
                alias = "cp";
                arguments = "\t [... =owner] - Challenge specified player.\n";

            }
            else if (cmnd == "format" || cmnd == "f")
            {
                desc = "format: Change the bot module's selected format.";
                alias = "f";
                arguments = "\t [...] - Format name";
            }
            else if (cmnd == "exit")
            {
                desc = "exit: Kills the bot and quits the program.";
            }
            else if (cmnd == "module" || cmnd == "m")
            {
                desc = "module: Switch the bot's AI module.";
                alias = "mode, m";
                arguments = "\t [...] - Module (Analytic, Random, Biased)";
            }
            else if (cmnd == "me")
            {
                writef("I'm a robot, not a miracle worker.", "system", COLOR_SYS);
                return;
            }

            writef(desc + "\n" +
                    "alias: " + alias + "\n" +
                    "arguments: " + arguments, "system", COLOR_SYS);
        }
    }
}
