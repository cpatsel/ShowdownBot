using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
using System.IO;

namespace ShowdownBot
{
    /// <summary>
    /// Superclass for all botmodules
    /// </summary>
    class BotModule
    {
        protected State activeState;
        protected IWebDriver browser;
        protected Bot manager;
        protected Consol c;
        protected string format;
        protected bool isContinuous;
        protected int maxBattles;
        protected int currentBattle;
        protected State lastBattleState; //used for continuously battling
        protected State lastState; //used in error handling.
        public BotModule(Bot m, IWebDriver b)
        {

            manager = m;
            browser = b;

            init();
        }

        public virtual void init()
        {
            activeState = State.IDLE;
            lastBattleState = State.IDLE;
            c = manager.getConsole();
            maxBattles = 1;
            isContinuous = false;
            currentBattle = 1;
        }
        public virtual void Update()
        {
           
            if (activeState == State.IDLE)
            {
                if (isContinuous)
                {
                    if (currentBattle < maxBattles)
                    {
                        changeState(lastBattleState);
                        currentBattle++;
                        return;
                    }
                }
                System.Threading.Thread.Sleep(5000);
            }
            else if (activeState == State.CHALLENGE)
            {
                lastBattleState = State.CHALLENGE;
                challengePlayer(manager.getChallengee(), format);
            }
            else if (activeState == State.SEARCH)
            {
                lastBattleState = State.SEARCH;
                ladder();
            }
            else if (activeState == State.FORFEIT)
            {
                forfeitBattle();
            }
            else if (activeState == State.BATTLE)
            {
                battle();
            }  
            
        }

        public virtual void battle()
        {
            //battle logic goes here.
        }

        /// <summary>
        /// Sends a challenge to a player.
        /// If no player is specified, it defaults to owner.
        /// </summary>
        /// <param name="b"></param>
        private void challengePlayer(string user, string format)
        {
            string player = user;

            cwrite("Searching for " + player);
            browser.FindElement(By.Name("finduser")).Click();
            IWebElement e = waitFind(By.Name("data"));
            if (e == null) return;
            e.SendKeys(player);
            e.Submit();

            cwrite("Contacting user for OU battle");
            if (!waitFindClick(By.Name("challenge"))) return;

            if (!waitFindClick(By.Name("format"))) return;

            if (!waitFindClick(By.CssSelector("button[name='selectFormat'][value='" + format + "']"))) return;

            browser.FindElement(By.Name("makeChallenge")).Click();
            ////TODO: implement a way to select alternate teams/ have more than one team.
            //Wait until the battle starts.
            if (!waitFindClick(By.Name("ignorespects"),MAX_WAIT_FOR_PLAYER_RESPONSE)) return;
            cwrite("Battle starting!", COLOR_BOT);
            changeState(State.BATTLE);

        }

        public virtual void ladder()
        {
            cwrite("Searching for new opponent in " + format, "bot", COLOR_BOT);
            if (!waitFindClick(By.Name("format"))) return;

            if (!waitFindClick(By.CssSelector("button[name='selectFormat'][value='" + format + "']"))) return;

            if (!waitFindClick(By.Name("search"))) return;
            cwrite("Waiting for an opponent...");

            while (elementExists(By.Name("cancelSearch")) && activeState == State.SEARCH)
            {
                wait();
            }
            if (activeState != State.SEARCH) return; //allow canceling of wait with "stop" command.
            cwrite("Battle starting!", COLOR_BOT);
            changeState(State.BATTLE);
        }

        /// <summary>
        /// Picks a lead if necessary. Defaults to picking the first pokemon on the team.
        /// Returns the name of the pokemon picked, and "error" if unable.
        /// </summary>
        /// <returns></returns>
        public virtual string pickLead()
        {
            string lead;
            cwrite("Selecting first pokemon as lead", COLOR_BOT);
            if (elementExists(By.CssSelector("button[name='chooseTeamPreview']")))
            {
                lead = waitFind(By.CssSelector("button[name='chooseTeamPreview'][value='0']")).Text;
                waitFindClick(By.CssSelector("button[name='chooseTeamPreview'][value='0']"));
            }
            else
                lead = "error";

            return lead;
        }

