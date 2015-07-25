using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WatiN.Core;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace ShowdownBot
{
    class Bot
    {
        // ///Bot info
        //Move to a file or something easier to read/write
        string site = "http://play.pokemonshowdown.com/";
        string username = "username"; //Change these
        string password = "password"; //Change these
        // ///Site Info
        string LoginButton = "login";
        string nameField = "username";
        string passwordField = "password";
        // ///Vars
        int loginAttempts;
        State activeState;
        IE mainBrowser;
        //////////////
        //Bot states
        public enum State
        {
            IDLE,
            BATTLEOU,
            RANDOMBATTLE,
            CHALLANGEPLR
        };



        Consol c;
        public Bot(Consol c)
        {
            this.c = c;
            activeState = State.IDLE;
            
        }


        public void Start()
        {
            //Todo: Add checks for routines, etc. Determine what the bot will 
            //do and store it somewhere to call it again after successful login.
            loginAttempts = 0;
            //TODO: clean this shit up and have it make sure we can actually connect to the website first.
            //if (CanConnect())
            //OpenSite(site);
            //else
            //{
            //    c.writef("Cannot connect to "+site,"[ERROR]",Color.Red);
            //    c.writef("Aborting processes. Please start again.",Color.Red);
            //}
            OpenSite(site);
            

        }

        public void performNextTask(IE b)
        {
            IE browser = b;
            while (activeState == State.IDLE)
            {
                System.Threading.Thread.Sleep(5000);
                //wait 5 seconds and check for a change in state.

            }
            if (activeState == State.RANDOMBATTLE)
            {
                challengePlayer(mainBrowser);
            }
        }

        public bool OpenSite(string site)
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

                
                //Challenge Vardy-B to random
                c.write("Searching for user");
                browser.Button(Find.ByName("finduser")).Click();
                browser.TextField(Find.ByName("data")).TypeText("Vardy-B");
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                c.write("Contacting user");
                browser.Button(Find.ByName("challenge")).Click();
                //browser.TextField(Find.ByName("message")).TypeText("Hi!");
               // System.Windows.Forms.SendKeys.SendWait("{ENTER}");
               // c.write("Sent message: Hi!");
                browser.Button(Find.ByName("makeChallenge")).Click();
                c.write("Challenge made, awaiting response.");

                ////Indicates we're in a battle, but there should be a better way to check.
                browser.WaitUntilContainsText("Sleep Clause Mod", 500);
                c.writef("Battle starting!", Global.botInfoColor);
                activeState = State.RANDOMBATTLE;
                randomBattle(browser);
                performNextTask(browser);
                // browser
                return true;
            }
        }

        public bool CanConnect()
           {
            var ping = new Ping();
            var reply = ping.Send(site);
            return reply.Status == IPStatus.Success;
           }

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
            ///The random battle has just started, pick a move.
            int moveSelection;
            int pokeSelection;
            int turn = 1;
            int[] pkmnExclude = null; //Pokemon to exclude from being selected.
            DateTime lastAction, currentAction;
            bool hasMoved = false;
            do
            {
                //If this button exists, the match is over.
                
                //if (browser.Button(Find.ByName("chooseSwitch")).Exists)
                //    c.writef("Found switching buttons.", "[DEBUG]", Color.Green);

               
             

                //Check for items that limit skills.


                if (checkMove(browser))
                {
                    moveSelection = determineMoveRandomly(browser);
                    c.writef("I'm selecting move " + moveSelection.ToString(), "[TURN " + turn.ToString() + "]", Global.botInfoColor);
                    browser.Button(Find.ByValue(moveSelection.ToString())).Click(); //Select move
                    //hasMoved = true;
                    lastAction = DateTime.Now;
                    System.Threading.Thread.Sleep(2000);
                    turn++;
                }
                ////////
                // c.write("Checking to see if we need to switch pokemon.");
                else if (checkSwitch(browser))
                {

                    c.writef("Switching pokemon.", Global.botInfoColor);
                    pokeSelection = pickPokeRandomly(pkmnExclude, browser);
                    //while (!browser.Button(Find.ByValue(pokeSelection.ToString())).Enabled ||
                    //    !browser.Button(Find.ByValue(pokeSelection.ToString())).Exists)
                    //{
                    //   // pkmnExclude[i] = selection;
                    //    pokeSelection = pickPokeRandomly(null,browser); //If the pokemon to be selected is fainted/DNE, pick another.
                    //    i++;
                    //} //moved to poke select method
                    c.writef("New pokemon selected: " + pokeSelection.ToString(), Global.botInfoColor);
                    //  turn++;
                    browser.Button(Find.ByValue(pokeSelection.ToString())).Click();
                    System.Threading.Thread.Sleep(2000);
                }
                else if (checkBattleEnd(browser))
                {

                    return true;
                }
                //else if (DateTime.Now.Add(-lastAction) >= 200)
                //{

                //}
                else
                {
                    // c.write("Sleeping for 2 secs");
                    System.Threading.Thread.Sleep(2000);
                }
              

            }while(activeState == State.RANDOMBATTLE);

            //Done battling, but the battle isn't over.
            if (activeState == State.IDLE && !checkBattleEnd(browser))
            {
                goMainMenu(browser, true);
            }
            else
                goMainMenu(browser, false);
            return true;

        }


        private void challengePlayer(IE b)
        {
            string player = "Vardy-B";
            IE browser = b;
            //wait for page to load
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
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                c.write("Contacting user for random battle");
                browser.Button(Find.ByName("challenge")).Click();
                //browser.TextField(Find.ByName("message")).TypeText("Hi!");
                // System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                // c.write("Sent message: Hi!");
                browser.Button(Find.ByName("makeChallenge")).Click();
                c.write("Challenge made, awaiting response.");

                ////Indicates we're in a battle, can't think of a better way to check for this.
                ////Check for the battle buttons/timer button
                browser.WaitUntilContainsText("Sleep Clause Mod", 500);
                c.writef("Battle starting!", Global.botInfoColor);
                randomBattle(browser);
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
            //if (exclude == null)
            //    return choice; 
            //if we exclude all but one, just return that one.
            //if (exclude.Length == 4)
            //{
            //    int total = 0;
            //    for (int i = 0; i < exclude.Length; i++)
            //    {
            //       total = total + exclude[i];
            //    }
            //    //Since all the choices (1-5) added together = 15, subtracting what we have from 15 will give us the remaining choice.
            //    return (15 - total);

            //}
            //Failing all that, pick a mon randomly.
            ////for (int i = 0; i < exclude.Length; i++)
            ////{
            ////    if (exclude[i] == choice)
            ////    choice = rand.Next(1,5);
            ////}
            //Exclude is busted right now, using simple method
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


        private int GetRandomExcluding(HashSet<int> ex, int min, int max)
        {
            var exclude = ex;
            var range = Enumerable.Range(min, max).Where(i => !exclude.Contains(i));

            var rand = new System.Random();
            int index = rand.Next(min-1, (max-1) - exclude.Count);
            return range.ElementAt(index);
        }

        private void sendText(string t)
        {
            c.Invoke((MethodInvoker)delegate { c.write(t); });
        }




        
    }//End of Class

}


