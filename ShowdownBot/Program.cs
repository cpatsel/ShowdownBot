using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using static ShowdownBot.GlobalConstants;
using static ShowdownBot.Global;
using OpenQA.Selenium;

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
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleException);
            Console.Title = SDB_TITLEBAR;
            BotConsole c = new ShowdownBot.BotConsole();


            //Application.Run(new Consol());
            

        }



        private static void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            logError(ex, true);   
        }

    }
}