        #region Battle Information Functions
        /// <summary>
        /// Checks whether it's possible to switch.
        /// </summary>
        /// <returns>can switch?</returns>
         protected bool checkSwitch()
        {

            if (!elementExists(By.Name("chooseMove")) &&
                elementExists(By.Name("chooseSwitch")) &&
                !elementExists(By.Name("undoChoice")))
            {
                return true;
            }

            return false;
        }

         /// <summary>
         /// Checks the bot's ability to select a move.
         /// Bot prioritizes making moves over switching (for now)
         /// </summary>
         /// <param name="b"></param>
         /// <returns>Can select a move?</returns>
         protected bool checkMove()
         {
             
              if (elementExists(By.Name("chooseMove")))
                     return true;
                 else
                     return false;
             
         }


         protected Move[] getMoves()
         {
             //todo deal with moves with no pp/disabled
             Move[] moves = new Move[4];
            int waittime = 1;
            for (int i = 0; i < 4; i++)
            {
                if (!waitUntilElementExists(By.CssSelector("button[value='" + (i + 1).ToString() + "'][name='chooseMove']"), waittime))
                {
                    cwrite("Unavailable or bad move " + i.ToString(), "debug", COLOR_BOT);
                    Move defal = new Move("error", types["error"]);
                    moves[i] = defal;
                    continue;
                }
                IWebElement b = browser.FindElement(By.CssSelector("button[value='" + (i + 1).ToString() + "'][name='chooseMove']"));
                string htmla = (string)((IJavaScriptExecutor)browser).ExecuteScript("return arguments[0].outerHTML;", b);
                string[] html = htmla.Split(new string[] { "data-move=\"" }, StringSplitOptions.None);
                //string[] html = b.GetAttribute("innerhtml").Split(new string[]{"data-move=\""},StringSplitOptions.None);
                var nametag = Array.Find(html, s => s.StartsWith("data-move"));
                string[] name = html[1].Split('"');
                string[] temp = b.GetAttribute("class").Split('-');
                string type = temp[1];

                // moves [i] =

                Move m;
                //hidden power and frustration check
                if (name[0] == "Hidden Power")
                {
                    string nname = "Hidden Power " + type;
                    if (!Global.moves.ContainsKey(nname))
                    {
                        m = new Move(nname, types[type.ToLower()], 60);
                        m.group = "special";
                        Global.moves.Add(m.name, m);
                        moves[i] = m;
                        cwrite("Move " + i.ToString() + " " + m.name, COLOR_BOT);

                    }
                    else
                    {
                        m = Global.moveLookup("Hidden Power " + type);
                        moves[i] = m;
                    }


                }
                else if (Global.moveLookup(name[0]).type.value == "normal")
                {
                    string nname = name[0]+" (" + type + ")";
                    if (!Global.moves.ContainsKey(nname))
                    {
                        //This handles all normal type moves affected by -ate abilities.
                        //I think it also handles Normalize as well.
                        m = new Move(nname, types[type.ToLower()]);
                        Move analog = Global.moveLookup(name[0]);
                        m.group = analog.group;
                        /* Check for -ate abilities by comparing the original type to the one we have.
                         * Add the 30% boost to the base power so no need to calc it later. */
                        if(m.type != analog.type)
                            m.bp = analog.bp + (analog.bp * 0.3f);
                        Global.moves.Add(m.name, m);
                        moves[i] = m;
                        cwrite("Move " + i.ToString() + " " + m.name, COLOR_BOT);
                    }
                    else
                    {
                        m = Global.moveLookup(nname);
                        moves[i] = m;
                    }

                }
                else
                {
                    if (Global.moves.ContainsKey(name[0]))
                        m = Global.moves[name[0]];
                    else
                    {
                        cwrite("Unknown move " + name[0], COLOR_WARN);
                        m = new Move(name[0], Global.types[type.ToLower()]);
                    }
                    moves[i] = m;
                    cwrite("Move " + i.ToString() + " " + name[0], COLOR_BOT);
               }
             }
             return moves;
         }


