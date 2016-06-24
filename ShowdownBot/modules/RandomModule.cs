using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

namespace ShowdownBot.modules
{
    class RandomModule: BotModule
    {
        public RandomModule(Bot m, IWebDriver b)
            : base(m, b)
        {
            format = "randombattle";
            
        }

        public override void battle()
        {
            int turn = 1;
            wait(10000);
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

        private bool battleRandomly(ref int turn)
        {
            
            int moveSelection;
            int pokeSelection;
            if (checkMove())
            {

                //first check if there's a mega evo option

                if (browser.FindElements(By.Name("megaevo")).Count != 0)
                    browser.FindElement(By.Name("megaevo")).Click();

                moveSelection = determineMoveRandomly();
                c.writef("I'm selecting move " + moveSelection.ToString(), "[TURN " + turn.ToString() + "]", Global.botInfoColor);
                //  browser.Button(Find.ByValue(moveSelection.ToString())).Click(); //Select move
                browser.FindElement(By.CssSelector("button[value='" + moveSelection.ToString() + "']")).Click();
                System.Threading.Thread.Sleep(2000);
                turn++;
            }
            else if (checkSwitch())
            {

                c.writef("Switching pokemon.", Global.botInfoColor);
                pokeSelection = pickPokeRandomly();
                c.writef("New pokemon selected: " + pokeSelection.ToString(), Global.botInfoColor);
                browser.FindElement(By.CssSelector("button[value='" + pokeSelection.ToString() + "']")).Click();
                System.Threading.Thread.Sleep(2000);
            }
            else if (checkBattleEnd())
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

        private int determineMoveRandomly()
        {
            Random rand = new Random();
            HashSet<int> exclude = new HashSet<int>();
            int choice = rand.Next(1, 4);

            while (browser.FindElements(By.CssSelector("button[value='" + choice.ToString() + "']")).Count == 0)
            {
                c.writef("Bad move choice: " + choice.ToString() + "Picking another", "[DEBUG]", Global.okColor);
                exclude.Add(choice);
                choice = GetRandomExcluding(exclude, 1, 4);
            }
            return choice;

        }
    }
}
