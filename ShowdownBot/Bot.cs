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
        string username; 
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
        float move1Weight = 0.4f;
        float move2Weight = 0.3f;
        float move3Weight = 0.2f;
        float move4Weight = 0.1f;
        bool needLogout, isLoggedIn;
        bool isRunning;

        BotModule mainModule;
        BotModule analyticModule;
        BotModule randomModule;
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
        
        public void changeState(State nstate)
        {
            c.write("Changing state to: " + nstate.ToString());
            mainModule.changeState(nstate);
            

        }
        public void changeMode(AiMode nmode)
        {
            c.write("Changing AI mode from " + modeCurrent.ToString() + " to: " + nmode.ToString());
            modeCurrent = nmode;
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
            //changeState(State.BUSY);
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
            analyticModule = new AnalyticModule(this, mainBrowser);
            randomModule = new RandomModule(this, mainBrowser);
            mainModule = randomModule;
            c.write("Ready.");
            //changeState(State.IDLE);
            while (isRunning)
            {

                //if (modeCurrent == AiMode.ANALYTIC)
                //{
                //    mainModule = analyticModule;

                //}
                //else if (modeCurrent == AiMode.RANDOM)
                //    mainModule = randomModule;

                mainModule.Update();
                //if (activeState == State.IDLE)
                //{
                //    System.Threading.Thread.Sleep(5000);
                //}
                //else if (activeState == State.RANDOMBATTLE)
                //{ 
                //    challengePlayer(owner,"ou");
                //}
                //else if (activeState == State.BATTLEOU)
                //{
                //    challengePlayer(owner,"ou");
                //}
            }
            c.writef("Done performing tasks.", Global.okColor);
        }

        

        private bool OpenSite(string site)
        {
            FirefoxProfileManager pm = new FirefoxProfileManager();
            FirefoxProfile ffp = pm.GetProfile("sdb");
            mainBrowser = new FirefoxDriver(ffp);
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
            FirefoxProfileManager pm = new FirefoxProfileManager();
            FirefoxProfile ffp = pm.GetProfile("sdb");
            mainBrowser = new FirefoxDriver(ffp);
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
       

        

        



       

       


    

        /// <summary>
        /// Determines actions based on the predetermined weight of each moveslot.
        /// </summary>
        /// <param name="b">browser</param>
        /// <param name="turn"></param>
        /// <returns>state of the battle (over/true, ongoing/false)</returns>
        //private bool battleBiased(ref int turn)
        //{
        //     //browser = b;
        //    int moveSelection;
        //    int pokeSelection;

        //    if (checkMove())
        //    {
        //        //WatiN.Core.CheckBox box = browser.CheckBox(Find.ByName("megaevo"));
        //        //if (box.Exists && !box.Checked)
        //        //{
        //        //    c.writef("I'm mega evolving this turn.", Global.botInfoColor);
        //        //    box.Click();
        //        //}

        //        moveSelection = pickMoveBiased();
        //        c.writef("I'm selecting move " + moveSelection.ToString(), "[TURN " + turn.ToString() + "]", Global.botInfoColor);
        //        //browser.Button(Find.ByValue(moveSelection.ToString())).Click(); //Select move
        //        System.Threading.Thread.Sleep(2000);
        //        turn++;
        //    }
        //    else if (checkSwitch())
        //    {
        //        //TODO: check if it's the first turn, and then select appropriate lead.
        //        c.writef("Switching pokemon.", Global.botInfoColor);
        //        pokeSelection = pickPokeRandomly();
        //        c.writef("New pokemon selected: " + pokeSelection.ToString(), Global.botInfoColor);
        //      //  browser.Button(Find.ByValue(pokeSelection.ToString())).Click();
        //        System.Threading.Thread.Sleep(2000);
        //    }
        //    else if (checkBattleEnd())
        //    {

        //        return true;
        //    }
        //    else
        //    {
        //        // c.write("Sleeping for 2 secs");
        //        System.Threading.Thread.Sleep(2000);
        //    }
        //    return false;
        //}

        #region Random Mode

        

        



        #endregion

        #region Biased Mode

            private int pickMoveBiased( )
        {
            // browser = b;
            
            //HashSet<int> exclude = new HashSet<int>();
            //int choice;
            //choice = getIndexBiased();
            //while (!browser.Button(Find.ByValue(choice.ToString())).Exists) 
            //{
            //    //If the move we've chosen does not exist, just cycle through until we get one.
            //    c.writef("Bad move choice: " + choice.ToString() + "Picking another", "[DEBUG]", Global.okColor);
            //    exclude.Add(choice);
            //    choice = GetRandomExcluding(exclude, 1, 4);
            //}

            return 0;//choice;
        }

            /// <summary>
            /// Helper method for pickMoveBiased.
            /// </summary>
            /// <returns>Choice index based on the specified weights.</returns>
            private int getIndexBiased()
            {
                int choice;
                Random rand = new Random();
                float percent = (float)rand.NextDouble();
                if (percent >= 0 && percent <= move4Weight)
                    choice = 4;
                else if (percent > move4Weight && percent <= move3Weight)
                    choice = 3;
                else if (percent > move3Weight && percent <= move2Weight)
                    choice = 2;
                else
                    choice = 1;

                return choice;
            }

        #endregion


            public Consol getConsole()
            {
                return c;
            }
            public string getOwner()
            {
                return owner;
            }

        #region Analytic Functions





        private void BuildPokedex()
        {
            c.write("Building pokedex, this may take a moment...");
            if (!File.Exists("pokebase.txt"))
            {
                c.writef("Could not find pokebase.txt.","[ERROR]",Global.errColor);
                c.writef("Analytic battle mode will not work correctly.",Global.warnColor);
                c.writef("Continuing operation.",Global.okColor);
                return;
            }
            using (var reader = new StreamReader("pokebase.txt"))
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

        

        
       

        private float getRisk( Pokemon you, Pokemon opponent)
        {
            /*
             * Check types must be interpreted here.
             * It returns a 0-8 value, with 2 being
             * the most dangerous, 0 being least.
             * and 1 being an even fight
             */
            
            float yve = you.checkTypes(opponent);
            float evy = opponent.checkTypes(you);
            float danger = (yve + evy) / 2; //danger becomes the average of your offensive matchup and defensive matchup
            c.writef("Offense:" +yve.ToString()+" Defense:"+evy.ToString()+"\nDanger: "+danger.ToString(), "[DEBUG]", Global.okColor);
            /*
             * Now, adjust danger according to 
             * characteristics like our role
             * and the opponent's defense type.
             * For example,  
             * */
            danger += you.checkKOChance(opponent);
            c.writef("Updated danger: " + danger.ToString(), "[DEBUG]", Global.okColor);
            //Now determine % chance we will switch
            float chance;
            if (danger <= 0.5)
                chance = 0; //don't worry about it
            else if ((danger > 0.5) && (danger < 1))
                chance = 0.1f;
            else if (danger == 1)
                 chance = 0.15f;
            else if ( (danger > 1) && (danger <= 1.5))
                 chance = 0.25f;
            else if ((danger > 1.5) && (danger <= 1.999))
                chance = 0.5f;
            else
                chance = 0.9f;
            
            return chance;
        }

       
       
        
        //TODO: This doesn't account for instances where there are less than 4 moves.
 
        

        
        
   #endregion




        

        


    }//End of Class

}