        /// <summary>
        /// Gets either active pokemon, but defaults to getting
        /// the opponent's (rightbar)
        /// </summary>
        /// <param name="barclass">leftbar retrives the Player's active Pokemon</param>
        /// <returns></returns>
         protected Pokemon getActivePokemon(string barclass="rightbar")
         {
             //I feel like there's an easier way to do this.

             cwrite("Getting active Pokemon");
             
             var elems = waitFind(By.ClassName(barclass),10);
             if (elems == null) return Global.lookup("error");
             IList<IWebElement> ticon = elems.FindElements(By.ClassName("teamicons"));
             string temp = parseNameFromPage(ticon);
             if (temp == "0")
             {
                //error!
                 return lookup("error");
             }
             ////Found the name, now look it up in the dex.
             cwrite("The current pokemon is "+temp);
             Pokemon p = Global.lookup(temp);
           
             return p;
         }


         /// <summary>
         /// Alias for getActivePokemon("leftbar")
         /// </summary>
         /// <returns></returns>
         protected Pokemon updateYourPokemon()
         {
             Pokemon p = getActivePokemon("leftbar");
             
             return p;
         }

         protected string parseNameFromPage(IList<IWebElement> ticons)
         {
             for(int i = 0; i<ticons.Count;i++)
             {
                
                 IWebElement e = ticons[i];
                 IList<IWebElement> elems;
                try
                {
                    elems = e.FindElements(By.ClassName("pokemonicon"));
                }
                catch(StaleElementReferenceException)
                {
                    continue;
                }
                 foreach (IWebElement s in elems)
                 {
                    if (s.GetAttribute("title").Contains("(active)"))
                     {
                        string[] name;
                        try
                        {
                            name = s.GetAttribute("title").Split(' ');
                            
			            }
                        catch(StaleElementReferenceException)
                        {
                            cwrite("Unable to determine active pokemon, maybe it fainted.", "debug", COLOR_WARN);
                            break;
                        }
                         //Nicknamed pokemon appear in the html as "Nickname (Pokemon) (active)"
                         //this means that the pokemon's name should be N-2, which should hold
                         //true even for non-named mons.
                          
			             string n_name = name[name.Length - 2].Trim('(', ')'); //gets a sanitized name.
                        if (name.Length >= 3)
                        {
                            string cleanold = name[name.Length - 3].Trim('(', ')');
                            if (n_name == "Mime" && cleanold == "Mr.")
                                return "mr. mime";
                            else if (n_name == "Jr." && cleanold == "Mime")
                                return "mime jr.";
                        }
                         return n_name.ToLower();
                     }
                 }
             }
             
             return "0"; //return indicator that we did not find it.
         }

        protected List<string> parseAllNamesFromPage(IList<IWebElement> ticons)
        {
            List<string> names_list = new List<string>();
            for (int i = 0; i < ticons.Count; i++)
            {
                IWebElement e = ticons[i];
                IList<IWebElement> elems;
                try
                {
                   elems  = e.FindElements(By.ClassName("pokemonicon"));
                }
                catch (StaleElementReferenceException)
                {
                    continue;
                }
                foreach (IWebElement s in elems)
                {
                    if (s.GetAttribute("title") != "Not revealed")
                    {
                        string[] name;
                        try
                        {
                            //todo this fails on formats without picking a lead like randombattle
                            name = s.GetAttribute("title").Split(' ');

                        }
                        catch (StaleElementReferenceException)
                        {
                            cwrite("Unable to determine some pokemon on a team.", "debug", COLOR_WARN);
                            break;
                        }
                        //Nicknamed pokemon appear in the html as "Nickname (Pokemon)"
                        //this means that the pokemon's name should be N-x, which should hold
                        //true even for non-named mons.
                        int x; //Index distance from name.Length which contains the Pokemon's actual name.
                        //Active pokemon never have (hp%) or (hp%|status), therefore "x" should be the same for all.
                        if (name.Contains("(active)") || name.Contains("(fainted)") || name[name.Length-1].IndexOf('%') != -1
                            || name[name.Length-1].IndexOf('|') != -1)
                        {
                            x = 2;
                        }
                        else
                            x = 1;
                        string n_name = name[name.Length - x].Trim('(', ')'); //gets a sanitized name.
                        if (name.Length >= 2)
                        {
                            string cleanold = name[name.Length - 2].Trim('(', ')');
                            if (n_name == "Mime" && cleanold == "Mr.")
                                names_list.Add("mr. mime");
                            else if (n_name == "Jr." && cleanold == "Mime")
                                names_list.Add("mime jr.");
                            else
                                names_list.Add(n_name.ToLower());
                        }
                        else if (n_name == "")
                        {
                            continue;
                        }
                        else
                            names_list.Add(n_name.ToLower());
                    }
                }
                }
            return names_list;
         }
        
