using ShowdownBot.modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShowdownBot.Global;
namespace ShowdownBot
{


    public enum Status
    {
        STATE_PAR,
        STATE_TOX,
        STATE_BRN,
        STATE_SLP,
        STATE_FRZ,
        STATE_HEALTHY,
        STATE_FAINT
    };

    /// <summary>
    /// A localised instance of a Pokemon
    /// </summary>
    public class BattlePokemon
    {
        public Pokemon mon;
        public Type type1;
        public Type type2;
        public int maxHealth;
        public int currentHealth;
        public Status status;
        public Boosts currentBoosts;
        public string item;
        public int level;
        public BattlePokemon(Pokemon p)
        {
            this.mon = p;
            maxHealth = mon.getRealStat("hp");
            currentHealth = maxHealth;
            status = Status.STATE_HEALTHY;
            currentBoosts = new Boosts();
            type1 = mon.type1;
            type2 = mon.type2;
            item = mon.item;
            level = 100; //TODO: actually find this, for now just assume its max.
            initBoosts();

           
        }

        private void initBoosts()
        {
            currentBoosts.atk = 0;
            currentBoosts.def = 0;
            currentBoosts.spa = 0;
            currentBoosts.spd = 0;
            currentBoosts.spe = 0;
            currentBoosts.evasion = 0;
            currentBoosts.accuracy = 0;
        }


        public void updateBoosts(string boostText)
        {
            string[] split = boostText.Split('×');
            string value = split[0];
            string whichBoost = split[1].Trim(' ').ToLower();
            setBoost(whichBoost, convertBoostToCount(float.Parse(value)));
        }

        /// <summary>
        /// Gets the modifier to be multiplied to each stat.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public float getBoostModifier(string stat)
        {
            if (stat == "atk")
                return convertBoost(currentBoosts.atk);
            else if (stat == "def")
                return convertBoost(currentBoosts.def);
            else if (stat == "spa")
                return convertBoost(currentBoosts.spa);
            else if (stat == "spd")
                return convertBoost(currentBoosts.spd);
            else if (stat == "spe")
                return convertBoost(currentBoosts.spe);
            else return 1.0f;
        }

        private void setBoost(string stat, int stage)
        {
            if (stat == "atk")
                currentBoosts.atk = stage;
            else if (stat == "def")
                currentBoosts.def = stage;
            else if (stat == "spa")
                currentBoosts.spa = stage;
            else if (stat == "spd")
                currentBoosts.spd = stage;
            else if (stat == "spe")
                currentBoosts.spe = stage;
        }

        /// <summary>
        /// Converts boost from a percentage to the 
        /// integer stage it is (-6 to 6)
        /// </summary>
        /// <param name="boost"></param>
        /// <returns></returns>
        private int convertBoostToCount(float boost)
        {
            float mod = 1.0f;
            if(boost >= 1)
            {
                mod = (boost * 2) - 2;
            }
            else
            {
                //lazy way
                float temp = 0;
                for (int i = 1; i <= 6; i++)
                {
                    temp = convertBoost(-i);
                    if (temp == boost)
                        mod = (i*-1);
                }
            }
            return (int) mod;
        }

        /// <summary>
        /// Converts a number of boosts to the percentages they represent.
        /// Doesn't work for accuracy/evasion
        /// </summary>
        /// <param name="boost"></param>
        /// <returns></returns>
        private float convertBoost(int boost)
        {
            float mod = 1.0f;
            if (boost > 0)
            {
                mod = (boost+2) / 2;
            }
           else if (boost < 0)
            {
                int denom = 3;
                for (int i = 0; i < Math.Abs(boost); i++)
                {
                    denom++;
                }
                mod = 2 / denom;
            }

            return mod;
        }
        public float getStat(string stat)
        {
            float statusmod = 1f;
            float itemmod = 1f;
            float real = mon.getRealStat(stat);
            float boosts = 1f;

            if (status == Status.STATE_PAR && stat == "spe")
                statusmod = 0.25f;
            else if (status == Status.STATE_BRN && stat == "atk")
                statusmod = 0.5f;

            boosts = getBoostModifier(stat);

            return real * statusmod * itemmod * boosts;
        }

        /// <summary>
        /// Returns damage multiplier (0x,.5x,1x,etc) of atk on def.
        /// </summary>
        /// <param name="t1">attacking move</param>
        /// <param name="t2">defending type</param>
        /// <returns>The damage multiplier</returns>
        public float damageCalc(Type atk, Type def)
        {
            Type t1 = types[atk.value];
            Type t2 = types[def.value];
            if (t1.nl != null)
            {
                if (t2.value == t1.nl[0].value)  //No single type has more than one immunity
                    return 0;
            }

            if (t1.se != null)
            {
                for (int i = 0; i < t1.se.Length; i++)
                {
                    if (t2.value == t1.se[i].value) //supereffective
                        return 2;
                }
            }
            if (t1.res != null)
            {
                for (int i = 0; i < t1.res.Length; i++)
                {
                    if (t2.value == t1.res[i].value)
                        return 0.5f;
                }
            }
            return 1;
        }

