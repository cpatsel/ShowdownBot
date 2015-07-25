using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace ShowdownBot
{

     


    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
           // Thread mainThread = new Thread(new ThreadStart(Application.Run(new Consol())));
            //Consol cmd = new Consol();
            Application.Run(new Consol());
           // new Thread(() => Application.Run(new Consol())).Start();
            

            //Thread thread = new Thread(() => Application.Run(cmd));
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            //while (true)
            //{
            //    string line = Console.ReadLine();

            //    // Action updateText = () => label.Text = line;
            //    //cmd.Invoke(new Action(() => cmd.Parse(line)));

            //}
            
        }

        //private void startAndLoop()
        //{
        //    Consol cmd = new Consol();
        //    new Thread(() => Application.Run(cmd)).Start();
        //    while (true)
        //    {
        //        string line = Console.ReadLine();

        //       // Action updateText = () => label.Text = line;
        //        cmd.Invoke(new Action (() => cmd.Parse(line)) );
        //    }
        //}
    }
}
