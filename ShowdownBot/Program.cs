﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

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
            Console.Title = Global.TITLEBAR;
            Application.Run(new Consol());
            

        }



        private static void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            using (StreamWriter sw = new StreamWriter("error.txt",true))
            {
               
                sw.WriteLine("----------");
                sw.WriteLine("["+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"]");
                sw.WriteLine("ERROR:"+ex.Message);
                sw.WriteLine(ex.StackTrace);
                
            }
            MessageBox.Show("A fatal error has occured. See error.txt for more info.");

            
        }

    }
}
