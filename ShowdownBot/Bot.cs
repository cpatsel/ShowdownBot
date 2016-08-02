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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ShowdownBot.GlobalConstants;
using static ShowdownBot.Global;
using ShowdownBot.modules;

namespace ShowdownBot
{
    class Bot
    {
        #region Bot Info

        string site = "http://play.pokemonshowdown.com";
        string username = "No Username Set!"; 
        string password;
        string owner;
        
        
        // ///Site Info
        string LoginButton = "login";
        string nameField = "username";
        string passwordField = "password";
        // ///Vars

        string challengee;
        AiMode modeCurrent;
        IWebDriver mainBrowser;
        Movelist movelist;
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
        public Consol getConsole(){ return c;}
        public string getOwner() { return owner;}
        public string getChallengee() { return challengee;}

        public void printInfo()
        {
            cwrite("\nCurrent module: " + getMode().ToString() + "\n" +
                "Current state: " + getState().ToString(),"bot", COLOR_BOT);
            mainModule.printInfo();
        }


        public void initialise(bool browser=true)
        {
            if (browser)
            {
                FirefoxProfileManager pm = new FirefoxProfileManager();
                FirefoxProfile ffp = pm.GetProfile(Global.FF_PROFILE);
                FirefoxOptions fo = new FirefoxOptions();
                fo.SetLoggingPreference(LogType.Driver, LogLevel.Off); //todo allow toggling this
                fo.SetLoggingPreference(LogType.Client, LogLevel.Off);
                
                mainBrowser = new FirefoxDriver(ffp);
            
                mainBrowser.Manage().Window.Maximize(); //prevent unintenttionally hiding elements in some versions of FF
                
                gBrowserInstance = mainBrowser;
                DesiredCapabilities d = new DesiredCapabilities();
            }

            analyticModule = new AnalyticModule(this, mainBrowser);
            randomModule = new RandomModule(this, mainBrowser);
            biasedModule = new BiasedModule(this, mainBrowser);
            compareModule = new ComparativeModule(this, mainBrowser);

        }
        public void changeState(State nstate)
        {
            cwrite("Changing state to: " + nstate.ToString());
            mainModule.changeState(nstate);
            

        }
        public void changeMode(AiMode nmode)
        {
            cwrite("Changing AI mode from " + modeCurrent.ToString() + " to: " + nmode.ToString());
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
        public void setContinuousBattles(int max)
        {
            mainModule.setContinuous(true);
            mainModule.setMaxBattles(max);
        }
        public void changeFormat(string nf)
        {
            cwrite("Changing format to "+nf.ToLower());
            mainModule.changeFormat(nf.ToLower());
        }
        public bool botForfeit() { return mainModule.forfeitBattle(); }
        public void Start(bool auth)
        {

            if (isRunning)
            {
                cwrite("Bot is already running!", COLOR_WARN);
                return;
            }
            isRunning = true;
            initialise();

            if (auth)
            {
                cwrite("Starting bot", "system", COLOR_SYS);
                if (!OpenSite(site))
                {
                    cwrite("Failed to open "+site, "[ERROR]", COLOR_ERR);
                    isRunning = false;
                    return;
                }
                else
                    Update();
            }
            else
            {
                cwrite("Starting bot without authentication...", "[SYSTEM]", COLOR_SYS);
                if (!OpenSiteNoAuth(site))
                {
                    cwrite("Failed to open " + site, "error", COLOR_ERR);
                    isRunning = false;
                    return;
                }
                else
                    Update();
            }


        }
        public void Start(string u,string p)
        {
            if (isRunning)
            {
                cwrite("Bot is already running!");
                return;
            }
            cwrite("Starting bot with credentials " + u + "\tPW:" + p,"system",COLOR_SYS);
            isRunning = true;
            initialise();
            //TODO: do we need to hold onto this information?
            string tempu = username;
            string tempp = password;
            username = u;
            password = p;
            
            if (!OpenSite(site))
            {
                cwrite("Failed to open " + site, "error", COLOR_ERR);
                isRunning = false;
                return;
            }
            else
            {
                Update();
            }
        }
        public void Kill()
        {
            if (!isRunning)
            {
                cwrite("Bot is not running!", COLOR_WARN);
                return;
            }
            cwrite("Bot is shutting down.");

            if (mainModule.getState() == State.BATTLE)
                mainModule.forfeitBattle();

            isRunning = false;
            closeBrowser();
            
            
        }
        public void challenge(string p)
        {
            challengee = p;
            changeState(State.CHALLENGE);
        }
        public void closeBrowser()
        {
            mainBrowser.Quit();
        }

        private void ReadFile()
        {
          
            if (!File.Exists("botInfo.txt"))
            {
                cwrite("Could not load config (maybe it's missing?)", "[ERROR]", COLOR_ERR);
                cwrite("You can try starting without authenticating (startf)", COLOR_WARN);
                return;
            }
            System.Threading.Thread.Sleep(1000);
            using (var reader = new StreamReader("botInfo.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //ghetto commenting, avoid using inline comments :^)
                    if (!line.StartsWith("#") && line != "")
                    {
                        string[] configparams = line.Split('=');
                        setInitVars(configparams[0], configparams[1]);
                    }
                }
            }
            
            cwrite("Bot's owner set to: " + owner, "[DEBUG]", COLOR_OK);
        }

        private void setInitVars(string key, string val)
        {
            if (key == "[OWNER]")
                owner = val;
            else if (key == "[USERNAME]")
                username = val;
            else if (key == "[PASSWORD]")
                password = val;
            else if (key == "[PROFILE]")
                Global.FF_PROFILE = val;
            else if (key == "[SHOW_DEBUG]")
            {
                val = val.ToLower();
                if (val == "false")
                    Global.showDebug = false;
                else if (val == "true")
                    Global.showDebug = true;
                else
                    cwrite("Unknown value " + val + " for SHOW_DEBUG", "WARNING", COLOR_WARN);
            }
            else if (key.Contains("UNKNOWN_PKMN"))
            {
                bool result;
                if (bool.TryParse(val, out result))
                    ADD_U_PKMN = result;
                else
                {
                    cwrite("Unknown value " + val + "for ADD_UNKNOWN_PKMN", "warning", COLOR_WARN);
                }
            }
            else if (key.StartsWith("[SLOT"))
            {
                if (key.Contains('1'))
                    Global.m1wgt = float.Parse(val);
                else if (key.Contains('2'))
                    Global.m2wgt = float.Parse(val);
                else if (key.Contains('3'))
                    Global.m3wgt = float.Parse(val);
                else
                    Global.m4wgt = float.Parse(val);
            }
        }
        private void Update()
        {
            
            mainModule = randomModule;
            cwrite("Ready.");
            while (isRunning)
            {
                mainModule.Update();
            }
            cwrite("Done performing tasks.", COLOR_OK);
        }

        

        private bool OpenSite(string site)
        {
            cwrite("Opening site");
            mainBrowser.Navigate().GoToUrl(site);
            if (!waitFindClick(By.Name(LoginButton))) return false;
            wait();
            mainBrowser.FindElement(By.Name(nameField)).SendKeys(username);
            mainBrowser.FindElement(By.Name(nameField)).Submit();
            if (password != null)
            {
                if (!waitUntilElementExists(By.Name(passwordField))) return false;
                mainBrowser.FindElement(By.Name(passwordField)).SendKeys(password);
                mainBrowser.FindElement(By.Name(passwordField)).Submit();
            }
            return true;
           
        }
        private bool OpenSiteNoAuth(string site)
        {
            cwrite("Opening site");
            mainBrowser.Navigate().GoToUrl(site);
            cwrite("Opened site, skipping authentication steps.");
            cwrite("Moving onto next task");
            wait(5000);
            return true;
        
        }
        public bool CanConnect()
           {
            var ping = new Ping();
            var reply = ping.Send(site);
            return reply.Status == IPStatus.Success;
           }


        private void BuildPokedex()
        {
            cwrite("Building pokedex, this may take a moment...");
            if (!File.Exists(Global.POKEBASEPATH))
            {
                cwrite("Could not find pokedex.js","[ERROR]",COLOR_ERR);
                cwrite("Analytic battle mode will not work correctly.",COLOR_WARN);
                cwrite("Continuing operation.",COLOR_OK);
                return;
            }
            string json;
            using (var reader = new StreamReader(Global.POKEBASEPATH))
            {
                while ((json = reader.ReadLine()) != null)
                {
                    string full = "{" + json + "}";
                    JObject po = JsonConvert.DeserializeObject<JObject>(full);
                    var first = po.First;
                    PokeJSONObj obj = JsonConvert.DeserializeObject<PokeJSONObj>(po.First.First.ToString());

                }
            }
            cwrite("Pokedex built!", COLOR_OK);

        }

        public void learn(int number)
        {
            cwrite("Initiating learning mode.", "bot", COLOR_BOT);
            isRunning = true;
            bool isLearning = true;
            if (number > 0)
                initialise();
            ReplayLearner rl = new ReplayLearner(mainBrowser,c);

            if (number > 0)
            {
                cwrite("Now downloading " + number.ToString() + " replays.", "bot", COLOR_BOT);
                rl.download(number);
            }
            else
            {
                rl.learn();
            }
  
            isLearning = false;
            cwrite("Stopping learning processes.", "bot", COLOR_BOT);
        }


        public void simulate(string you, string enemy)
        {
            if (!isRunning)
                initialise(false);
            if (!compareModule.setup)
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
                cwrite("No browser is running!", "error", COLOR_ERR);
                return;
            }
            
            if (!Directory.Exists(@"./logs"))
            {
                Directory.CreateDirectory(@"./logs");
            }
            string date = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fn = @"./logs/" + date + ".txt";
            IList<LogEntry> list = mainBrowser.Manage().Logs.GetLog(LogType.Browser);
            using (StreamWriter sw = new StreamWriter(fn))
            {
               
                for (int i = 0; i < list.Count; i++)
                {
                    sw.WriteLine(list[i].Message);
                }
                sw.Close();
            }
            cwrite("log_" + date + ".txt created.");

        }
       
    }//End of Class

}


