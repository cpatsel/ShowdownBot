using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

namespace ShowdownBot.modules
{
    class AnalyticModule : BotModule
    {
        public enum LastBattleAction
        {
            ACTION_ATTACK_SUCCESS,
            ACTION_ATTACK_FAILURE,
            ACTION_STATUS,
            ACTION_BOOST,
            ACTION_SWITCH
        };
        private LastBattleAction lastAction =LastBattleAction.ACTION_ATTACK_SUCCESS;

        public AnalyticModule(Bot m, IWebDriver b) : base(m,b)
        {
            format = "ou";
            
        }

        
        public override void battle()
        {
            int turn = 1;
            
            wait(5000); //give battle time to load
            //Select lead
            string lead;
            c.writef("Selecting first pokemon as lead", Global.botInfoColor);
            System.Threading.Thread.Sleep(5000); //let page load

            //TODO: actually pick this analytically.
            lead = browser.FindElement(By.CssSelector("button[name='chooseTeamPreview'][value='0']")).Text;
            browser.FindElement(By.CssSelector("button[name='chooseTeamPreview'][value='0']")).Click();

            Pokemon active = Global.lookup(lead);
            Pokemon enemy = null;
            do
            {
                   
                    wait();
                    enemy = getActivePokemon();
                    active = updateYourPokemon();
                    
                    battleAnalytic(ref active, enemy, ref turn);
                

            } while (activeState == State.BATTLE); 
        }

        private bool battleAnalytic(ref Pokemon active, Pokemon enemy, ref int turn)
        {
            //Extra check to make sure we pick a lead.
            //Possibly redundant
            if (enemy == null)
            {
                if (checkSwitch())
                {

                    browser.FindElement(By.CssSelector
                        ("button[value='" + pickPokeRandomly().ToString() + "'][name='chooseSwitch']"));
                    
                    //todo change this to analytic pick
                }
                else
                    return false;
            }

            if (checkBattleEnd())
            {
                return true;
            }

            //Switch if fainted
            else if (checkSwitch())
            {
                active = pickPokeAnalytic(enemy);
                if (active == null)
                {
                    c.writef("Can't find new pokemon, unable to continue.", "[ERROR]", Global.errColor);
                    return true;
                }
                return false;
            }
            //Preemptively switch out of bad situations
            else if (needSwitch(active, enemy))
            {

                c.writef("I'm switching out.", "Turn " + turn.ToString(), Global.botInfoColor);
                wait();
                Pokemon temp = pickPokeAnalytic(enemy);
                if (temp == null)
                {
                    c.writef("Couldn't pick a pokemon. Going with moves instead.", "[!]", Global.warnColor);
                    pickMoveAnalytic(active, enemy);

                }
                else
                {
                    active = temp;
                }
                turn++;
                return false;

            }
            else if (checkMove())
            {
                if (browser.FindElements(By.Name("megaevo")).Count != 0)
                    browser.FindElement(By.Name("megaevo")).Click();
                //for now, automatically activate mega
                string mv = pickMoveAnalytic(active, enemy);
                c.writef("I'm picking move " + mv, "Turn " + turn.ToString(), Global.botInfoColor);
                c.writef("Last Move: " + lastAction.ToString(), "DEBUG", Global.okColor);

                turn++;
            }

            else
                wait();

            return false;
        }

        private Pokemon pickPokeAnalytic(Pokemon enemy)
        {
            //Loop over all pokemon
            int bestChoice = 1000;
            float highestdamage = 5000f;
            wait();
            for (int i = 1; i <= 5; i++)
            {
                if (browser.FindElements(By.CssSelector("button[value='" + i.ToString() + "'][name='chooseSwitch']")).Count == 0)
                    continue;
                Pokemon p = Global.lookup(browser.FindElement(By.CssSelector("button[value='" + i.ToString() + "'][name='chooseSwitch']")).Text);
                if (bestChoice == 1000)
                    bestChoice = i; //set a default value that can be accessed.
                float temp = p.matchup(enemy);
                //negate the return here in order to coincide with the defensive switching in the loop above.
                //this should prevent eternally switching pokemon
                 temp += enemy.checkKOChance(p);
                 if (temp < highestdamage)
                 {
                     highestdamage = temp;
                     bestChoice = i;
                 }
                 c.writef(p.name + " value:" + temp, "[DEBUG]", Global.okColor);




            }
            
            var b = browser.FindElement(By.CssSelector("button[value='" + bestChoice.ToString() + "'][name=chooseSwitch]"));
            Pokemon nextPoke = Global.lookup(b.Text);
            b.Click();
            return nextPoke;
        }

        private string pickMoveAnalytic(Pokemon you, Pokemon enemy)
        {
            float[] rankings = new float[4]; //ranking of each move
            float bestMove = 0f;
            int choice = 1;
            float risk = you.matchup(enemy);
            Move[] moves = getMoves();
            for (int i = 0; i < 4; i++)
            {
                //For now, only determine the best attacking move.
                if (moves[i].bp == 0)
                {
                    if (moves[i].boost && (lastAction == LastBattleAction.ACTION_BOOST))
                        rankings[i] = 0;
                    //placeholder
                    else if (moves[i].boost && risk <= 1)
                        rankings[i] = 2;


                }
                else if (moves[i].bp != 0)
                {
                    rankings[i] = you.damageCalcTotal(moves[i], enemy);


                }
                c.writef(moves[i].name + "'s rank: " + rankings[i].ToString(), "[DEBUG]", Global.okColor);

            }
            for (int i = 0; i < 4; i++)
            {
                if (rankings[i] > bestMove)
                {
                    bestMove = rankings[i];
                    choice = i + 1;
                }
            }

            //figure out what move we've chosen
            Move chosenMove = moves[choice - 1];
            if (chosenMove.boost) lastAction = LastBattleAction.ACTION_BOOST;
            else
                lastAction = LastBattleAction.ACTION_ATTACK_SUCCESS;
            if (checkMove())
            {
                //  mainBrowser.Eval("$('button[name=chooseMove][value=" + choice.ToString() + "]').click()");
                browser.FindElement(By.CssSelector("button[value='" + choice.ToString() + "'][name='chooseMove']")).Click();
                return chosenMove.name;
            }
            else
                return "no move";

        }

        private bool needSwitch(Pokemon you, Pokemon enemy)
        {
            if (isLastMon())
                return false;
            //If the cancel button exists, we have either already made our switch, or have made a move this turn.
            if (browser.FindElements(By.Name("undoChoice")).Count != 0)
                return false;
            //if the pokemon is at low health, don't bother
            //WatiN.Core.Element e = mainBrowser.Element(Find.ByClass("critical"));
            //if (e.Exists)
            float tolerance = 2.5f;
            if (you.matchup(enemy) > tolerance)
                return true;
            else return false;
        }
        private bool isLastMon()
        {
            int totalMons = 0;
            for (int i = 1; i <= 5; i++)
            {
                
                if (browser.FindElements(By.CssSelector("button[value='"+i.ToString()+"']")).Count != 0)
                    totalMons++;
            }
            if (totalMons == 0)
                return true;
            else
                return false;
        }

    }
}
