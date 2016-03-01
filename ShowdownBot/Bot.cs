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
using System.Collections;

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
        State activeState;
        AiMode modeCurrent;
        IE mainBrowser;
        Movelist movelist;
        float move1Weight = 0.4f;
        float move2Weight = 0.3f;
        float move3Weight = 0.2f;
        float move4Weight = 0.1f;
        bool needLogout, isLoggedIn;
        bool isRunning;
        LastBattleAction lastAction;
        string status;
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

        public enum LastBattleAction
        {
            ACTION_ATTACK_SUCCESS,
            ACTION_ATTACK_FAILURE,
            ACTION_STATUS,
            ACTION_BOOST,
            ACTION_SWITCH
        };
        #endregion

        Consol c;
        public Bot(Consol c)
        {
            this.c = c;
            isRunning = false;
            activeState = State.IDLE;
            modeCurrent = AiMode.RANDOM; //TODO: set default in config to be read
            pokedex = new Dictionary<string, Pokemon>();
            ReadFile();
            Global.setupTypes();
            BuildPokedex();
            Global.moves = new Dictionary<string,Move>();
            movelist = new Movelist();
            movelist.initialize();
            testDCalc();
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
            changeState(State.BUSY);
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
        private void testDCalc()
        {

            Pokemon def = pokedex["alakazam"];
            Pokemon atk = pokedex["beedrill"];
            float risk = getRisk(mainBrowser, def, atk);
            c.writef("Risk is... " + risk.ToString(), "[DEBUG]", Global.okColor);
            
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
            changeState(State.BUSY);
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
        private void Update(IE b)
        {
            IE browser = b;
            c.write("Ready.");
            changeState(State.IDLE);
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
               // browser.NativeDocument.Body.SetFocus();
                browser.WaitForComplete(200);
                if (browser.Span(Find.ByClass("username")).Exists)
                {
                    c.writef("Assuming already logged in.", Global.okColor);
                }
                else
                {
                    c.write("Login found, attempting to login as" + username);
                    try
                    {
                      //  browser.DomContainer.Eval("$('button[name=login]').click();");
                    }
                    catch (Exception e)
                    {
                        c.writef(e.ToString(), Global.warnColor);
                    }

                   // WatiN.Core.Button b = browser.Button(Find.ByName("openSounds"));
                  //  b.Click();
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

                        c.write("Entering password " + password);
                        browser.TextField(Find.ByName(passwordField)).TypeText(password);
                        browser.Button(Find.ByText("Log in")).Click();
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
        /// similar to randomBattle, except that there is additional
        /// logic for selecting lead, etc. that is not present in Random Battles.
        /// </summary>
        /// <param name="browser"></param>
        /// <returns>Successful end to battle</returns>

        int lastTurn = 0;
        private bool ouBattle(IE browser)
        {
            int turn = 1;
            IE b = browser;
            
            //Select lead
            WatiN.Core.Button lead;
            if (modeCurrent == AiMode.ANALYTIC) lead = SelectLead(browser); 
            else lead = b.Button(Find.ByValue("0").And(Find.ByName("chooseTeamPreview")));

            Pokemon active = pokedex[lead.OuterText.ToLower()];
            Pokemon enemy = null;
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
                else if (modeCurrent == AiMode.ANALYTIC)
                {

                    if (turn != lastTurn)
                    {
                        enemy = getActivePokemon(browser);
                        status = updateYourPokemon();
                    }
                    battleAnalytic(browser,ref active, enemy, ref turn);
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
        /// Considers different courses of action based on information about the current enemy.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="enemy"></param>
        /// <param name="turn"></param>
        /// <returns>End of battle?</returns>
        private bool battleAnalytic(IE b, ref Pokemon active, Pokemon enemy, ref int turn)
        {
            IE browser = b;
            int moveSelection;
            int pokeSelection;
            if (enemy == null)
            {
                if (checkSwitch(b))
                {
                    WatiN.Core.Button but = b.Button(Find.ByName("chooseSwitch") &&
                                            Find.ByValue(pickPokeRandomly(b).ToString()));
                    but.Click();
                }
                else
                    return false;
            }
            /* 
             * small ( maybe <10% ) chance of doing something random (so as to not be entirely predictable)
             * first we do risk assessment
             *      if we should switch, find most suitable candidate
             *  then pick a move
             *  firstly, preempt set-up if afflicable/relevant
             *  
             *      if sweeper
             *          check for nullifying abilities like wonder guard/levitate etc.
             *          avoid picking an attack nullified by opponent's type
             *          pick the most powerful attack against the enemy's types
             *      if support/cleric
             *          check field for already present status, and avoid doubling up.
             *      
             *      else if all choices are equally bad/good just pick a random one.   
             * 
            */
            
            if (checkBattleEnd(browser))
            {
                return true;
            }
            else if (turn == lastTurn)
                return false;
            //Switch if fainted
            else if (checkSwitch(browser))
            {
                active = pickPokeAnalytic(browser, enemy);
            }
            //Preemptively switch out of bad situations
            else if (needSwitch(browser, getRisk(
                                    browser, active, enemy)))
            {
               
                c.writef("I'm switching out.", "Turn "+turn.ToString(), Global.botInfoColor);
                active = pickPokeAnalytic(browser, enemy);
                turn++;
                
            }
            else if (checkMove(browser))
            {
                //for now, automatically activate mega
                WatiN.Core.CheckBox box = browser.CheckBox(Find.ByName("megaevo"));
                if (box.Exists && !box.Checked)
                {
                    box.Click();
                }
               
                c.writef("I'm picking move " + pickMoveAnalytic(active, enemy), "Turn " + turn.ToString(), Global.botInfoColor);
                turn++;
            }

            else
                System.Threading.Thread.Sleep(2000);
           
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
                    pokedex.Add(p.name, p);
                }
            }
            c.writef("Pokedex built!", Global.okColor);
        }

        private Pokemon getActivePokemon(IE browser)
        {
            //I feel like there's an easier way to do this.
            
            c.write("Getting active Pokemon");
            IElementContainer elem = (IElementContainer)browser.Element(Find.ByClass("rightbar"));
            Div ticon = elem.Div(Find.ByClass("teamicons"));
            string temp = parseNameFromPage(ticon);
            if (temp == "0")
            {
                //Get the second row
                ticon = elem.Div(Find.ByClass("teamicons") && Find.ByIndex(1));
                temp = parseNameFromPage(ticon);
                if (temp == "0") return null;
            }
            //Found the name, now look it up in the dex.
            c.write("The current pokemon is "+temp);
            Pokemon p;
            try
            {
                p = pokedex[temp];
            }
            catch (Exception e)
            {
                c.writef("POKEMON: "+ temp + e.ToString(), "[WARNING]", Global.warnColor);
                p = null;
            }
            return p;
        }
        private string updateYourPokemon()
        {
            IE browser = mainBrowser;

            string currentStatus = "healthy";
            IElementContainer elem = (IElementContainer)browser.Element(Find.ByClass("statbar rstatbar"));
            Div status = elem.Div(Find.ByClass("status"));
            currentStatus = status.OuterText;
            c.writef("Status:", Global.botInfoColor);
            return currentStatus;

        }
        string parseNameFromPage(Div divcollection)
        {
            foreach (Span s in divcollection.Spans)
            {
              
                if (s.Title != null)
                {
                    if (s.Title.Contains("(active)"))
                    {
                        string[] name = s.Title.Split(' ');
                        //Nicknamed pokemon appear in the html as "Nickname (Pokemon) (active)"
                        //this means that the pokemon's name should be N-2, which should hold
                        //true even for non-named mons.
                        string n_name = name[name.Length - 2].Trim('(', ')'); //gets a sanitized name.
                        return n_name.ToLower();
                    }
                }
            }
            return "0"; //return indicator that we did not find it.
        }
        WatiN.Core.Button SelectLead(IE browser)
        {
            //for now just select the first pokemon
            //todo: analyze things, pick the best choice

            return browser.Button(Find.ByValue("0").And(Find.ByName("chooseTeamPreview")));
        }

        private float getRisk(IE browser, Pokemon you, Pokemon opponent)
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

        private bool needSwitch(IE browser, float chance)
        {
            if (isLastMon(browser))
                return false;
            Random rand = new Random();
            float decision = (float)rand.NextDouble();
            if (decision <= chance)
                return true;
            else
                return false;
        }
        private bool isLastMon(IE browser)
        {
            int totalMons = 0;
            for (int i = 1; i <= 5; i++)
            {
                if (browser.Button(Find.ByValue(i.ToString()) && Find.ByName("chooseSwitch")).Exists)
                    totalMons++;
            }
            if (totalMons == 0)
                return true;
            else
                return false;
        }
       
        private Pokemon pickPokeAnalytic(IE browser, Pokemon enemy)
        {
            //Loop over all pokemon
            int bestChoice = 1;
            float highestdamage = 0;
            WatiN.Core.Button b;
            for (int i = 1; i <= 5; i++)
            {

                b = browser.Button(Find.ByValue(i.ToString()) && Find.ByName("chooseSwitch"));
                if (b.Exists)
                {
                    Pokemon p = pokedex[b.OuterText.ToLower()];
                    float temp = enemy.checkTypes(p);
                    temp += enemy.checkKOChance(p);
                    if (temp > highestdamage)
                    {
                        highestdamage = temp;
                        bestChoice = i;
                    }

                }
            }
            b = browser.Button(Find.ByValue(bestChoice.ToString()) && Find.ByName("chooseSwitch"));
            if (b.Exists)
            {
                Pokemon nextPoke = pokedex[b.OuterText.ToLower()];
                b.Click();
                return nextPoke;
            }
            else
            {
                b = browser.Button(Find.ByName("chooseSwitch") && Find.ByValue(pickPokeRandomly(browser).ToString()));
                Pokemon nextPoke = pokedex[b.OuterText.ToLower()];
                b.Click();
                return nextPoke;
            }
            
        }
        //TODO: This doesn't account for instances where there are less than 4 moves.
 
        private string pickMoveAnalytic(Pokemon you, Pokemon enemy)
        {
            float[] rankings = new float[4]; //ranking of each move
            float bestMove = 0f;
            int choice = 1;
            float risk = getRisk(mainBrowser, you, enemy);
            Move[] moves = getMoves();
            for (int i = 0; i<4; i++)
            {
                //For now, only determine the best attacking move.
                if (moves[i].bp == 0)
                {
                    if (moves[i].boost && lastAction == LastBattleAction.ACTION_BOOST)
                        rankings[i] = 0;
                    //placeholder
                    if (moves[i].boost && risk < 0.5)
                        rankings[i] = 2;


                }
                else if (moves[i].bp > 0 || moves[i].bp == -1)
                {
                    rankings[i] = you.attacks(moves[i], enemy);
                    //prune early if the attack is 100% ineffective
                    if (rankings[i] == 0) continue;
                    if (moves[i].type.value == "ground" && (enemy.type2.value == "flying" || enemy.ability1 == "levitate"))
                    {
                        rankings[i] = 0;
                        continue;
                    }
                    if (moves[i].type == you.type1 || moves[i].type == you.type2)
                        rankings[i] += 0.5f;
                }
                c.writef(moves[i].name + "'s rank: " + rankings[i].ToString(), "[DEBUG]", Global.okColor);

            }
            for (int i = 0; i<4; i++)
            {
                if (rankings[i] > bestMove)
                {
                    bestMove = rankings[i];
                    choice = i+1;
                }
            }

            //figure out what move we've chosen
            Move chosenMove = moves[choice - 1];
            if (chosenMove.boost) lastAction = LastBattleAction.ACTION_BOOST;
            else
                lastAction = LastBattleAction.ACTION_ATTACK_SUCCESS;
            if (checkMove(mainBrowser))
            {
                WatiN.Core.Button b = mainBrowser.Button(Find.ByName("chooseMove") && Find.ByValue(choice.ToString()));
                if (b.Exists)
                {
                    b.Click();
                    return chosenMove.name;
                }
                else
                    return "Move "+choice.ToString();
            }
            else
                return "no move";

        }

        private Move[] getMoves()
        {
            Move[] moves = new Move[4];
            for (int i = 0; i < 4; i++)
            {
                WatiN.Core.Button b = mainBrowser.Button(Find.ByName("chooseMove") && Find.ByValue((i + 1).ToString()));
                string[] html = b.OuterHtml.Split(new string[] { "data-move=\"" }, StringSplitOptions.None);
                //var nametag = Array.Find(html, s => s.StartsWith("data-move"));
                string[] name = html[1].Split('"');
                string[] temp = b.ClassName.Split('-');
                string type = temp[1];
                
                
                    moves[i] = lookupMove(name[0], Global.types[type.ToLower()]);
                   // c.writef("Move " + i.ToString() + " " + name[0], Global.botInfoColor);
                
            }
            return moves;
        }
        //possibly redundant
        private Move lookupMove(string n, Type t)
        {
            Move m;
            if (Global.moves.ContainsKey(n))
                m = Global.moves[n];
            else
            {
                c.writef("Unknown move " + n, Global.warnColor);
                m = new Move(n, t);
            }
            return m;
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
              //  browser.Button(Find.ByText("Forfeit")).Click();//forfeiting also closes the tab.
                
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