         /// <summary>
         /// Randomly selects a pokemon.
         /// </summary>
         /// <returns>Index of pokemon.</returns>
         protected int pickPokeRandomly()
         {
             Random rand = new Random();

             HashSet<int> exclude = new HashSet<int>();
             int choice = rand.Next(1, 5);
             cwrite("Choosing new pokemon");
             choice = rand.Next(1, 5);
            
             while (!elementExists(By.CssSelector("button[value='"+choice.ToString()+"']")))
             {
                 cwrite("Bad pokemon " + choice.ToString() + ". Rolling for another.","debug", COLOR_BOT);

                 exclude.Add(choice); //Steer it in the right direction by removing bad choices.
                 choice = GetRandomExcluding(exclude, 1, 5);

             }
             return choice;
         }

         /// <summary>
         /// Gets a random number from the range, excluding all numbers in the hash set.
         /// </summary>
         /// <param name="ex">set of excluded numbers</param>
         protected int GetRandomExcluding(HashSet<int> ex, int min, int max)
         {
             var exclude = ex;
             var range = Enumerable.Range(min, max).Where(i => !exclude.Contains(i));

             var rand = new System.Random();
             int index = rand.Next(min - 1, (max - 1) - exclude.Count);
             return range.ElementAt(index);
         }

         protected bool checkBattleEnd()
         {
             if (elementExists(By.Name("closeAndMainMenu")))
             {
                 //The match is over
                 cwrite("The battle has ended! Returning to main menu.", COLOR_BOT);
                 browser.FindElement(By.Name("closeAndMainMenu")).Click();
                 activeState = State.IDLE;
                 return true;
             }
             return false;

         }

         /// <summary>
         /// Exits a battle and forfeits accordingly.
         /// </summary>
         /// <param name="forfeit">Go through steps to forfeit match?</param>
         /// <returns>Whether it was successful</returns>
         protected bool goMainMenu(bool forfeit)
         {

            if (forfeit)
            {
                //force the browser to click the exit button.
                if (elementExists(By.ClassName("closebutton")))
                    browser.FindElement(By.ClassName("closebutton")).Click();
                else
                    return false;
                wait();
                if (elementExists(By.XPath("//button[@type='submit']")))
                {
                    browser.FindElement(By.XPath("//button[@type='submit']")).Click();
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (elementExists(By.ClassName("closebutton")))
                {
                    browser.FindElement(By.ClassName("closebutton")).Click();
                    return true;
                }
                else
                    return false;
                
            }
         }

        #endregion


        public bool forfeitBattle()
        {
            if (!goMainMenu(true))
            {
                cwrite("Unable to forfeit.", "!", COLOR_WARN);
                return false;
            }
            else
            {
                cwrite("Forfeited.", COLOR_BOT);
                changeState(State.IDLE);
                return true;
            }
        }

        public void changeState(State ns)
        {
            lastState = activeState;
            activeState = ns;
        }
        public State getState()
        {
            return activeState;
        }


        public virtual void printInfo()
        {
            cwrite("Generic Bot info:\n" +
                    "Format: " + format, COLOR_BOT);
        }

        public void setContinuous(bool v)
        {
            isContinuous = v;
        }
        public void setMaxBattles(int m)
        {
            maxBattles = m;
        }
        public void changeFormat(string nf)
        {
            format = nf;
        }

            
    }
}
