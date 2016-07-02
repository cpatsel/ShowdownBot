using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

namespace ShowdownBot.modules
{
    class BiasedModule : BotModule
    {

        float M1WGT;
        float M2WGT;
        float M3WGT;
        float M4WGT;
        float weightTotal;
        
        public BiasedModule(Bot m, IWebDriver b)
            : base(m, b)
        {
            format = "ou";
            M1WGT = Global.m1wgt;
            M2WGT = Global.m2wgt;
            M3WGT = Global.m3wgt;
            M4WGT = Global.m4wgt;
            weightTotal = (M1WGT + M2WGT + M3WGT + M4WGT);
        }

        public override void battle()
        {
            int turn = 1;
            wait(10000);
            do
            {
               battleBiased(ref turn);

            } while (activeState == State.BATTLE);

            //Done battling, but the battle isn't over.

            if (activeState == State.IDLE && !checkBattleEnd())
            {
                goMainMenu(true);
            }
            
        }



        private bool battleBiased(ref int turn)
        {
            int moveSelection;
            int pokeSelection;

            if (checkMove())
            {
                if (browser.FindElements(By.Name("megaevo")).Count != 0)
                {
                    browser.FindElement(By.Name("megaevo")).Click();
                }
                wait();
                moveSelection = pickMoveBiased();
                c.writef("I'm selecting move " + moveSelection.ToString(), "[TURN " + turn.ToString() + "]", Global.botInfoColor);
                browser.FindElement(By.CssSelector("button[value='" + moveSelection.ToString() + "']")).Click();
                turn++;
            }
            else if (checkSwitch())
            {
                //TODO: check if it's the first turn, and then select appropriate lead.
                c.writef("Switching pokemon.", Global.botInfoColor);
                pokeSelection = pickPokeRandomly();
                c.writef("New pokemon selected: " + pokeSelection.ToString(), Global.botInfoColor);
                browser.FindElement(By.CssSelector("button[value='" + pokeSelection.ToString() + "']")).Click();
                wait();
            }
            else if (checkBattleEnd())
            {
                return true;
            }
            else
            {
                wait();
            }
            return false;
        }

        private int pickMoveBiased()
        {
            HashSet<int> exclude = new HashSet<int>();
            int choice;
            choice = getIndexBiased();
            while (browser.FindElements(By.CssSelector("button[value='"+choice.ToString()+"']")).Count == 0)
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
        /// <returns>Choice index based on the specified weights.</returns>
        private int getIndexBiased()
        {
            int choice = 1;
            Random rand = new Random();
            float cumulative = 0.0f;
            float percent = (float)rand.NextDouble()*(weightTotal);
            c.writef("Choosing move that meets " + percent.ToString(), "debug", Global.okColor);
            List<float> weights = new List<float>{ M1WGT, M2WGT, M3WGT, M4WGT };
            weights.Sort();
            foreach (float wgt in weights)
            {
                percent -= wgt;
                if (percent <= 0)
                    return 5-choice;
                choice++;
            }
            return choice;

        }

        public override void printInfo()
        {
            c.writef("Biased mode info:\n" +
                    "Format: " + format +
                    "\nMove weight 1: "+M1WGT+
                    "\nMove weight 2: "+M2WGT+
                    "\nMove weight 3: "+M3WGT+
                    "\nMove weight 4: "+M4WGT+
                    "\nTotal: "+weightTotal
                    ,Global.botInfoColor);
        }
    }
}
