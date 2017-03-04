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

        Dictionary<string, string> options;
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
            modeCurrent = AiMode.RANDOM; 
            Global.pokedex = new Dictionary<string, Pokemon>();
            options = new Dictionary<string, string>();
            ReadFile();
            Global.setupTypes();
            BuildPokedex();
            Global.moves = new Dictionary<string,Move>();
            movelist = new Movelist();
            movelist.initialize();
            

            
           
        }

        public State getState() { return mainModule.getState(); }
        public AiMode getMode() { return modeCurrent; }

        /// <summary>
        /// Returns whether the bot is running or not
        /// </summary>
        /// <returns></returns>
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
                ChromeOptions options = new ChromeOptions();
                ChromeDriverService svc = ChromeDriverService.CreateDefaultService(@"./cd/");
                svc.SuppressInitialDiagnosticInformation = true;

                options.AddArgument(CD_ARGS);
                options.AddArgument("user-data-dir=" + USERDATA_PATH);
                options.AddArgument("--profile-directory=" + PROFILE_NAME);
                mainBrowser = new ChromeDriver(svc,options);
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
        public void changeMode(AiMode nmode, bool silent = false)
        {
            if (!silent)
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
            //Do a quick error check for randombattle and ou
            //This allows users to type in ou and randombattle, and format it to
            //the now named gen7ou and gen7randombattle respectively
            String f = nf.ToLower();
            if (f == "ou" || f == "randombattle")
                f = "gen7" + f;
            cwrite("Changing format to "+f.ToLower());
            mainModule.changeFormat(f.ToLower());
        }

        public void testBattle()
        {
            BattlePokemon off = new BattlePokemon(Global.lookup("my_magcargo"));
            BattlePokemon def = new BattlePokemon(Global.lookup("bronzong"));
            List<BattlePokemon> defteam = new List<BattlePokemon>();
            defteam.Add(def);
            def.mon.certainAbility = "levitate";
            Move m1 = Global.moveLookup("Earthquake");
            Move m2 = Global.moveLookup("Fire Blast");
            def.setHealth(100);
            int dmg1 = off.rankMove(m1, def,defteam, LastBattleAction.ACTION_ATTACK_SUCCESS, Weather.NONE);
            int dmg2 = off.rankMove(m2, def, defteam, LastBattleAction.ACTION_ATTACK_SUCCESS, Weather.NONE);
            cwrite(off.mon.name + "'s " + m1.name + " against " + def.mon.name+":"+dmg1, "debug", COLOR_BOT);
            cwrite(off.mon.name + "'s " + m2.name + " against " + def.mon.name + ":" + dmg2, "debug", COLOR_BOT);

        }
        public bool botForfeit()
        {
            if (mainModule.getState() == State.BATTLE)
            {
                mainModule.changeState(State.FORFEIT);
                return true;
            }
            else
            {
                cwrite("No battle to forfeit!",COLOR_WARN);
                return false;
            }
        }
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
        public void Start(string u, string p)
        {
            if (isRunning)
            {
                cwrite("Bot is already running!");
                return;
            }

            string oldu = username;
            string oldp = password;
            username = u;
            password = p;
            if (u.Length >= 19)
            {
                username = username.Substring(0, 18);
                cwrite("Username too long, truncating.", COLOR_WARN);
            }
                
            cwrite("Starting bot with credentials:\n" + username + "\nPW:" + password,"system",COLOR_SYS);
            isRunning = true;
            initialise();
            
            
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
            {
                owner = val;
                if (owner.Length >= 19)
                {
                    owner = owner.Substring(0, 18);
                    cwrite("Owner username too long, truncating.", COLOR_WARN);
                }
            }

            else if (key == "[USERNAME]")
            {
                username = val;
                if (username.Length >= 19)
                {
                    username = username.Substring(0, 18);
                    cwrite("Username too long, truncating.", COLOR_WARN);
                }
            }

            else if (key == "[PASSWORD]")
                password = val;
            else if (key == "[USERDATA_PATH]")
                Global.USERDATA_PATH = val;
            else if (key == "[PROFILE]")
                Global.PROFILE_NAME = val;
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
            else if (key == "[CHROMEARGS]")
            {
                string path = @"" + val;
                if (File.Exists(path))
                {
                    using (var reader = new StreamReader(path))
                    {
                        CD_ARGS = reader.ReadToEnd();
                    }
                }
                else
                {
                    cwrite("No chromedriver argument file found at path "+path+". Check that the path in botInfo.txt is correct.", "warning", COLOR_WARN);
                    cwrite("Continuing with default logging-level=3", COLOR_OK);
                }
            }
            else if (key == "[DEFAULT_MODULE]")
            {
                val = val.ToLower();
                if (val == "r" || val == "random")
                    changeMode(AiMode.RANDOM, true);
                else if (val == "b" || val == "biased")
                    changeMode(AiMode.BIAS, true);
                else if (val == "a" || val == "analytic")
                    changeMode(AiMode.ANALYTIC, true);
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
            else if (key.Contains("UPDATE_ONSTART"))
            {
                if (val.ToLower().Contains("true"))
                    Global.updateOnStart = true;
                else
                    Global.updateOnStart = false;
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

        public void Refresh() { mainBrowser.Navigate().Refresh(); }
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
            
            using (var reader = new StreamReader(Global.POKEBASEPATH))
            {
                string json;
                json = reader.ReadToEnd();
                JObject jo = JsonConvert.DeserializeObject<JObject>(json);
                string allmons = jo.First.ToString();
                var current = jo.First;
                for (int i = 0; i < jo.Count; i++)
                {
                    
                    PokeJSONObj pk = JsonConvert.DeserializeObject<PokeJSONObj>(current.First.ToString());
                    Pokemon mon = new Pokemon(pk);
                    Global.pokedex.Add(mon.name, mon);
                    current = current.Next;

                }
            }
            //cwrite("Pokedex built!", COLOR_OK);

        }

        public void learn(int number)
        {
            cwrite("Initiating learning mode.", "bot", COLOR_BOT);
            isRunning = true;
            //bool isLearning = true; //uncommented for now.
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
  
            //isLearning = false;
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

        
       
    }//End of Class

}


