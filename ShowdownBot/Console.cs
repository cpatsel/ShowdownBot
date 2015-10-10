using System;
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
            //temp = f;
            //bot = new Bot(this);
            ts = new ThreadStart(() => bot = new Bot(this));
            threadBot = new Thread(ts);
            threadBot.SetApartmentState(ApartmentState.STA);
            threadBot.Start();
           // threadBot.Join();
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

            string date = GetDate();
            Console.Write("[" + date + "]");
            Console.ForegroundColor = c;
            header = header.Trim('[',']').ToUpper();
           if ((!Global.showDebug) && (header == "DEBUG"))
            { Console.ResetColor(); return; }
            Console.Write("["+header+"]");
            Console.ResetColor();
            Console.Write(t +"\n");
            
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
                writef(input,"[USER]",Global.defaultColor);
                Parse(input);
            }
        }


        public void Parse(string t)
        {
            string[] args; 
            args = Regex.Split(t," ");
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
                else if (args[0] == "kill")
                {

                    writef("Killing bot.", "[SYSTEM]", Global.sysColor);
                    cmd = () => bot.Kill();
                    ts = new ThreadStart(cmd);
                    threadBot = new Thread(ts);
                    threadBot.SetApartmentState(ApartmentState.STA);
                    threadBot.Start();
                }
                else if ( t == "help")
                {
                    DisplayHelp();
                   
                }
                else if (args[0] == "cs" || args[0] == "changestate")
                {
                    if (args[1] != null)
                    {
                        if (args[1] == "idle")
                        {
                            cmd = () => bot.changeState(ShowdownBot.Bot.State.IDLE);
                            ts = new ThreadStart(cmd);
                            threadBot = new Thread(ts);
                            threadBot.SetApartmentState(ApartmentState.STA);
                            threadBot.Start();
                        }
                        else if (args[1] == "randombattle" || args[1] == "rb")
                        {
                            cmd = () => bot.changeState(ShowdownBot.Bot.State.RANDOMBATTLE);
                            ts = new ThreadStart(cmd);
                            threadBot = new Thread(ts);
                            threadBot.SetApartmentState(ApartmentState.STA);
                            threadBot.Start();
                        }
                        else if (args[1] == "overused" || args[1] == "ou")
                        {
                            cmd = () => bot.changeState(ShowdownBot.Bot.State.BATTLEOU);
                            ts = new ThreadStart(cmd);
                            threadBot = new Thread(ts);
                            threadBot.SetApartmentState(ApartmentState.STA);
                            threadBot.Start();
                        }
                    }
                    else
                    {
                        writef("Acceptable arguments are idle, ou, random", "[SYSTEM]", Global.sysColor);
                    }
                }
                else if (args[0] == "m" || args[0] == "mode")
                {
                    if (args[1] != null)
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
                    else
                    {
                        writef("Acceptable arguments are idle, ou, random", "[SYSTEM]", Global.sysColor);
                    }
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
                    //temp.Close();
                    Environment.Exit(0);
                    this.Close();



                }
                else if (args[0] == "info")
                {
                    write("Bot is running:" + bot.getStatus().ToString() + "\n"
                        + "Current state: " + bot.getState().ToString() + "\n"
                        + "Current mode: " + bot.getMode().ToString() + "\n"
                        
                        );
                }
                else if (args[0] == "visible" || args[0] == "v")
                {
                    //change visibility
                }
                else
                {
                    writef("Undefined command " + t, "[SYSTEM]", Global.sysColor);

                }

            }
        
        private void DisplayHelp()
        {
          
            writef("Available commands are: start, startf, exit, changestate, info", "[SYSTEM]", Global.sysColor);
            
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
