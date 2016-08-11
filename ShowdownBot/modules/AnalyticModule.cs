using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
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
            ACTION_RECOVER,
            ACTION_SLEEPTALK,
            ACTION_SWITCH
        };
        private LastBattleAction lastAction =LastBattleAction.ACTION_ATTACK_SUCCESS;
        protected Move lastMove;
        protected List<BattlePokemon> myTeam;
        protected List<BattlePokemon> enemyTeam;
        protected BattlePokemon errormon;
        protected BattlePokemon currentActive;
        protected int turnsSpentSleepTalking;

        public AnalyticModule(Bot m, IWebDriver b) : base(m,b)
        {
            format = "ou";
            myTeam = new List<BattlePokemon>();
            enemyTeam = new List<BattlePokemon>();
            errormon = new BattlePokemon(Global.lookup("error"));
            currentActive = errormon;
            turnsSpentSleepTalking = 0;
        }

        /// <summary>
        /// Conversion method to get the BattlePokemon version of a particular Pokemon in 
        /// a specific team.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private BattlePokemon getPokemon(Pokemon p,List<BattlePokemon> team)
        {
            for (int i = 0; i < team.Count; i++)
            {
                if (team[i].mon.name == p.name)
                    return team[i];
            }
            return errormon;
        }
        
        public override void battle()
        {
            int turn = 1;
            
            wait(5000); //give battle time to load
            buildTeams();
            //Select lead
            string lead;
            cwrite("Selecting first pokemon as lead", COLOR_BOT);

            //TODO: actually pick this analytically.
            if (elementExists(By.CssSelector("button[name='chooseTeamPreview']")))
            {
                lead = browser.FindElement(By.CssSelector("button[name='chooseTeamPreview'][value='0']")).Text;
                browser.FindElement(By.CssSelector("button[name='chooseTeamPreview'][value='0']")).Click();
            }
            else
                lead = "error";

            BattlePokemon active = null;//Global.lookup(lead);
            BattlePokemon enemy = null;
            do
            {
                   
                    wait();
                    enemy = getPokemon(getActivePokemon(),enemyTeam);
                    active = getPokemon(updateYourPokemon(),myTeam);
                    updateActiveStatuses(ref active,ref enemy);
                    currentActive = active;
                    battleAnalytic(ref active, enemy, ref turn);
                

            } while (activeState == State.BATTLE); 
        }

        public virtual bool buildTeams()
        {
            cwrite("Building teams.");
            var elems = waitFind(By.ClassName("leftbar")); //player
            IList<IWebElement> ticon = elems.FindElements(By.ClassName("teamicons"));
            List<string> names = parseAllNamesFromPage(ticon);
            for (int i = 0; i<names.Count;i++)
            {
                myTeam.Add(new BattlePokemon(Global.lookup(names[i])));
            }
            elems = waitFind(By.ClassName("rightbar")); //opponent
            ticon = elems.FindElements(By.ClassName("teamicons"));
            names = parseAllNamesFromPage(ticon);
            for (int i = 0; i < names.Count; i++)
            {
               enemyTeam.Add(new BattlePokemon(Global.lookup(names[i])));
            }
            return true;

        }

        private void updateTeamStatuses()
        {

        }

        private void updateHealth(IWebElement statbar, ref BattlePokemon p)
        {
            var elem = findWithin(statbar, By.ClassName("hptext"));
            if (elem != null)
            {
                int pct = 100;
                string txt = elem.Text.Trim('%');
                int.TryParse(txt, out pct);
                p.setHealth(pct);
            }
        }

        private void updateActiveStatuses (ref BattlePokemon you, ref BattlePokemon opponent)
        {
            var yourStats = waitFind(By.CssSelector("div[class='statbar rstatbar']"),1);
            if (yourStats == null) return;
            if (findWithin(yourStats, By.ClassName("par")) != null) you.status = Status.STATE_PAR;
            else if (findWithin(yourStats, By.ClassName("psn")) != null) you.status = Status.STATE_TOX;
            else if (findWithin(yourStats, By.ClassName("brn")) != null) you.status = Status.STATE_BRN;
            else if (findWithin(yourStats, By.ClassName("slp")) != null) you.status = Status.STATE_SLP;
            else if (findWithin(yourStats, By.ClassName("frz")) != null) you.status = Status.STATE_FRZ;
            else you.status = Status.STATE_HEALTHY;
            updateHealth(yourStats, ref you);


            var oppStats = waitFind(By.CssSelector("div[class='statbar lstatbar']"));
            if (oppStats == null) return;
            if (findWithin(oppStats, By.ClassName("par")) != null) opponent.status = Status.STATE_PAR;
            else if (findWithin(oppStats, By.ClassName("psn")) != null) opponent.status = Status.STATE_TOX;
            else if (findWithin(oppStats, By.ClassName("brn")) != null) opponent.status = Status.STATE_BRN;
            else if (findWithin(oppStats, By.ClassName("slp")) != null) opponent.status = Status.STATE_SLP;
            else if (findWithin(oppStats, By.ClassName("frz")) != null) opponent.status = Status.STATE_FRZ;
            else opponent.status = Status.STATE_HEALTHY;
            updateHealth(oppStats, ref opponent);
        }
        private bool battleAnalytic(ref BattlePokemon active, BattlePokemon enemy, ref int turn)
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
                    cwrite("Can't find new pokemon, unable to continue.", "[ERROR]", COLOR_ERR);
                    return true;
                }
                return false;
            }
            //Preemptively switch out of bad situations
            else if (needSwitch(active, enemy))
            {

                cwrite("I'm switching out.", "Turn " + turn.ToString(), COLOR_BOT);
                wait();
                BattlePokemon temp = pickPokeAnalytic(enemy);
                if (temp == null)
                {
                    cwrite("Couldn't pick a pokemon. Going with moves instead.", "[!]", COLOR_WARN);
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
                if (elementExists(By.Name("megaevo")))
                    browser.FindElement(By.Name("megaevo")).Click();
                //for now, automatically activate mega
                string mv = pickMoveAnalytic(active, enemy);
                cwrite("I'm picking move " + mv, "Turn " + turn.ToString(), COLOR_BOT);
                cwrite("Last Move: " + lastAction.ToString(), "DEBUG", COLOR_OK);

                turn++;
            }

            else
                wait();

            return false;
        }

        //TODO: add moves to each BattlePokemon as they're encountered so that its not dependent on the web elements.
        private BattlePokemon pickPokeAnalytic(BattlePokemon enemy)
        {
            //Loop over all pokemon
            int bestChoice = 1000;
            float highestdamage = 5000f;
            wait();
            for (int i = 1; i <= 5; i++)
            {
                if (!elementExists(By.CssSelector("button[value='" + i.ToString() + "'][name='chooseSwitch']")))
                    continue;
                BattlePokemon p = getPokemon(Global.lookup(browser.FindElement(By.CssSelector("button[value='" + i.ToString() + "'][name='chooseSwitch']")).Text),myTeam);
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
                 cwrite(p.mon.name + " value:" + temp, "[DEBUG]", COLOR_OK);




            }
            
            var b = browser.FindElement(By.CssSelector("button[value='" + bestChoice.ToString() + "'][name=chooseSwitch]"));
            BattlePokemon nextPoke = getPokemon(Global.lookup(b.Text),myTeam);
            b.Click();
            return nextPoke;
        }

        private string pickMoveAnalytic(BattlePokemon you, BattlePokemon enemy)
        {
            float[] rankings = new float[4]; //ranking of each move
            float bestMove = 0f;
            float RANK_MAX = 255;
            int choice = 1;
            float risk = you.matchup(enemy);
            Move[] moves = getMoves();
            for (int i = 0; i < 4; i++)
            {
               
                if (moves[i].bp == 0 || moves[i].bp == -1)
                {
                    if (moves[i].heal && getRecoverChance(you,enemy) > new Random().NextDouble())
                        rankings[i] = 150 + (100 - you.getHPPercentage());
                    //Sleep talk if asleep, but never more than twice in a row.
                    //Must use name.contains due to the way normal moves are added ( with (type) appended).
                    else if (moves[i].name.Contains("Sleep Talk") && you.status == Status.STATE_SLP && turnsSpentSleepTalking < 2)
                        rankings[i] = RANK_MAX;



                }
                else if (moves[i].bp != 0)
                {
                    rankings[i] = you.damageCalcTotal(moves[i], enemy);


                }
                cwrite(moves[i].name + "'s rank: " + rankings[i].ToString(), "[DEBUG]", COLOR_OK);

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
            setLastBattleAction(chosenMove);

            if (chosenMove.name.Contains("Sleep Talk"))
                turnsSpentSleepTalking++;
            else
                turnsSpentSleepTalking = 0;

            if (elementExists(By.CssSelector("button[value='" + choice.ToString() + "'][name='chooseMove']")))
            {
                browser.FindElement(By.CssSelector("button[value='" + choice.ToString() + "'][name='chooseMove']")).Click();
                return chosenMove.name;
            }
            else
                return "no move";

        }


        private void setLastBattleAction(Move m)
        {
            if (m.boost) lastAction = LastBattleAction.ACTION_BOOST;
            else if (m.name.Contains("Sleep Talk"))
            {
                lastAction = LastBattleAction.ACTION_SLEEPTALK;
            }
            else
                lastAction = LastBattleAction.ACTION_ATTACK_SUCCESS;

        }

        /// <summary>
        /// Likelihood that the pokemon should recover this turn.
        /// </summary>
        private float getRecoverChance(BattlePokemon you,BattlePokemon e)
        {
            int hpThreshold = 40; //Percent of health at which to conisder recovering.
            float chance = 0.0f;

            if (you.getHPPercentage() <= hpThreshold) chance += 0.3f;
            if (you.checkKOChance(e) < 0.3f) chance += 0.2f; //heal if we can't OHKO opponent.
            if (you.status != Status.STATE_HEALTHY && lastAction != LastBattleAction.ACTION_SLEEPTALK) chance += 0.2f;
            if (lastAction == LastBattleAction.ACTION_RECOVER) chance -= 0.2f;

            return chance;
        }


        private bool needSwitch(BattlePokemon you, BattlePokemon enemy)
        {
            if (isLastMon())
                return false;
            //If the cancel button exists, we have either already made our switch, or have made a move this turn.
            if (elementExists(By.Name("undoChoice")))
                return false;
            //if the pokemon is at low health, don't bother
            if (you.getHPPercentage() <= 25)
                return false;
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
                
                if (elementExists(By.CssSelector("button[value='"+i.ToString()+"']")))
                    totalMons++;
            }
            if (totalMons == 0)
                return true;
            else
                return false;
        }

        public override void printInfo()
        {
            cwrite("Analytic info:\n" +
                    "Format: " + format, COLOR_BOT);
            if (activeState == State.BATTLE)
            {
                cwrite("Battle: " + browser.Url +
                    "\nCurrent Pokemon: " + currentActive.mon.name +
                    "\n\tHP: " + currentActive.getHealth() + "/" + currentActive.maxHealth+
                    "\n\tStatus: " + currentActive.status,COLOR_BOT);
            }
        }

    }
}
