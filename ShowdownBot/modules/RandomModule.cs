using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
namespace ShowdownBot.modules
{
    class RandomModule: BotModule
    {
        public RandomModule(Bot m, IWebDriver b)
            : base(m, b)
        {
            format = "gen7randombattle";
            
        }

        public override void battle()
        {
            int turn = 1;
            pickLead();
            
            do
            {
                battleRandomly(ref turn);
              
            } while (activeState == State.BATTLE);

            //Done battling, but the battle isn't over.

            if (activeState == State.IDLE && !checkBattleEnd())
            {
                goMainMenu(true);
            }
            

        }

        public override string pickLead()
        {
           string lead;
            
            if (elementExists(By.CssSelector("button[name='chooseTeamPreview']")))
            {
                cwrite("Selecting random pokemon for lead.", COLOR_BOT);
                int val = new Random().Next(0, 5);
                lead = waitFind(By.CssSelector("button[name='chooseTeamPreview'][value='"+val+"']")).Text;
                waitFindClick(By.CssSelector("button[name='chooseTeamPreview'][value='"+val+"']"));
            }
            else
                lead = "error";

            return lead;
        }

        private bool battleRandomly(ref int turn)
        {
            
            int moveSelection;
            int pokeSelection;
            if (checkMove())
            {

                //first check if there's a mega evo option

                if (elementExists(By.Name("megaevo")))
                    browser.FindElement(By.Name("megaevo")).Click();

                moveSelection = determineMoveRandomly();
                cwrite("I'm selecting move " + moveSelection.ToString(), "[TURN " + turn.ToString() + "]", COLOR_BOT);
                //  browser.Button(Find.ByValue(moveSelection.ToString())).Click(); //Select move
                browser.FindElement(By.CssSelector("button[value='" + moveSelection.ToString() + "']")).Click();
                wait();
                turn++;
            }
            else if (checkSwitch())
            {

                cwrite("Switching pokemon.", COLOR_BOT);
                pokeSelection = pickPokeRandomly();
                cwrite("New pokemon selected: " + pokeSelection.ToString(), COLOR_BOT);
                browser.FindElement(By.CssSelector("button[value='" + pokeSelection.ToString() + "']")).Click();
                wait();
            }
            else if (checkBattleEnd())
            {
                return true;
            }
            else
            {
                //cwrite("Sleeping for 2 secs","debug",Global.defaultColor);
                wait();
            }
            return false;
        }

        private int determineMoveRandomly()
        {
            Random rand = new Random();
            HashSet<int> exclude = new HashSet<int>();
            int choice = rand.Next(1, 4);

            while (!elementExists(By.CssSelector("button[name=chooseMove][value='" + choice.ToString() + "']")))
            {
                cwrite("Bad move choice: " + choice.ToString() + "Picking another", "[DEBUG]", COLOR_OK);
                exclude.Add(choice);
                choice = GetRandomExcluding(exclude, 1, 4);
            }
            return choice;

        }
    }
}