        public int getHealth() { return currentHealth; }
        public void setHealth(int percent)
        {
            float realpct = (float)(percent / 100f);
            int health = (int)(maxHealth * realpct);
            currentHealth = health;
        }

        /// <summary>
        /// Ranks a given move.
        /// Returns a rank >= 1
        /// </summary>
        /// <param name="m"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public int rankMove(Move m, BattlePokemon enemy,List<BattlePokemon> enemyTeam, LastBattleAction lba)
        {
            int DEFAULT_RANK = 11; //(15 - 4)
            int rank = DEFAULT_RANK; // rank of move m
            if (m.group != "status")
            {
                rank = hitsToKill(m, enemy);
                //discourage the use of low accuracy moves if they're overkill
                if (m.accuracy != 1 && enemy.getHPPercentage() < 20)
                    ++rank;
                if (m.priority > 0)
                    rank -= m.priority;
                //To rank in ascending order (ie 1 is a poor rank) subtract the rank from the max.
                if (rank > GlobalConstants.MAX_MOVE_RANK) rank = GlobalConstants.MAX_MOVE_RANK;
                rank = GlobalConstants.MAX_MOVE_RANK - rank;
               
            }
            else
            {
               if (m.heal)
                {
                    rank = getRecoverChance(enemy, lba);
                    rank += ((100-getHPPercentage())/10);
                }
               else if (m.status)
                {
                    rank += getStatusChance(this, enemy, m, enemyTeam);
                    /* add the status rank to the default rank, meaning a good
                     * status move will rank around the same as a 2HKO. This
                     * will prevent cases where an easy OHKO is available, but
                     * a turn is wasted on status. However it may need more
                     * balancing.
                     */
                }
               else if (m.isBoost)
                {
                    rank += getBoostChance(this, enemy, m, lba);
                }
            }
            return rank;
        }

        /// <summary>
        /// Returns how many times this pokemon will
        /// have to use move m to KO enemy.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public int hitsToKill(Move m, BattlePokemon enemy)
        {
            int totalDamage = damageFormula(m, enemy);
            //Compare the damage we will deal to the health of the enemy.
            int times = 0; //number of times it takes to use this move to KO the opponent.
            int health = enemy.getHealth();
            for (;(health > 0); times++)
            {
                if (times > GlobalConstants.MAX_HKO)
                    break;
                health -= totalDamage;
            }
            return times;
        }

        /// <summary>
        /// The damage formula used by the games / PS!
        /// This returns a fairly accurate representation of
        /// how much damage this pokemon will do to enemy with move m.
        /// Ignores critical chance and variance.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        private int damageFormula(Move m, BattlePokemon enemy)
        {
            float first = (2f * this.level + 10f) / 250f;
            float second = 0;
            if (m.group == "physical")
            {
                second = (float)this.getStat("atk") / (float)enemy.getStat("def");
            }
            else
            {
                second = (float)this.getStat("spa") / (float)enemy.getStat("spd");
            }
            float third = getRealBP(m, enemy);
            float totaldmg = first * second * third + 2;

            //Calculate modifier.
            //We will assume no critical hit and disregard the random variance.
            float itemmod = 1;
            float stab = 1;
            float immunity = 1;

            float type = damageCalc(m.type, enemy.type1);
            if(enemy.type1 != enemy.type2)
                type = type * damageCalc(m.type, enemy.type2);

            if (m.type == this.type1 || m.type == this.type2)
            {
                stab = 1.5f;
            }
            itemmod = itemDamageMod(m, enemy);
            if (immunityCheck(m.type, enemy))
                immunity = 0;
            float multiplier = type * stab * itemmod * immunity;
            return (int)Math.Floor(totaldmg * multiplier);
        }

