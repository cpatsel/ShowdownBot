using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using System.Collections;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Firefox;
using ShowdownBot.modules;

namespace ShowdownBot
{
    class Bot
    {
        #region Bot Info

        string site = "http://play.pokemonshowdown.com";
        string username = "No Username Set!"; 
        string password;
        string owner; //More uses for this later, right now it's used to initiate the challenge.
        
        Dictionary<string,Pokemon> pokedex;
        // ///Site Info
        string LoginButton = "login";
        string nameField = "username";
        string passwordField = "password";
        // ///Vars
        int loginAttempts;
        
        AiMode modeCurrent;
        IWebDriver mainBrowser;
        Movelist movelist;
        bool needLogout, isLoggedIn;
        bool isRunning;

        BotModule mainModule;
        BotModule analyticModule;
        BotModule biasedModule;
        BotModule randomModule;
        ComparativeModule compareModule;
        //////////////
        //Bot states
        
        //Determines AI
        public enum AiMode
        {
            RANDOM,
            BIAS,
            ANALYTIC
        };

        
        #endregion

        Consol c;
        public Bot(Consol c)
        {
            this.c = c;
            isRunning = false;
            //activeState = State.IDLE;
            modeCurrent = AiMode.RANDOM; //TODO: set default in config to be read
            Global.pokedex = new Dictionary<string, Pokemon>();
            ReadFile();
            Global.setupTypes();
            BuildPokedex();
            Global.moves = new Dictionary<string,Move>();
            movelist = new Movelist();
            movelist.initialize();

            
           
        }

        public State getState() { return mainModule.getState(); }
        public AiMode getMode() { return modeCurrent; }
        public bool getStatus() { return isRunning; }


        public void initialise(bool browser=true)
        {
            if (browser)
            {
                FirefoxProfileManager pm = new FirefoxProfileManager();
                FirefoxProfile ffp = pm.GetProfile("sdb");
                mainBrowser = new FirefoxDriver(ffp);

                DesiredCapabilities d = new DesiredCapabilities();
            }

            analyticModule = new AnalyticModule(this, mainBrowser);
            randomModule = new RandomModule(this, mainBrowser);
            biasedModule = new BiasedModule(this, mainBrowser);
            compareModule = new ComparativeModule(this, mainBrowser);

        }
        public void changeState(State nstate)
        {
            c.write("Changing state to: " + nstate.ToString());
            mainModule.changeState(nstate);
            

        }
        public void changeMode(AiMode nmode)
        {
            c.write("Changing AI mode from " + modeCurrent.ToString() + " to: " + nmode.ToString());
            modeCurrent = nmode;
            switch (modeCurrent)
            {
                case AiMode.ANALYTIC:
                    {
                        mainModule = analyticModule;
                        break;
                    }
                case AiMode.RANDOM:
                    {
                        mainModule = randomModule;
                        break;
                    }
                case AiMode.BIAS:
                    {
                        mainModule = biasedModule;
                        break;
                    }
            }
        }

        public void Start(bool auth)
        {
            if (isRunning)
            {
                c.writef("Bot is already running!", Global.warnColor);
                return;
            }
            loginAttempts = 0;
            isRunning = true;
            c.write("Opening site.");
            initialise();

            if (auth)
            {
                if (!OpenSite(site))
                {
                    c.writef("Failed to initiate bot.", "[ERROR]", Global.errColor);
                }
            }
            else
            {

                OpenSiteNoAuth(site);
            }


        }
        //TODO: add checks to other battle methods to break out of loop if bot is no longer running.
        public void Kill()
        {
            if (!isRunning)
            {
                c.writef("Bot is not running!", Global.warnColor);
                return;
            }
            if (needLogout)
            {
              //  Logout();
            }
            c.write("Bot is shutting down.");
           // changeState(State.BUSY);
            isRunning = false;
           
            
            
        }

        public void closeBrowser()
        {
            mainBrowser.Quit();
        }
        private void ReadFile()
        {
          
            if (!File.Exists("botInfo.txt"))
            {
                c.writef("Could not load config (maybe it's missing?)", "[ERROR]", Global.errColor);
                c.writef("You can try starting without authenticating (startf)", Global.warnColor);
                return;
            }
            System.Threading.Thread.Sleep(1000);
            using (var reader = new StreamReader("botInfo.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //ghetto commenting, avoid using inline comments :^)
                    if (!line.StartsWith("#"))
                    {
                        string[] configparams = line.Split('=');
                        setInitVars(configparams[0], configparams[1]);
                    }
                }
            }
            c.writef("Bot's owner set to: " + owner, "[DEBUG]", Global.okColor);
        }

