using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

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
        public BotModule(Bot m, IWebDriver b)
        {

            manager = m;
            browser = b;

            init();
        }

        public virtual void init()
        {
            activeState = State.IDLE;
            c = manager.getConsole();

        }
        public virtual void Update()
        {
           
            if (activeState == State.IDLE)
            {
                System.Threading.Thread.Sleep(5000);
            }
            else if (activeState == State.CHALLENGE)
            {
                challengePlayer(manager.getOwner(), format);
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

            c.write("Searching for " + player);
            System.Threading.Thread.Sleep(2000);
            browser.FindElement(By.Name("finduser")).Click();
            System.Threading.Thread.Sleep(2000);
            IWebElement e = browser.FindElement(By.Name("data"));
            e.SendKeys(player);
            System.Threading.Thread.Sleep(2000);
            e.Submit();
            System.Threading.Thread.Sleep(2000);
            c.write("Contacting user for OU battle");
            System.Threading.Thread.Sleep(2000);
            browser.FindElement(By.Name("challenge")).Click();
            System.Threading.Thread.Sleep(2000);
            browser.FindElement(By.Name("format")).Click();
            System.Threading.Thread.Sleep(2000);
            browser.FindElement(By.CssSelector("button[name='selectFormat'][value='" + format + "']")).Click();
            System.Threading.Thread.Sleep(2000);
            browser.FindElement(By.Name("makeChallenge")).Click();
            ////TODO: implement a way to select alternate teams/ have more than one team.
            c.writef("Battle starting!", Global.botInfoColor);
            changeState(State.BATTLE);

        }


        #region Battle Information Functions
        /// <summary>
        /// Checks whether it's possible to switch.
        /// </summary>
        /// <returns>can switch?</returns>
         protected bool checkSwitch()
        {
            if (browser.FindElements(By.Name("chooseMove")).Count != 0 &&
                browser.FindElements(By.Name("chooseSwitch")).Count !=0 &&
                browser.FindElements(By.Name("undoChoice")).Count != 0)
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
             
              if (browser.FindElements(By.Name("chooseMove")).Count != 0)
                     return true;
                 else
                     return false;
             
         }


         protected Move[] getMoves()
         {
             //todo deal with moves with no pp/disabled
             Move[] moves = new Move[4];
             for (int i = 0; i < 4; i++)
             {
                 var b = browser.FindElement(By.CssSelector("button[name='chooseMove'][value='"+(i+1).ToString()+"']"));
                 string[] html = b.GetAttribute("outerhtml").Split(new string[]{"data-move=\""},StringSplitOptions.None);
                 var nametag = Array.Find(html, s => s.StartsWith("data-move"));
                 string[] name = html[1].Split('"');
                 string[] temp = b.GetAttribute("class").Split('-');
                 string type = temp[1];

                // moves [i] =
                 
                 Move m;
                 if (Global.moves.ContainsKey(name[0]))
                     m = Global.moves[name[0]];
                 else
                 {
                     c.writef("Unknown move " + name[0], Global.warnColor);
                     m = new Move(name[0], Global.types[type.ToLower()]);
                 }
                 moves[i] = m;
                 //   moves[i] = lookupMove(name[0], Global.types[type.ToLower()]);
                  c.writef("Move " + i.ToString() + " " + name[0], Global.botInfoColor);

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

             c.write("Getting active Pokemon");
             var elems = browser.FindElement(By.ClassName(barclass));
             IList<IWebElement> ticon = elems.FindElements(By.ClassName("teamicons"));
             string temp = parseNameFromPage(ticon);
             if (temp == "0")
             {
                //error!
                 return null;
             }
             ////Found the name, now look it up in the dex.
             c.write("The current pokemon is "+temp);
             Pokemon p = Global.lookup(temp);
           
             return p;
         }
         protected Pokemon updateYourPokemon()
         {
             Pokemon p = getActivePokemon("leftbar");
             //do other stuff that may be useful here
             return p;
         }

         protected string parseNameFromPage(IList<IWebElement> ticons)
         {
             for(int i = 0; i<ticons.Count;i++)
             {
                 IWebElement e = ticons[i];
                 IList<IWebElement> elems = e.FindElements(By.ClassName("pokemonicon"));
                 foreach (IWebElement s in elems)
                 {
                    if (s.GetAttribute("title").Contains("(active)"))
                     {
                         string[] name = s.GetAttribute("title").Split(' ');
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


         /// <summary>
         /// Randomly selects a pokemon.
         /// </summary>
         /// <returns>Index of pokemon.</returns>
         protected int pickPokeRandomly()
         {
             Random rand = new Random();

             HashSet<int> exclude = new HashSet<int>();
             int choice = rand.Next(1, 5);
             c.write("Choosing new pokemon");
             choice = rand.Next(1, 5);
            
             while (browser.FindElements(By.CssSelector("button[value='"+choice.ToString()+"']")).Count == 0)
             {
                 c.writef("Bad pokemon " + choice.ToString() + ". Rolling for another.", Global.botInfoColor);

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
             if (browser.FindElements(By.Name("closeAndMainMenu")).Count != 0)
             {
                 //The match is over
                 c.writef("The battle has ended! Returning to main menu.", Global.botInfoColor);

                 activeState = State.IDLE;
                 return true;
             }
             return false;

         }

         /// <summary>
         /// Should be used when exiting a battle, prematurely or otherwise.
         /// </summary>
         /// <param name="b"></param>
         /// <param name="forfeit">Go through steps to forfeit match?</param>
         /// <returns>Whether it forfeited</returns>
         protected bool goMainMenu(bool forfeit)
         {

             if (forfeit)
             {
                 //force the browser to click the exit button.
                 browser.FindElement(By.ClassName("closebutton")).Click();
                 return true;
             }
             else
                 return false;
         }

        #endregion


         

         public void changeState(State ns)
        {
            activeState = ns;
        }
        public State getState()
        {
            return activeState;
        }


        

        protected void wait(int timeInMiliseconds)
        {
            System.Threading.Thread.Sleep(timeInMiliseconds);
        }
        protected void wait()
        {
            //basic wait of 2 seconds
            wait(2000);
        }
            
    }
}
