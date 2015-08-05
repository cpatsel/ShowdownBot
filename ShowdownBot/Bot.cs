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
        bool needLogout;
        bool isRunning;
        //////////////
        //Bot states
        public enum State
        {
            IDLE,
            BATTLEOU,
            RANDOMBATTLE,
            CHALLANGEPLR

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
            // performNextTask(mainBrowser);

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
            needLogout = auth;
            isRunning = true;
            c.write("Opening site.");
            if (auth)
                OpenSite(site);
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
                c.write("Logging out.");
                //Logout
            }
            c.write("Bot is shutting down.");
            isRunning = false;
           
            
            
        }
        private void ReadFile()
        {
            //ConfigurationManager is giving me an error
            site = ConfigurationSettings.AppSettings.Get("site");
            username = ConfigurationSettings.AppSettings.Get("username");
            password = ConfigurationSettings.AppSettings.Get("password");
            owner = ConfigurationSettings.AppSettings.Get("owner");
            c.writef("Bot's owner set to: " + owner, "[DEBUG]", Global.okColor);
        }
        private void performNextTask(IE b)
        {
            IE browser = b;
           // bool loop = true;
            //A better exit condition needs to be implemented.
            while (isRunning)
            {
                if (activeState == State.IDLE)
                {
                    System.Threading.Thread.Sleep(5000);
                    //wait 5 seconds and check for a change in state.

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
                if (mainBrowser == null)
                    c.writef("main browser is null", "[DEBUG]", Global.okColor);
                //wait a second for page to load.
                //System.Threading.Thread.Sleep(1000);
                browser.WaitUntilContainsText("Choose name");
                if (!browser.Button(Find.ByName(LoginButton)).Exists)
                {
                    c.write("TEST");
                  c.writef("Cannot find login button", "[WARNING]", Global.warnColor);
                    c.writef("Assuming already logged in, proceeding", Global.warnColor);
                }
                else
                {
                    c.write("Login found, attempting to login as" + username);
                    if (Login(browser))
                    {
                        c.write("Successfully logged in as " + username);
                    }
                    else
                    {
                        c.writef("Could not log in. Aborting.", "[ERROR]", Global.errColor);
                        return false;
                    }



                }
                performNextTask(browser);
                //if (!browser.Span(Find.ByClass("username")).Exists) //The userbar should show our name if we've succesfully logged in.
                //{
                //    c.writef("Cannot find Username bar", "[DEBUG]", Color.Green);
                //    //c.writef("Unable to validate login.", "[ERROR]", Color.Red);
                //    //c.writef("Aborting operations.", Color.Red);
                //    //return false;
                //}
                //if (!browser.ContainsText(username))
                //{
                //    c.writef("Cannot find instance of username", "[DEBUG]", Color.Green);
                //}

                
               
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
                performNextTask(browser);
               
                
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
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                //   System.Threading.Thread.Sleep(1000);
                if (!browser.TextField(Find.ByName(password)).Exists)
                {
                    c.writef("Cannot find password field", "[WARNING]", Global.warnColor);
                    c.writef("Assuming already logged in, proceeding", Global.warnColor);
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
                        ////browser.Button(Find.ByText("Log In")).Click();
                        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                        
                    //}

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
            //This doesn't work

            //if (activeState == State.IDLE && !checkBattleEnd(browser))
            //{
            //    goMainMenu(browser, true);
            //}
            //else
            //    goMainMenu(browser, false);
            return true;

        }

        private bool ouBattle(IE browser)
        {
            int turn = 1;
            IE b = browser;
            
            //Select lead
            WatiN.Core.Button lead = b.Button(Find.ByValue("0").And(Find.ByName("chooseTeamPreview")));

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
            int[] pkmnExclude = null;

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
                pokeSelection = pickPokeRandomly(pkmnExclude, browser);
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
            int[] pkmnExclude = null;

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
                pokeSelection = pickPokeRandomly(pkmnExclude, browser);
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
        /// TODO: implement specifying which player. (Defaults to "owner")
        /// </summary>
        /// <param name="b"></param>
        private void challengePlayer(IE b)
        {
            string player = owner;
            IE browser = b;
            c.writef("Waiting for page to load", "[DEBUG]", Global.okColor);
            browser.WaitForComplete(160);
            if (activeState == State.RANDOMBATTLE)
            {
                if (b == null)
                    c.writef("current browser is null", "[DEBUG]", Global.okColor);
                c.write("Searching for "+ player);
                if (!browser.Button(Find.ByName("finduser")).Exists)
                    c.writef("finduser button does not exist!", "[DEBUG]", Global.okColor);
                browser.Button(Find.ByName("finduser")).Click();
                browser.TextField(Find.ByName("data")).TypeText(player);
               // System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                browser.Button(Find.ByText("Open")).Click();
                c.write("Contacting user for random battle");
                browser.Button(Find.ByName("challenge")).Click();
                browser.Button(Find.ByName("makeChallenge")).Click();
                c.write("Challenge made, awaiting response.");
                ////TODO: Check for the battle buttons/timer button. More reliable than checking for text.
                browser.WaitUntilContainsText("Sleep Clause Mod", 500);
                c.writef("Battle starting!", Global.botInfoColor);
                randomBattle(browser);
                performNextTask(browser);
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
                browser.WaitUntilContainsText("Sleep Clause Mod", 500);
                c.writef("Battle starting!", Global.botInfoColor);
                ouBattle(browser);
                performNextTask(browser);
            }
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
        private int pickPokeRandomly(int[] ex, IE b)
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
       
        /// <summary>
        /// Helper method for pickMoveBiased.
        /// </summary>
        /// <returns></returns>
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


        private bool goMainMenu(IE b, bool forfeit)
        {
            IE browser = b;
            if (forfeit)
            {
                //TODO: bot can't find this chatbox
                browser.TextField(Find.ByClass("textbox")).TypeText("/forfeit");
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                System.Threading.Thread.Sleep(2000);
                //browser.Button(Find.ByLabelText("Forfeit")).Click();
                browser.Button(Find.ByName("closeAndMainMenu")).Click(); 
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