        private void setInitVars(string key, string val)
        {
            switch (key)
            {
                case "[OWNER]":
                    {
                        owner = val;
                        break;
                    }
                case "[USERNAME]":
                    {
                        username = val;
                        break;
                    }
                case "[PASSWORD]":
                    {
                        password = val;
                        break;
                    }
                case "[SHOW_DEBUG]":
                    {
                        val = val.ToLower();
                        if (val == "false")
                            Global.showDebug = false;
                        else if (val == "true")
                            Global.showDebug = true;
                        else
                            c.writef("Unknown value " + val + " for SHOW_DEBUG", "WARNING", Global.warnColor);
                        break;

                    }

                default:
                    {
                        //No match, do nothing.
                        break;
                    }

            }
        }
        private void Update()
        {
            
            mainModule = analyticModule;
            c.write("Ready.");
            //changeState(State.IDLE);
            while (isRunning)
            {
                mainModule.Update();
            }
            c.writef("Done performing tasks.", Global.okColor);
        }

        

        private bool OpenSite(string site)
        {
            mainBrowser.Navigate().GoToUrl(site);
            mainBrowser.Manage().Timeouts().ImplicitlyWait(System.TimeSpan.FromSeconds(10));
            mainBrowser.FindElement(By.Name(LoginButton)).Click();
            mainBrowser.FindElement(By.Name(nameField)).SendKeys(username);
            mainBrowser.FindElement(By.Name(nameField)).Submit();

            mainBrowser.FindElement(By.Name(passwordField)).SendKeys(password);
            mainBrowser.FindElement(By.Name(passwordField)).Submit();
            
            Update();
            return true;
           
        }
        private bool OpenSiteNoAuth(string site)
        {
            
            mainBrowser.Navigate().GoToUrl(site);
            c.write("Opened site, skipping authentication steps.");
            c.write("Moving onto next task");
            Update(); 
            return true;
        
        }
        public bool CanConnect()
           {
            var ping = new Ping();
            var reply = ping.Send(site);
            return reply.Status == IPStatus.Success;
           }

            public Consol getConsole()
            {
                return c;
            }
            public string getOwner()
            {
                return owner;
            }

        private void BuildPokedex()
        {
            c.write("Building pokedex, this may take a moment...");
            if (!File.Exists(Global.POKEBASEPATH))
            {
                c.writef("Could not find pokebase.txt.","[ERROR]",Global.errColor);
                c.writef("Analytic battle mode will not work correctly.",Global.warnColor);
                c.writef("Continuing operation.",Global.okColor);
                return;
            }
            using (var reader = new StreamReader(Global.POKEBASEPATH))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Pokemon p = new Pokemon(line);
                  //  c.writef("Adding pokemon " + p.name, "[DEBG]", Global.defaultColor);
                    Global.pokedex.Add(p.name, p);
                }
            }
            c.writef("Pokedex built!", Global.okColor);
        }

        public void learn(int number)
        {
            c.writef("Initiating learning mode.", "bot", Global.botInfoColor);
            isRunning = true;
            bool isLearning = true;
            if (number > 0)
                initialise();
            ReplayLearner rl = new ReplayLearner(mainBrowser,c);

            if (number > 0)
            {
                c.writef("Now downloading " + number.ToString() + " replays.", "bot", Global.botInfoColor);
                rl.download(number);
            }
            else
            {
                rl.learn();
            }
  
            isLearning = false;
            c.writef("Stopping learning processes.", "bot", Global.botInfoColor);
        }


        public void simulate(string you, string enemy)
        {
            if (!isRunning)
                initialise(false);
            compareModule.buildDB();
            compareModule.simulate(Global.lookup(you), Global.lookup(enemy));
        }

        /// <summary>
        /// Doesn't do much of anything with firefox unfortunately.
        /// </summary>
        public void saveLog()
        {
            if (!isRunning)
            {
                c.writef("No browser is running!", "error", Global.errColor);
                return;
            }

            if (!Directory.Exists(@"./logs"))
            {
                Directory.CreateDirectory(@"./logs");
            }
            string date = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fn = @"./logs/" + date + ".txt";
            IList<LogEntry> list = mainBrowser.Manage().Logs.GetLog("har");
            using (StreamWriter sw = new StreamWriter(fn))
            {
               
                for (int i = 0; i < list.Count; i++)
                {
                    sw.WriteLine(list[i].Message);
                }
                sw.Close();
            }
            c.write("log_" + date + ".txt created.");

        }
       
    }//End of Class

}


