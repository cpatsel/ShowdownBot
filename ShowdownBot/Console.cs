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
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
using System.Threading.Tasks;

namespace ShowdownBot
{
    public partial class Consol : Form
    {
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

        /// <summary>
        /// Legacy write functions.
        /// </summary>
        /// <param name="t"></param>
        public void write(string t)
        {
            cwrite(t);
        }
        public void writef(string t, ConsoleColor c)
        {
            cwrite(t, c);
        }
        public void writef(string t, string header, ConsoleColor c)
        {
            cwrite(t, header, c);
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
        
        private Dictionary<string,string> parseCmdArgs(string[] args)
        {
            int length = args.Length;
            Dictionary<string, string> kvp = new Dictionary<string, string>();
            for (int i = 1; i < length; i++)
            {
                if (args[i].Contains("-"))
                {
                    try
                    {
			if (hasValidExtraValue(args,i))
                        	kvp.Add(args[i], args[i + 1]);
			else
				kvp.Add(args[i],"");
                    }
                    catch (ArgumentNullException)
                    {
                        cwrite("Bad argument"+args[i+1], COLOR_ERR);
                    }
                    catch (ArgumentException)
                    {
                        //do something else
                        cwrite("Bad argument" + args[i + 1], COLOR_ERR);
                    }
                }
            }
            return kvp;
        }

	private bool hasValidExtraValue(string[] args, int index)
	{
		if (index+1 >= args.Length)
			return false;
	        if (args[index+1].Contains('-'))
			return false;
		return true;
	}

        private bool isSet(Dictionary<string,string> args,string flag)
        {
            if (!flag.StartsWith("-"))
                flag.Insert(0, "-");
            if (args.ContainsKey(flag))
                return true;
            else return false;
        }
        private void botUseCommand(Action cmd, bool startup = false)
        {
            //Only allow the start, startf commands to be run when no bot is live.
            if (!startup)
            {
                if (bot.getStatus())
                    Task.Factory.StartNew(cmd);
                else
                    cwrite("No bot is running!", COLOR_WARN);
            }
            else
                Task.Factory.StartNew(cmd);

            /* ts = new ThreadStart(cmd);
             threadBot = new Thread(ts);
             threadBot.SetApartmentState(ApartmentState.STA);
             try
             {
                 threadBot.Start();
             }
             catch (Exception e)
             {
                 writef(e.ToString(), COLOR_ERR);
             }*/
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
