using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.IO;
namespace ShowdownBot
{

    class ReplayLearner
    {
        IWebDriver browser;

        Consol c;
        public ReplayLearner(IWebDriver b, Consol con)
        {
            browser = b;
            c = con;

        }
        //todo: see if there's a way to filter by elo rank to weed out the trash replays
        public void download(int number = 1)
        {
            browser.Navigate().GoToUrl("https://replay.pokemonshowdown.com");
            System.Threading.Thread.Sleep(5000);
            browser.FindElement(By.Name("format")).SendKeys("ou");
            browser.FindElement(By.Name("format")).Submit();
            IList<IWebElement> list = browser.FindElements(By.XPath("//ul[@class='linklist']/li"));
            list[0].Click(); //click the button here to load the replay frame for future use.
            list = browser.FindElements(By.XPath("//ul[@class='linklist']/li"));
            /* Download capabilities are limited with selenium so there are two options:
             * First and easiest is to configure the firefox profile to automatically save the file
             * instead of prompting the user's decision.
             * 
             * Second is to get the generated URL from clicking the download button, navigate to it
             * (which returns a plaintext webpage with the HTML), copy it into memory and then use
             * NET functions to save it in the appropriate place.
             * 
             * The second option is more user-friendly, however implementing it may prove difficult
             * at the moment. For now, let's assume the profile will automatically send the file where
             * it needs to be.
             */

            for (int i = 0; i < number; i++)
            {

                list[i].Click();
                System.Threading.Thread.Sleep(2000);
                IWebElement dlb = browser.FindElement(By.PartialLinkText("Download"));
                dlb.Click();
                c.writef("Downloaded " + browser.Url, "replaymanager", Global.botInfoColor);
                //see note above
            }
            c.writef("Done!", "replaymanager", Global.okColor);
        }
        public void learn()
        {
            string path = @"./rpdata/";
            string[] files = Directory.GetFiles(@"./rpdata/");
            string[] fileContents = new string[files.Length];
            if (!Directory.Exists(path + @"old/"))
            {
                Directory.CreateDirectory(path + @"old/");
            }
            for (int i = 0; i < files.Length; i++)
            {
                using (StreamReader sr = new StreamReader(files[i]))
                {
                    string contents = sr.ReadToEnd();
                    fileContents[i] = contents;
                }

            }

            for (int i = 0; i < fileContents.Length; i++)
            {
                //split the html by the script tag, which will give us the battle log in the second array position (1)
                string log = fileContents[i].Split(new string[] { "<script" }, StringSplitOptions.None)[1];

                //split the log by line feed (ascii 10)
                string[] cleanLog = log.Split((char)10);
                runThroughLog(cleanLog);
                string fn = Path.GetFileName(files[i]);
                File.Move(files[i], path + @"old/" + fn);
                c.write("Processed " + fn + "");
            }
            c.writef("Done!", "replayprocessor", Global.okColor);
        }

        private void runThroughLog(string[] log)
        {
            List<string> movelist = new List<string>();
            List<string> switchlist = new List<string>();

            //collect all information regarding moves made.
            for (int i = 0; i < log.Length; i++)
            {
                if (log[i].Contains("|move|"))
                {
                    movelist.Add(log[i]);
                }
                else if (log[i].Contains("|switch|") || log[i].Contains("|drag|"))
                {
                    switchlist.Add(log[i]);
                }
            }

            using (StreamWriter sw = new StreamWriter(Global.DBPATH, true))
            {

                for (int i = 0; i < movelist.Count; i++)
                {
                    sw.WriteLine(parseMove(movelist[i], switchlist));
                }
            }

        }

        /// <summary>
        /// Takes a string and formats it to a move-target database entry.
        /// </summary>
        /// <returns></returns>
        private string parseMove(string s, List<string> switchlist)
        {
            string[] working = s.Split('|');
            string p1 = working[2].Split(' ')[1];
            p1 = getRealName(p1, switchlist);
            string move = working[3];
            string p2 = working[4].Split(' ')[1];
            p2 = getRealName(p2, switchlist);

            //now concat it alltogether into an easily readable string
            return p1.ToLower() + "|" + move + "|" + p2.ToLower();

        }
        private string getRealName(string name, List<string> switchlist)
        {
            string info = switchlist.Find(x => x.Contains(name));
            info = info.Split('|')[3];
            string names = info.Split(',')[0];
            return names;
        }

    }
}
