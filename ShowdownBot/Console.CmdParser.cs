using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static ShowdownBot.GlobalConstants;
namespace ShowdownBot
{
    partial class Consol
    {
        public void Parse(string t)
        {
            string[] args;
            args = Regex.Split(t, " ");
            if (args[0] == "start")
            {

                writef("Starting bot...", "[SYSTEM]", COLOR_SYS);
                botUseCommand(() => bot.Start(true));
            }
            else if (args[0] == "startf")
            {

                writef("Starting bot without authentication...", "[SYSTEM]", COLOR_SYS);
                botUseCommand(() => bot.Start(false));
            }
            else if (args[0] == "stop" || args[0] == "idle")
            {
                botUseCommand(() => bot.changeState(State.IDLE));
            }
            else if (args[0] == "kill")
            {

                writef("Killing bot.", "[SYSTEM]", COLOR_SYS);
                botUseCommand(() => bot.Kill());
            }
            else if (args[0] == "help")
            {
                if (args.Length > 1)
                    help(args[1]);
                else
                    DisplayHelp();

            }
            else if (args[0] == "search" || args[0] == "ladder")
            {
                botUseCommand(() => bot.changeState(State.SEARCH));
            }
            else if (args[0] == "version")
            {
                writef("ShowdownBot v" + SDB_VERSION, "system", COLOR_SYS);
            }
            else if (args[0] == "format" || args[0] == "f")
            {
                if (paramCheck(2, args, args[0]))
                    botUseCommand(() => bot.changeFormat(args[1]));
            }
            else if (args[0] == "challenge" || args[0] == "cp")
            {
                if (args.Length == 2)
                    botUseCommand(() => bot.challenge(args[1]));
                else if (args.Length == 4)
                {
                    if (args[2] == "-c")
                    {
                        try
                        {
                            botUseCommand(() => bot.setContinuousBattles(int.Parse(args[3])));
                            botUseCommand(() => bot.challenge(args[1]));
                        }
                        catch
                        {
                            writef("Invalid argument " + args[3], "error", COLOR_ERR);
                        }

                    }
                    else
                    {
                        help("challenge");
                    }
                }
                else
                    botUseCommand(() => bot.challenge(bot.getOwner()));
            }
            else if (args[0] == "m" || args[0] == "mode" || args[0] == "module")
            {
                if (paramCheck(2, args, "m"))
                {
                    if (args[1] == "random" || args[1] == "r")
                    {
                        botUseCommand(() => bot.changeMode(ShowdownBot.Bot.AiMode.RANDOM));
                        
                    }
                    else if (args[1] == "biased" || args[1] == "b")
                    {
                        botUseCommand(() => bot.changeMode(ShowdownBot.Bot.AiMode.BIAS));
                    }
                    else if (args[1] == "analytic" || args[1] == "a")
                    {
                         botUseCommand(() => bot.changeMode(ShowdownBot.Bot.AiMode.ANALYTIC));
                    }
                }

            }
            else if (args[0] == "dump" || args[0] == "dumplog")
            {
                botUseCommand(() => bot.saveLog());
            }
            else if (args[0] == "exit" || args[0] == "quit")
            {
                writef("Shutting down.", "[SYSTEM]", COLOR_SYS);
                writef("Killing bot.", "[SYSTEM]", COLOR_SYS);
                botUseCommand(() => bot.Kill());
                Environment.Exit(0);
                this.Close();
            }
            else if (args[0] == "info")
            {
                if (!bot.getStatus())
                {
                    write("No bot running.");
                }
                else
                {
                    botUseCommand(() => bot.printInfo());
                }
            }
            else if (args[0] == "forfeit")
            {
                botUseCommand(() => bot.botForfeit());
            }
            else if (args[0] == "visible" || args[0] == "v")
            {
                //change visibility
            }
            else if (args[0] == "clear" || args[0] == "cls")
            {
                Console.Clear();
            }
            else if (args[0] == "learn" || args[0] == "l")
            {

                if (!paramCheck(2, args, "learn"))
                {

                    return;
                }
                if (args[1] == "d" || args[1] == "download")
                {
                    if (paramCheck(3, args, "learn"))
                    {
                        botUseCommand(() => bot.learn(Int32.Parse(args[2])));
                    }
                }
                else
                {
                    botUseCommand(() => bot.learn(0));
                }

            }
            else if (args[0] == "simulate" || args[0] == "sim")
            {
                botUseCommand(() => bot.simulate(args[1], args[2]));
            }
            else
            {
                writef("Undefined command " + t, "[SYSTEM]", COLOR_SYS);

            }

        }
    }
}
