using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static ShowdownBot.GlobalConstants;
using static ShowdownBot.Global;
namespace ShowdownBot
{
    partial class Consol
    {
       
        public void Parse(string t)
        {
            string[] args;
            args = Regex.Split(t, " ");
            Dictionary<string, string> param = parseCmdArgs(args);
            if (args[0] == "start")
            {
                if (args.Length > 1)
                {
                    
                    if (isSet(param, "-u") && isSet(param, "-p"))
                        botUseCommand(() => bot.Start(param["-u"], param["-p"]),true);
                    else if (isSet(param, "-u"))
                        botUseCommand(() => bot.Start(param["-u"], null),true);
                    else
                        help("start");
                }
                else
                    botUseCommand(() => bot.Start(true),true);

            }
            else if (args[0] == "startf")
            { 
                botUseCommand(() => bot.Start(false),true);
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
                if (isSet(param, "-f"))
                    botUseCommand(() => bot.changeFormat(param["-f"]));
                if (isSet(param, "-c"))
                {
                    int num;
                    if(!int.TryParse(param["-c"], out num)) { help(args[0]); return; }
                    botUseCommand(() => bot.setContinuousBattles(num));
                }
                botUseCommand(() => bot.changeState(State.SEARCH));
            }
            else if (args[0] == "refresh" || args[0] == "rf")
            {
                botUseCommand(() => bot.Refresh());
            }
            else if (args[0] == "update")
            {
                checkForNewVersion();
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
            else if (args[0] == "challengeplayer" || args[0] == "challenge" || args[0] == "cp")
            {
                if (isSet(param,"-f"))
                {
                    botUseCommand(() => bot.changeFormat(param["-f"]));
                }

                if (isSet(param, "-c") && isSet(param, "-u"))
                {
                    int num;
                    if (!int.TryParse(param["-c"],out num)) { help(args[0]); return; }
                    botUseCommand(() => bot.setContinuousBattles(num));
                    botUseCommand(() => bot.challenge(param["-u"]));
                }
                else if (isSet(param, "-c"))
                {
                    int num;
                    if (!int.TryParse(param["-c"], out num)) { help(args[0]); return; }
                    botUseCommand(() => bot.setContinuousBattles(num));
                    botUseCommand(() => bot.challenge(bot.getOwner()));
                }
                else if (isSet(param, "-u")) { botUseCommand(() => bot.challenge(param["-u"])); }
                else if (args.Length == 2) { botUseCommand(() => bot.challenge(args[1])); }
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
            else if (args[0] == "tb")
            {
               // botUseCommand(() => bot.testBattle(), true);
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
                if (isSet(param, "-m"))
                {
                    string a = param["-m"];
                    a = a.Replace('_', ' ');
                    Move m = Global.moveLookup(a);
                    write(m.name + " (" + m.group + "):" + m.type.value + ", " + m.bp + ", " + (m.accuracy*100) + "%\n" + m.desc);
                    var_dump(m);
                }
                else if (isSet(param, "-p"))
                {
                    string a = param["-p"];
                    a = a.Replace('=', '-');
                    Pokemon p = Global.lookup(a);
                    write(p.name + ": " + p.type1.value + "/" + p.type2.value + "\nTypically " + p.getRoleToString() + " with " + p.getDefTypeToString() + " defenses.");
                    writef("Debug Info:\n" + p.statSpread.ToString(), "debug", COLOR_OK);
                    var_dump(p);
                }
                else
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
            }
            else if (args[0] == "forfeit")
            {
                botUseCommand(() => bot.botForfeit());
            }
            else if (args[0] == "clear" || args[0] == "cls")
            {
                if (isSet(param, "-e"))
                {
                    if (File.Exists(ERRLOGPATH))
                    {
                        File.Delete(ERRLOGPATH);
                        writef("Cleared error log.", "system", COLOR_SYS);
                    }
                }
                else
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
