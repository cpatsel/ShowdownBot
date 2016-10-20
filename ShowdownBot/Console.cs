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
using System.Net;
using System.Xml.Linq;

namespace ShowdownBot
{
    public partial class Consol
    {
        Bot bot;
        bool ready = false;
        Thread threadBot;
        ThreadStart ts;
        XDocument helpdoc;
        public Consol()
        {
            ts = new ThreadStart(() => bot = new Bot(this));
            threadBot = new Thread(ts);
            threadBot.SetApartmentState(ApartmentState.STA);
            threadBot.Start();
            helpdoc = XDocument.Load(HELPPATH);
            if (Global.updateOnStart)
                this.checkForNewVersion();
            write("Console initialized.");
            Consol_Load();
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
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Action job = (Action)e.Argument;
            job();
        }

        private void Consol_Load()
        {
            
            while (true)
            {
                string line = Console.ReadLine();
                Parse(line);
            }
        }

        public bool checkForNewVersion()
        {
            WebClient wc = new WebClient();
            string versiontext;
            try
            {
                versiontext = wc.DownloadString(VERSIONFILE_URL).TrimEnd('\n');
            }
            catch
            {
                cwrite("Unable to connect to github.", "error", COLOR_ERR);
                return false;
            }
            string[] components = versiontext.Split('.');
            string[] mycomponents = SDB_VERSION.TrimEnd("-unreleased".ToCharArray()).Split('.');
            bool behind = false;
            ConsoleColor color = GlobalConstants.COLOR_OK;
            if (Int32.Parse(components[0]) > Int32.Parse(mycomponents[0]))
            {
                behind = true;
                color = COLOR_ERR;
            }
            else if (Int32.Parse(components[1]) > Int32.Parse(mycomponents[1]))
            {
                behind = true;
                color = COLOR_WARN;
            }
            else if ((Int32.Parse(components[2]) > Int32.Parse(mycomponents[2]))
                && (Int32.Parse(components[1]) == Int32.Parse(mycomponents[1])))
            {
                behind = true;
                color = COLOR_WARN;
            }
            if (behind)
            {
                cwrite("There's a new version v" + versiontext + " available!","updater", color);
            }
            else
                cwrite("You have the latest version!","updater", COLOR_OK);
            return true;
        }
    }
}
