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
