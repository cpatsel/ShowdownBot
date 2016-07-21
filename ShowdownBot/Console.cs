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


        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
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
                writef(e.ToString(), COLOR_ERR);
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
