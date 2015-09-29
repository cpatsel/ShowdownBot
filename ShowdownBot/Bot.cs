using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WatiN.Core;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;

namespace ShowdownBot
{
    class Bot
    {
        #region Bot Info

        string site;
        string username; 
        string password;
        string owner; //More uses for this later, right now it's used to initiate the challenge.
        // ///Site Info
        string LoginButton = "login";
        string nameField = "username";
        string passwordField = "password";
        // ///Vars
        int loginAttempts;
        State activeState;
        AiMode modeCurrent;
        IE mainBrowser;
        float move1Weight = 0.4f;
        float move2Weight = 0.3f;
        float move3Weight = 0.2f;
        float move4Weight = 0.1f;
        bool needLogout, isLoggedIn;
        bool isRunning;
        //////////////
        //Bot states
        public enum State
        {
            IDLE,
            BATTLEOU,
            RANDOMBATTLE,
            CHALLANGEPLR,
            BUSY

        };
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
            activeState = State.IDLE;
            modeCurrent = AiMode.RANDOM; //TODO: set default in config to be read
          
            ReadFile();
        }

        public State getState() { return activeState; }
        public AiMode getMode() { return modeCurrent; }
        public bool getStatus() { return isRunning; }
        
        public void changeState(State nstate)
        {
            c.write("Changing state to: " + nstate.ToString());
            State oldState = activeState;
            activeState = nstate;
            if (mainBrowser != null)
            {
                if (oldState == State.RANDOMBATTLE)
                {
                    //forfeit a match we're in if we switch.
                    //this (should be) handled in the randombattle() method.

                }
            }
            // Update(mainBrowser);

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
                Logout(mainBrowser);
            }
            c.write("Bot is shutting down.");
            isRunning = false;
           
            
            
        }
        private void ReadFile()
        {
            if (!File.Exists("botInfo.txt"))
            {
                c.writef("Could not load config (maybe it's missing?)", "[ERROR]", Global.errColor);
                c.writef("You can try starting without authenticating (startf)", Global.warnColor);
                return;
            }
            using (var reader = new StreamReader("botInfo.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //ghetto commenting, avoid using inline comments :^)
                    if (!line.StartsWith("//") || !line.StartsWith("#"))
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

            }
        }
        private void Update(IE b)
        {
            IE browser = b;
            c.write("Ready.");
            while (isRunning)
            {
                if (activeState == State.IDLE)
                {
                    System.Threading.Thread.Sleep(5000);
                }
                else if (activeState == State.RANDOMBATTLE)
                { 
                    challengePlayer(mainBrowser);
                }
                else if (activeState == State.BATTLEOU)
                {
                    challengePlayer(mainBrowser);
                }
            }
            c.writef("Done performing tasks.", Global.okColor);
        }

        private bool OpenSite(string site)
        {
            using (var browser = new IE(site))
            {
                mainBrowser = browser;
                //wait a second for page to load.
                browser.WaitForComplete(160);
                System.Threading.Thread.Sleep(5000);
                if (!browser.Button(Find.ByName(LoginButton)).Exists)
                {
                  c.writef("Cannot find login button", "[WARNING]", Global.warnColor);
                  
                  System.Threading.Thread.Sleep(2000);
                  if (browser.Span(Find.ByClass("username")).Exists)
                  {
                      c.writef("Assuming already logged in.", Global.okColor);
                  }
                  else
                  {
                      c.writef("There was a problem logging in.","[ERROR]",Global.errColor);
                      return false;
                  }
                }
                else
                {
                    c.write("Login found, attempting to login as" + username);
                    if (Login(browser))
                    {
                        c.write("Successfully logged in as " + username);
                        needLogout = true;
                    }
                    else
                    {
                        c.writef("Could not log in. Aborting.", "[ERROR]", Global.errColor);
                        return false;
                    }



                }
                Update(browser);
                return true;
            }
        }
        private bool OpenSiteNoAuth(string site)
        {
            using (var browser = new IE(site))
            {
                mainBrowser = browser;
                if (mainBrowser == null)
                    c.writef("main browser is null", "[DEBUG]", Global.okColor);
                c.write("Opened site, skipping authentication steps.");
                browser.WaitForComplete();
                c.write("Moving onto next task");
                Update(browser);
               
                
                return true;
            }
        }
        public bool CanConnect()
           {
            var ping = new Ping();
            var reply = ping.Send(site);
            return reply.Status == IPStatus.Success;
           }

       



        //Change the type if we change the browser
        private bool Login(IE browser)
        {
            
           
                //Click the choose name button and input the creditentials

                browser.Button(Find.ByName(LoginButton)).Click();
                browser.TextField(Find.ByName(nameField)).TypeText(username);
                //needs to be a better way to find the second "choose name" button. For now, just mimic hitting enter.
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                System.Threading.Thread.Sleep(1000);
                if (!browser.TextField(Find.ByName("password")).Exists)
                {
                    c.writef("Cannot find password field", "[WARNING]", Global.warnColor);
                    return false;
                }
                    //Make sure the above assumption is correct.
                //if (browser.Button(Find.ByName(LoginButton)).Exists)
                //{
                //    //if (loginAttempts < 2)
                //    //{
                //    //    loginAttempts++;
                //    //    c.writef("Assumption incorrect, attempting to login again.", "[ERROR]", Color.Red);
                //    //    Login(browser);
                //    //}
                //    //else
                //    //{
                //    //    return false;
                //    //}
                //    return false;

                //}
                    //else
                    //{
                        c.write("Entering password " + password);
                        browser.TextField(Find.ByName(passwordField)).TypeText(password);
                        browser.Button(Find.ByText("Log in")).Click();
                        //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                        
                    //}

                    return true;
                
            
        }

        private bool Logout(IE browser)
        {
            c.write("Logging out.");
            browser.Button(Find.ByName("openOptions")).Click();
            browser.Button(Find.ByName("logout")).Click();
            return true;
        }
        
        private bool CheckLoggedIn(IE browser)
        {
            if (browser.Button(Find.ByName(LoginButton)).Exists)
                return false;
            else
                return true;
        }

        private bool checkBattleEnd(IE b)
        {
            IE browser = b;
            if (browser.Button(Find.ByName("closeAndMainMenu")).Exists)
            {
                //The match is over
                c.writef("The battle has ended! Returning to main menu.", Global.botInfoColor);
                browser.Button(Find.ByName("closeAndMainMenu")).Click();
                activeState = State.IDLE;
                return true;
            }
            else
                return false;

        }

        /// <summary>
        /// Checks the bot's ability to select a move.
        /// Bot prioritizes making moves over switching (for now)
        /// </summary>
        /// <param name="b"></param>
        /// <returns>Can select a move?</returns>
        private bool checkMove(IE b)
        {
            
            IE browser = b;
            //int selection = sel;
            WatiN.Core.Button but = browser.Button(Find.ByName("chooseMove"));
            if (but.Exists)
            {
               
                return true;
            }
            else
                return false;
            
        }

        /// <summary>
        /// Checks if the bot can switch.
        /// </summary>
        /// <returns>can switch?</returns>
        private bool checkSwitch(IE b)
        {
            IE browser = b;
            if (!browser.Button(Find.ByName("chooseMove")).Exists &&
                 browser.Button(Find.ByName("chooseSwitch")).Exists &&
                !browser.Button(Find.ByName("undoChoice")).Exists)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Action loop for BattleRandom state.
        /// </summary>
        /// <returns></returns>

        private bool randomBattle(IE browser)
        {
            int turn = 1;
            do
            {

                if (modeCurrent == AiMode.RANDOM)
                {
                    battleRandomly(browser, ref turn);
                }
                else if (modeCurrent == AiMode.BIAS)
                {
                    battleBiased(browser, ref turn);
                }

            }while(activeState == State.RANDOMBATTLE);

            //Done battling, but the battle isn't over.

            if (activeState == State.IDLE && !checkBattleEnd(browser))
            {
                goMainMenu(browser, true);
            }
            return true;

        }

        /// <summary>
        /// Action loop for the BattleOU state.
        /// </summary>
        /// <param name="browser"></param>
        /// <returns>Successful end to battle</returns>
        private bool ouBattle(IE browser)
        {
            int turn = 1;
            IE b = browser;
            
            //Select lead
            WatiN.Core.Button lead;
            if (modeCurrent == AiMode.ANALYTIC) lead = SelectLead(browser); 
            else lead = b.Button(Find.ByValue("0").And(Find.ByName("chooseTeamPreview")));

            do
            {

                if (lead.Exists)
                {
                    c.writef("Selecting first pokemon as lead.", Global.botInfoColor);
                    lead.Click();
                }

                if (modeCurrent == AiMode.RANDOM)
                {
           
                    battleRandomly(browser, ref turn);
                }
                else if (modeCurrent == AiMode.BIAS)
                {
                   
                    battleBiased(browser, ref turn);
                }

            } while (activeState == State.BATTLEOU);
            return true;
        }

        /// <summary>
        /// Determines all actions randomly, with some guidance.
        /// </summary>
        /// <param name="b">browser instance</param>
        /// <param name="turn"></param>
        /// <returns></returns>
        private bool battleRandomly(IE b, ref int turn)
        {
            IE browser = b;
            int moveSelection;
            int pokeSelection;
            if (checkMove(browser))
            {

                //first check if there's a mega evo option
                WatiN.Core.CheckBox box = browser.CheckBox(Find.ByName("megaevo"));
                if (box.Exists && !box.Checked)
                {
                    box.Click();
                }

                moveSelection = determineMoveRandomly(browser);
                c.writef("I'm selecting move " + moveSelection.ToString(), "[TURN " + turn.ToString() + "]", Global.botInfoColor);
                browser.Button(Find.ByValue(moveSelection.ToString())).Click(); //Select move
                //hasMoved = true;
                //lastAction = DateTime.Now;
                System.Threading.Thread.Sleep(2000);
                turn++;
            }
            else if (checkSwitch(browser))
            {

                c.writef("Switching pokemon.", Global.botInfoColor);
                pokeSelection = pickPokeRandomly(browser);
                c.writef("New pokemon selected: " + pokeSelection.ToString(), Global.botInfoColor);
                browser.Button(Find.ByValue(pokeSelection.ToString())).Click();
                System.Threading.Thread.Sleep(2000);
            }
            else if (checkBattleEnd(browser))
            {
                return true;
            }
            else
            {
                //c.write("Sleeping for 2 secs");
                System.Threading.Thread.Sleep(2000);
            }
            return false;
        }

        /// <summary>
        /// Determines actions based on the predetermined weight of each moveslot.
        /// </summary>
        /// <param name="b">browser</param>
        /// <param name="turn"></param>
        /// <returns>state of the battle (over/true, ongoing/false)</returns>
        private bool battleBiased(IE b, ref int turn)
        {
            IE browser = b;
            int moveSelection;
            int pokeSelection;

            if (checkMove(browser))
            {
                WatiN.Core.CheckBox box = browser.CheckBox(Find.ByName("megaevo"));
                if (box.Exists && !box.Checked)
                {
                    c.writef("I'm mega evolving this turn.", Global.botInfoColor);
                    box.Click();
                }

                moveSelection = pickMoveBiased(browser);
                c.writef("I'm selecting move " + moveSelection.ToString(), "[TURN " + turn.ToString() + "]", Global.botInfoColor);
                browser.Button(Find.ByValue(moveSelection.ToString())).Click(); //Select move
                System.Threading.Thread.Sleep(2000);
                turn++;
            }
            else if (checkSwitch(browser))
            {
                //TODO: check if it's the first turn, and then select appropriate lead.
                c.writef("Switching pokemon.", Global.botInfoColor);
                pokeSelection = pickPokeRandomly(browser);
                c.writef("New pokemon selected: " + pokeSelection.ToString(), Global.botInfoColor);
                browser.Button(Find.ByValue(pokeSelection.ToString())).Click();
                System.Threading.Thread.Sleep(2000);
            }
            else if (checkBattleEnd(browser))
            {

                return true;
            }
            else
            {
                // c.write("Sleeping for 2 secs");
                System.Threading.Thread.Sleep(2000);
            }
            return false;
        }

        /// <summary>
        /// Sends a challenge to a player.
        /// If no player is specified, it defaults to owner.
        /// </summary>
        /// <param name="b"></param>
        private void challengePlayer(IE b, string user)
        {
            string player = user;
            IE browser = b;
            c.writef("Waiting for page to load", "[DEBUG]", Global.okColor);
            browser.WaitForComplete(160);
            if (activeState == State.RANDOMBATTLE)
            {
                if (b == null)
                    c.writef("current browser is null", "[DEBUG]", Global.okColor);
                c.write("Searching for "+ player);
                if (!browser.Button(Find.ByName("finduser")).Exists)
                    c.writef("finduser button does not exist!", "[DEBUG]", Global.warnColor);
                browser.Button(Find.ByName("finduser")).Click();
                browser.TextField(Find.ByName("data")).TypeText(player);
               // System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                browser.Button(Find.ByText("Open")).Click();
                c.write("Contacting user for random battle");
                browser.Button(Find.ByName("challenge")).Click();
                browser.Button(Find.ByName("makeChallenge")).Click();
                c.write("Challenge made, awaiting response.");
                ////TODO: Check for the battle buttons/timer button. More reliable than checking for text.
                browser.WaitUntilContainsText("Format:");
                c.writef("Battle starting!", Global.botInfoColor);
                randomBattle(browser);
            }
            else if (activeState == State.BATTLEOU)
            {
                c.write("Searching for " + player);
                if (!browser.Button(Find.ByName("finduser")).Exists)
                    c.writef("finduser button does not exist!", "[DEBUG]", Global.okColor);
                browser.Button(Find.ByName("finduser")).Click();
                browser.TextField(Find.ByName("data")).TypeText(player);
                browser.Button(Find.ByText("Open")).Click();
                c.write("Contacting user for OU battle");
                browser.Button(Find.ByName("challenge")).Click();
                //Select format
                browser.Button(Find.ByName("format")).Click();
                browser.Button(Find.ByValue("ou")).Click();
                //TODO: implement a way to select alternate teams/ have more than one team.
                browser.Button(Find.ByName("makeChallenge")).Click();
                c.write("Challenge made, awaiting response.");
                ////TODO: Check for the battle buttons/timer button. More reliable than checking for text.
                browser.WaitUntilContainsText("Format:");
                c.writef("Battle starting!", Global.botInfoColor);
                ouBattle(browser);
            }
        }
        private void challengePlayer(IE b)
        {
            challengePlayer(b, owner);
        }

        private int determineMoveRandomly(IE b)
        {
            IE browser = b;
            Random rand = new Random();
            HashSet<int> exclude = new HashSet<int>();

            int choice = rand.Next(1, 4);
            
            while (!browser.Button(Find.ByValue(choice.ToString())).Exists) //should help it select moves with choice items/outrage etc.
            {
                c.writef("Bad move choice: " + choice.ToString()+ "Picking another", "[DEBUG]", Global.okColor);
                exclude.Add(choice);
                choice = GetRandomExcluding(exclude, 1, 4);
            }
            return choice;
        }

        /// <summary>
        /// Randomly selects a pokemon.
        /// </summary>
        /// <returns>Index of pokemon.</returns>
        private int pickPokeRandomly(IE b)
        {
            Random rand = new Random();
            IE browser = b;
            HashSet<int> exclude = new HashSet<int>();
            int i = 0;
            int choice = rand.Next(1, 5);
            c.write("Choosing new pokemon");
            choice = rand.Next(1, 5);
            while (!browser.Button(Find.ByValue(choice.ToString())).Exists )
            {
                c.writef("Bad pokemon " + choice.ToString() + ". Rolling for another.", Global.botInfoColor);

                exclude.Add(choice); //Steer it in the right direction by removing bad choices.
                choice = GetRandomExcluding(exclude, 1, 5);
               
            }
            return choice;
        }

        private int pickMoveBiased(IE b)
        {
            IE browser = b;
            
            HashSet<int> exclude = new HashSet<int>();
            int choice;
            choice = getIndexBiased();
            while (!browser.Button(Find.ByValue(choice.ToString())).Exists) 
            {
                //If the move we've chosen does not exist, just cycle through until we get one.
                c.writef("Bad move choice: " + choice.ToString() + "Picking another", "[DEBUG]", Global.okColor);
                exclude.Add(choice);
                choice = GetRandomExcluding(exclude, 1, 4);
            }

            return choice;
        }
       
        #region Analytic Functions
        WatiN.Core.Button SelectLead(IE browser)
        {
            //for now just select the first pokemon
            //todo: analyze things, pick the best choice

            return browser.Button(Find.ByValue("0").And(Find.ByName("chooseTeamPreview")));
        }





#endregion


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

        /// <summary>
        /// Should be used when exiting a battle, prematurely or otherwise.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="forfeit">Go through steps to forfeit match?</param>
        /// <returns>Whether it forfeited</returns>
        private bool goMainMenu(IE b, bool forfeit)
        {
            IE browser = b;
            if (forfeit)
            {
                //force the browser to click the exit button.
                browser.Eval("$('.closebutton').click();");
                browser.Button(Find.ByText("Forfeit")).Click();//forfeiting also closes the tab.
                
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Gets a random number from the range, excluding all numbers in the hash set.
        /// </summary>
        /// <param name="ex">set of excluded numbers</param>
        private int GetRandomExcluding(HashSet<int> ex, int min, int max)
        {
            var exclude = ex;
            var range = Enumerable.Range(min, max).Where(i => !exclude.Contains(i));

            var rand = new System.Random();
            int index = rand.Next(min-1, (max-1) - exclude.Count);
            return range.ElementAt(index);
        }


    }//End of Class

}


