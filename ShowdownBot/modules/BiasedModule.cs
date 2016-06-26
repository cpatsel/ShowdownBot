using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

namespace ShowdownBot.modules
{
    class BiasedModule : BotModule
    {

        const float M1WGT = 0.4f;
        const float M2WGT = 0.3f;
        const float M3WGT = 0.2f;
        const float M4WGT = 0.1f;

        public BiasedModule(Bot m, IWebDriver b)
            : base(m, b)
        {

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
            int choice;
            Random rand = new Random();
            float percent = (float)rand.NextDouble();
            if (percent >= 0 && percent <= M4WGT)
                choice = 4;
            else if (percent > M4WGT && percent <= M3WGT)
                choice = 3;
            else if (percent > M3WGT && percent <= M2WGT)
                choice = 2;
            else
                choice = 1;

            return choice;
        }
    }
}
