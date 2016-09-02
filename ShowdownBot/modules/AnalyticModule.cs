using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
namespace ShowdownBot.modules
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

    class AnalyticModule : BotModule
    {
        
        protected LastBattleAction lastAction =LastBattleAction.ACTION_ATTACK_SUCCESS;
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
           // lastMove = moveLookup("error");
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
                else if (team[i].mon.name.Contains(p.name))
                {
                    return team[i]; //This handles cases like Tornadus-Therian, who would be in p.name as just "Tornadus"
                }
                else if (p.name.Contains("-mega") && p.name.Contains(team[i].mon.name))
                {
                    team[i].changeMon(p.name);
                    return team[i];
                }


            }
            return errormon;
        }
        
        public override void battle()
        {
            int turn = 1;
            
            wait(5000); //give battle time to load
            if (format == "randombattle")
                buildOwnTeam();
            buildTeams();


            //Select lead
            pickLead();

            BattlePokemon active = null;//Global.lookup(lead);
            BattlePokemon enemy = null;
            do
            {
                   
                    wait();
                    if(format == "randombattle")
                        buildTeams(); //if randombattle, check to see if any new pokemon have been revealed.
                    enemy = getPokemon(getActivePokemon(),enemyTeam);
                    active = getPokemon(updateYourPokemon(),myTeam);
                    updateActiveStatuses(ref active,ref enemy);
                    currentActive = active;
                    battleAnalytic(ref active, enemy, ref turn);
                

            } while (activeState == State.BATTLE);
            myTeam.Clear();
            enemyTeam.Clear(); 
        }


        /// <summary>
        /// Picks the last lead-role pokemon on the team, or if none are present,
        /// Compares scores of each mon against the other team's mons.
        /// Regardless, this method must be called after BuildTeams.
        /// </summary>
        /// <returns></returns>
        public override string pickLead()
        {
            string lead;
            int index = -1;
            if (elementExists(By.CssSelector("button[name='chooseTeamPreview']")))
            {
                for (int i = 0; i< myTeam.Count; i++)
                {
                    if (myTeam[i].mon.getRole().lead)
                        index = i;

                }
                
                if (index == -1)
                {
                    float[] scores = new float[myTeam.Count];
                    scores.Initialize();
                    for(int i = 0; i < enemyTeam.Count; i++)
                    {
                        for (int j = 0; j < myTeam.Count; j++)
                        {
                            scores[j] += myTeam[j].checkTypes(enemyTeam[i]);
                        }
                    }
                    //Find the highest score
                    float maxscore = 0;
                    for (int i = 0; i < scores.Length; i++)
                    {
                        if (scores[i] > maxscore)
                        {
                            maxscore = scores[i];
                            index = i;
                        }
                    }
                }
                
                lead = waitFind(By.CssSelector("button[name='chooseTeamPreview'][value='" + index + "']")).Text;
                waitFindClick(By.CssSelector("button[name='chooseTeamPreview'][value='" + index + "']"));
            }
            else
                lead = "error";

            return lead;
        }



        /// <summary>
        /// Iterates over the revealed pokemon and adds them to the team if they have not already been.
        /// </summary>
        /// <returns></returns>
        public virtual bool buildTeams()
        {
            cwrite("Updating teams.");
            var elems = waitFind(By.ClassName("leftbar")); //player
            IList<IWebElement> ticon = elems.FindElements(By.ClassName("teamicons"));
            //IList<IWebElement> ticon = findElementsFromWithin(elems, By.ClassName("teamicons"));
            List<string> names = parseAllNamesFromPage(ticon);
            for (int i = 0; i<names.Count;i++)
            {
                if (!myTeam.Any(bpkmn => bpkmn.mon.name == names[i])) 
                    myTeam.Add(new BattlePokemon(Global.lookup(PERSONAL_PRE+names[i])));
                ////attempt to look up our own custom pokemon
            }
            elems = waitFind(By.ClassName("rightbar")); //opponent
            //ticon = findElementsFromWithin(elems,By.ClassName("teamicons"));
            ticon = elems.FindElements(By.ClassName("teamicons"));
            names = parseAllNamesFromPage(ticon);
            for (int i = 0; i < names.Count; i++)
            {
                if (!enemyTeam.Any(bpkmn => bpkmn.mon.name == names[i]))
                    enemyTeam.Add(new BattlePokemon(Global.lookup(names[i])));
            }
            return true;

        }

        /// <summary>
        /// Populates myTeam from the switch menu, rather than from the team icons.
        /// This does not cover the currently active pokemon, so it should be used in
        /// conjunction with the regular buildTeam.
        /// </summary>
        private void buildOwnTeam()
        {
            var switchmenu = waitFind(By.ClassName("switchmenu"));
            var elems = switchmenu.FindElements(By.ClassName("chooseSwitch"));
            if (switchmenu != null)
            {
                string[] text = switchmenu.Text.Split('\r');
                foreach (string s in text)
                {
                    myTeam.Add(new BattlePokemon(Global.lookup(s.TrimStart('\n'))));
                }
            }
        }
        private void updateTeamStatuses()
        {

        }

        private void updateHealth(IWebElement statbar, ref BattlePokemon p)
        {
            var elem = findWithin(statbar, By.ClassName("hptext"));
            if (elem != null)
            {
                string txt = elem.Text;
                int pct = 100;
                txt = txt.Trim('%');
                int.TryParse(txt, out pct);
                p.setHealth(pct);
            }
        }

        /// <summary>
        /// Updates various information about the two active pokemon including status and health percentage.
        /// </summary>
        /// <param name="you"></param>
        /// <param name="opponent"></param>
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
                active = pickPokeAnalytic(enemy,true);
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
                BattlePokemon temp = pickPokeAnalytic(enemy,false);
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
        private BattlePokemon pickPokeAnalytic(BattlePokemon enemy,bool offense)
        {
            //Loop over all pokemon
            int bestChoice = 1000;
            float highestdamage = (offense)? 0 : 5000f;
            for (int i = 1; i <= 5; i++)
            {
                if (!elementExists(By.CssSelector("button[value='" + i.ToString() + "'][name='chooseSwitch']")))
                    continue;
                BattlePokemon p = getPokemon(Global.lookup(PERSONAL_PRE+waitFind(By.CssSelector("button[value='" + i.ToString() + "'][name='chooseSwitch']"),1).Text),myTeam);
                if (bestChoice == 1000)
                    bestChoice = i; //set a default value that can be accessed.

                float temp = 0;

                /* Depending if we are switching offensively or defensively compare either the potential
                 * new mon's aptitude to KO the opponent (offensive) or the opponent's to KO ours (defensive).
                 */
                if (offense)
                {
                    
                    temp = enemy.matchup(p);
                    temp += p.checkKOChance(enemy);
                    if (temp > highestdamage)
                    {
                        highestdamage = temp;
                        bestChoice = i;
                    }
                }
                else
                {
                    temp = p.matchup(enemy);
                    temp += enemy.checkKOChance(p);
                    if (temp < highestdamage)
                    {
                        highestdamage = temp;
                        bestChoice = i;
                    }
                }
                
                 cwrite(p.mon.name + " value:" + temp, "[DEBUG]", COLOR_OK);




            }
            
            var b = waitFind(By.CssSelector("button[value='" + bestChoice.ToString() + "'][name=chooseSwitch]"));
            if (b != null)
            {
                BattlePokemon nextPoke = getPokemon(Global.lookup(b.Text), myTeam);
                b.Click();
                return nextPoke;
            }
            return null;
        }

        private string pickMoveAnalytic(BattlePokemon you, BattlePokemon enemy)
        {
            float[] rankings = new float[4]; //ranking of each move
            float bestMove = 0f;
            int choice = 1;
            float risk = you.matchup(enemy);
            Move[] moves = getMoves();
            for (int i = 0; i < 4; i++)
            {

                //Sleep talk if asleep, but never more than twice in a row.
                //Must use name.contains due to the way normal moves are added ( with (type) appended).
                if (moves[i].name.Contains("Sleep Talk") && you.status == Status.STATE_SLP && turnsSpentSleepTalking < 2)
                    rankings[i] = MAX_MOVE_RANK;
                else
                    rankings[i] = you.rankMove(moves[i], enemy, enemyTeam, lastAction);

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
            if (m.isBoost) lastAction = LastBattleAction.ACTION_BOOST;
            else if (m.name.Contains("Sleep Talk"))
            {
                lastAction = LastBattleAction.ACTION_SLEEPTALK;
            }
            else if (m.status)
            {
                lastAction = LastBattleAction.ACTION_STATUS;
            }
            else
                lastAction = LastBattleAction.ACTION_ATTACK_SUCCESS;
            lastMove = m;
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
            float tolerance = 2.5f; //Default tolerance based on type matchups.
            if (you.currentBoosts.total() > 0)
                tolerance += 1.5f; //enemy must be much more dangerous to consider switching with boosts.
            else if (you.currentBoosts.total() < 0)
                tolerance -= 1f; //more likely to switch if drops are present.
            
            if (you.matchup(enemy) > tolerance)
                return true;
            else return false;
        }
        private bool isLastMon()
        {
            int totalMons = 0;
            for (int i = 1; i <= 5; i++)
            {
                
                if (elementExists(By.CssSelector("button[name='chooseSwitch'][value='"+i.ToString()+"']")))
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
