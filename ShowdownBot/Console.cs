﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;

namespace ShowdownBot
{
    public partial class Consol : Form
    {

        // Form1 temp;
        Bot bot;
        Thread threadBot;
        ThreadStart ts;

        public Consol()
        {

            ts = new ThreadStart(() => bot = new Bot(this));
            threadBot = new Thread(ts);
            threadBot.SetApartmentState(ApartmentState.STA);
            threadBot.Start();
            InitializeComponent();
            richTextBox1.WordWrap = false;
            write("Console initialized.");


        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void write(string t)
        {
            //richTextBox1.AppendText("[" + GetDate() + "]" + t + "\n");
            //richTextBox1.SelectionStart = richTextBox1.TextLength;
            //richTextBox1.ScrollToCaret();
            Console.WriteLine("[" + GetDate() + "]" + t);
            Console.ResetColor();
        }
        public void writef(string t, ConsoleColor c)
        {
            //int l = richTextBox1.TextLength;
            string date = GetDate();
            //richTextBox1.AppendText("[" + date + "]" + t + "\n");

            //richTextBox1.SelectionStart = l;
            //richTextBox1.SelectionLength = t.Length + date.Length + 2;
            //richTextBox1.SelectionColor = c;
            //richTextBox1.ScrollToCaret();
            Console.ForegroundColor = c;
            Console.WriteLine("[" + date + "]" + t);
            Console.ResetColor();

        }
        public void writef(string t, string header, ConsoleColor c)
        {
            header = header.Trim('[', ']').ToUpper();
            if ((!Global.showDebug) && (header == "DEBUG"))
            { Console.ResetColor(); return; }
            string date = GetDate();
            Console.Write("[" + date + "]");
            Console.ForegroundColor = c;
            Console.Write("[" + header + "]");
            Console.ResetColor();
            Console.Write(t + "\n");


        }
        private string GetDate()
        {
            string dt = DateTime.Now.ToString("HH:mm:ss");
            return dt;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string input = textBox1.Text;
                textBox1.Clear();
                writef(input, "[USER]", Global.defaultColor);
                Parse(input);
            }
        }


        public void Parse(string t)
        {
            string[] args;
            args = Regex.Split(t, " ");
            Action cmd = null;

            if (args[0] == "start")
            {

                writef("Starting bot...", "[SYSTEM]", Global.sysColor);
                cmd = () => bot.Start(true);
                ts = new ThreadStart(cmd);
                threadBot = new Thread(ts);
                threadBot.SetApartmentState(ApartmentState.STA);
                threadBot.Start();
            }
            else if (args[0] == "startf")
            {

                writef("Starting bot without authentication...", "[SYSTEM]", Global.sysColor);
                cmd = () => bot.Start(false);
                ts = new ThreadStart(cmd);
                threadBot = new Thread(ts);
                threadBot.SetApartmentState(ApartmentState.STA);
                threadBot.Start();
            }
            else if (args[0] == "stop" || args[0] == "idle")
            {
                botUseCommand(() => bot.changeState(State.IDLE));
            }
            else if (args[0] == "kill")
            {

                writef("Killing bot.", "[SYSTEM]", Global.sysColor);
                cmd = () => bot.Kill();
                ts = new ThreadStart(cmd);
                threadBot = new Thread(ts);
                threadBot.SetApartmentState(ApartmentState.STA);
                threadBot.Start();
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
                writef("ShowdownBot v" + Global.VERSION, "system", Global.sysColor);
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
                else
                    botUseCommand(() => bot.challenge(bot.getOwner()));
            }
            else if (args[0] == "m" || args[0] == "mode" || args[0] == "module")
            {
                if (paramCheck(2, args, "m"))
                {
                    if (args[1] == "random" || args[1] == "r")
                    {
                        cmd = () => bot.changeMode(ShowdownBot.Bot.AiMode.RANDOM);
                        ts = new ThreadStart(cmd);
                        threadBot = new Thread(ts);
                        threadBot.SetApartmentState(ApartmentState.STA);
                        threadBot.Start();
                    }
                    else if (args[1] == "biased" || args[1] == "b")
                    {
                        cmd = () => bot.changeMode(ShowdownBot.Bot.AiMode.BIAS);
                        ts = new ThreadStart(cmd);
                        threadBot = new Thread(ts);
                        threadBot.SetApartmentState(ApartmentState.STA);
                        threadBot.Start();
                    }
                    else if (args[1] == "analytic" || args[1] == "a")
                    {
                        cmd = () => bot.changeMode(ShowdownBot.Bot.AiMode.ANALYTIC);
                        ts = new ThreadStart(cmd);
                        threadBot = new Thread(ts);
                        threadBot.SetApartmentState(ApartmentState.STA);
                        threadBot.Start();
                    }
                }

            }
            else if (args[0] == "dump" || args[0] == "dumplog")
            {
                cmd = () => bot.saveLog();
                ts = new ThreadStart(cmd);
                threadBot = new Thread(ts);
                threadBot.SetApartmentState(ApartmentState.STA);
                threadBot.Start();
            }
            else if (args[0] == "exit")
            {
                writef("Shutting down.", "[SYSTEM]", Global.sysColor);
                writef("Killing bot.", "[SYSTEM]", Global.sysColor);
                cmd = () => bot.Kill();
                ts = new ThreadStart(cmd);
                threadBot = new Thread(ts);
                threadBot.SetApartmentState(ApartmentState.STA);
                threadBot.Start();
                cmd = () => bot.closeBrowser();
                ts = new ThreadStart(cmd);
                threadBot = new Thread(ts);
                threadBot.SetApartmentState(ApartmentState.STA);
                threadBot.Start();
                //temp.Close();
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
                writef("Undefined command " + t, "[SYSTEM]", Global.sysColor);

            }

        }

        private void DisplayHelp()
        {

            writef("Available commands are: challenge, clear, dump, exit, info,\n kill, module, " +
                    "start, startf, version", "[SYSTEM]", Global.sysColor);

        }
        private bool paramCheck(int correctParams, string[] args, string c = "none")
        {
            if (args.Length < correctParams)
            {
                help(c);
                return false;
            }
            else return true;
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

            writef(desc + "\n" +
                    "alias: " + alias + "\n" +
                    "arguments: " + arguments, "system", Global.sysColor);
        }


        private void botUseCommand(Action cmd)
        {
            ts = new ThreadStart(cmd);
            threadBot = new Thread(ts);
            threadBot.SetApartmentState(ApartmentState.STA);
            try
            {
                threadBot.Start();
            }
            catch (Exception e)
            {
                writef(e.ToString(), Global.errColor);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Action job = (Action)e.Argument;
            job();
        }

        private void Consol_Load(object sender, EventArgs e)
        {
            while (true)
            {
                string line = Console.ReadLine();
                Parse(line);
            }
        }
    }
}