        /// <summary>
        /// Returns a rank from -4 to 7
        /// </summary>
        /// <param name="e"></param>
        /// <param name="lastAction"></param>
        /// <returns></returns>
        private int getRecoverChance(BattlePokemon e, LastBattleAction lastAction)
        {
            BattlePokemon you = this;
            int hpThreshold = 40; //Percent of health at which to conisder recovering.
            int chance = 0;

            if (you.getHPPercentage() <= hpThreshold) chance += 3;
            else if (you.getHPPercentage() > (100 - hpThreshold)) chance -= 2;
            if (e.checkKOChance(you) < 0.3f) chance += 2; //heal if the opponent cannot ohko us.
            if (you.status != Status.STATE_HEALTHY && lastAction != LastBattleAction.ACTION_SLEEPTALK) chance += 2;
            if (lastAction == LastBattleAction.ACTION_RECOVER) chance -= 2;

            return chance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="you"></param>
        /// <param name="e"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private int getStatusChance(BattlePokemon you, BattlePokemon e, Move m, List<BattlePokemon> enemyTeam)
        {
            int max = GlobalConstants.MAX_MOVE_RANK;
            if (e.status != Status.STATE_HEALTHY)
                return -max;
            if (e.mon.hasAbility("magic bounce"))
                return -max;
            if (e.mon.hasAbility("poison heal") && m.statuseffect == "tox")
                return -max;
            if (e.mon.hasAbility("limber") && m.statuseffect == "par")
                return -max;
            if (m.statuseffect == "tox" && (e.hasType(types["poison"]) || e.hasType(types["steel"])))
                return -max;
            if (m.statuseffect == "par" && (e.hasType(types["ground"]) || e.hasType(types["electric"])))
                return -max;

            int chance = 0;
            if (m.statuseffect == "brn" && e.mon.getRole().physical)
                chance += 2;
            else if (m.statuseffect == "par" && (e.getStat("spe") >= you.getStat("spe")))
                chance += 2;
            else if (m.statuseffect == "slp" && (you.getStat("spe") >= e.getStat("spe")))
            {
                foreach (BattlePokemon bp in enemyTeam)
                {
                    if (bp.status == Status.STATE_SLP)
                        return -max; //abide by sleep clause
                }
                chance += 2;
            }

            chance += (int)Math.Round(10 * e.checkKOChance(you)); //Increase chance if enemy is too strong and needs to be weakened.

            return chance;
        }


        private int getBoostChance(BattlePokemon you, BattlePokemon e, Move m, LastBattleAction lastAction)
        {
            int chance = 0;
            int minHP = 30;
            float enemyTolerance = 0.5f;

            List<String> boosts = m.whatBoosts();
            //lower rank if already maxed out.
            foreach (string s in boosts)
            {
                if (this.getBoostModifier(s) == 4f)
                    chance -= 1;
            }
            if (you.getHPPercentage() <= minHP) chance -= 2; //too weak, should focus efforts elsewhere

            if (e.checkKOChance(you) < enemyTolerance) chance += 2; //enemy does not threaten us
            else if (e.checkKOChance(you) - 0.2f < enemyTolerance) chance += 2; //if boosting will make us survive, do it.
            else chance -= 2; //otherwise too risky

            if (you.mon.getRole().setup) chance += 4; //if the mon is a setup sweeper, etc, 
            if (lastAction == LastBattleAction.ACTION_BOOST) chance -= 1; //Be careful not to boost forever.
            return chance;
        }










        /// <summary>
        /// Calculates the total damage a move will do to a particular pokemon,
        /// with respect to abilities, types, STAB, common items, etc.
        /// </summary>
        /// <param name="m">Move used by (this) pokemon.</param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public float damageCalcTotal(Move m, BattlePokemon enemy)
        {
            float mult = damageCalc(m.type, enemy.type1);
            mult = mult * damageCalc(m.type, enemy.type2);
            float dmg = getRealBP(m,enemy);
            float stab = 1;
            float ability = 1;
            float immunity = 1;
            if (immunityCheck(m.type, enemy))
                immunity = 0;
            if (m.type == this.type1 || m.type == this.type2)
            {
                stab = 1.5f;
            }
            //do ability calculations

            float additional = itemDamageMod(m, enemy);
            

            return (dmg * mult * stab * ability * additional * immunity);
        }

        /// <summary>
        /// Returns the damage modifier for the held item.
        /// </summary>
        /// <returns></returns>
        private float itemDamageMod(Move m, BattlePokemon enemy)
        {
            if (item == "none")
                return 1;
            if (item == "choiceband" && m.group == "physical")
                return 1.5f;
            if (item == "choicespecs" && m.group == "special")
                return 1.5f;
            if (item == "lifeorb")
                return 1.3f;
            if (item == "expertbelt" && this.isSuperEffective(m, enemy))
                return 1.2f;

            return 1;
        }
        /// <summary>
        /// Used to handle moves whose BP is unknown or varies.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private float getRealBP(Move m, BattlePokemon enemy)
        {
            if (m.bp > -1)
                return m.bp;
               
            else if (m.bp == -2)
            {
                if (m.name == "Gyro Ball")
                {
                    float np = 25 * (enemy.getStat("spe") / this.getStat("spe"));
                }
            }

            return 20; //assume default
        }

        /// <summary>
        /// Gets the heuristic value of pokemon p vs. this pokemon.
        /// Returns a value between 0-8
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public float matchup(BattlePokemon p)
        {
            //All of this sucks and it needs to be rewritten to be more clear and useful.
            if (Object.ReferenceEquals(p, null))
            {
                //in a test case where volt-switch killed an opponent "p" was null
                //todo: return a more helpful indicator
                return 1;
            }
            return heuristic(p.type1, p,false) + heuristic(p.type2, p,false);
        }

        public float heuristic(Move m, BattlePokemon p)
        {
            return heuristic(m.type, p, true);
        }

        /// <summary>
        /// Gives a specialised value of the effectiveness
        /// of a move t from pokemon p towards the defender.
        /// </summary>
        /// <param name="t">Attacking move type</param>
        /// <param name="p">Pokemon attacking</param>
        /// <returns></returns>
        public float heuristic(Type t, BattlePokemon p, bool countStab)
        {
            float eff1 = damageCalc(t, this.type1);
            float eff2 = damageCalc(t, this.type2);
            float stab = 1;
            if (countStab)
            {
                if (t == p.type1 || t == p.type2)
                    stab = 1.5f;
            }
            float immunityFromAbility = 1;
            if (immunityCheck(t, this))
                immunityFromAbility = 0;

            return (eff1 * eff2 * stab * immunityFromAbility);
        }
        /// <summary>
        /// Predicts how well the pokemon matches up against
        /// the opponent. This only takes into account the pokemon's
        /// typing as compared to it's opponent's.
        /// </summary>
        /// <param name="enemy">the opposing pokemon</param>
        /// <returns>a float in range 0-8</returns>
        public float checkTypes(BattlePokemon defender)
        {
            float val = damageCalc(this.mon.type1, defender.type1);
            if (defender.type1.value != defender.type2.value ||
                defender.type2.value != null)
                val = val * damageCalc(this.mon.type1, defender.type2);
            if (this.mon.type1 != this.mon.type2 || this.mon.type2 != null)
            {
                val = val * damageCalc(this.mon.type2, defender.type2);
                if (defender.type1.value != defender.type2.value ||
                defender.type2.value != null)
                    val = val * damageCalc(this.mon.type2, defender.type2);
            }
            return val;

        }

        /// <summary>
        /// Checks if a move is immune against another Pokemon based on its abilities, etc.
        /// NOTE: Type-based immunities are covered within their own typings, not here.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool immunityCheck(Type t, BattlePokemon p)
        {
            if (t.value == "ground" && p.mon.hasAbility("levitate"))
                return true;
            if (t.value == "fire" && p.mon.hasAbility("flashfire"))
                return true;
            return false;
        }

        /// <summary>
        /// Simply returns whether the move is super-effective or better (4x)
        /// </summary>
        /// <param name="m"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public bool isSuperEffective(Move m, BattlePokemon enemy)
        {
            float a = damageCalc(m.type, enemy.type1);
            float b = 1;
            if (enemy.type1 != enemy.type2)
                b = damageCalc(m.type, enemy.type2);
            return (a * b > 2f) ? true : false;
        }
        public int getHPPercentage()
        {
            float f = ((float)currentHealth / (float)maxHealth);
            return (int)(f * 100f);
        }

        /// <summary>
        /// Returns a number between 0,1 that determines
        /// the likelihood of this mon KOing the enemy
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public float checkKOChance(BattlePokemon enemy)
        {
            float chance = 0;
            //First check if we are faster.
            
            if (this.getStat("spe") > enemy.getStat("spe"))
            {
                chance += 0.1f;
            }
            //Now check if we are a suitable attacker
            Role role = mon.getRole();
            DefenseType edeftype = enemy.mon.getDefType();
            if (((role.physical) && (edeftype.special)) ||
                 ((role.special) && (edeftype.physical)) ||
                 (edeftype.any))
            {
                chance += 0.4f;
            }

            if (enemy.getHPPercentage() < 50)
                chance += 0.2f;
            else
                chance -= 0.1f;

            if (checkTypes(enemy) >= 2.5f)
                chance += 0.2f;

            //Todo: add other parameters here. Decrement chance for unsuitable matchings, etc.
            //Todo also: adjust chance scale. do some calcs to find out what produces more favorable results.
            return chance;

        }
        
        /// <summary>
        /// Changes the internal pokemon.
        /// Used primarily to mega evolve.
        /// </summary>
        public void changeMon(string name)
        {
            Pokemon newmega = Global.lookup(name);
                mon = newmega;
                type1 = mon.type1;
                type2 = mon.type2;
        }

        public bool hasType(Type t)
        {
            if (this.type1 == t) return true;
            if (this.type2 == t) return true;
            return false;
        }
    }
}
