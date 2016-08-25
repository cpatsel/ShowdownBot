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
        public BattlePokemon(Pokemon p)
        {
            this.mon = p;
            maxHealth = mon.getRealStat("hp");
            currentHealth = maxHealth;
            status = Status.STATE_HEALTHY;
            currentBoosts = new Boosts();
            type1 = mon.type1;
            type2 = mon.type2;
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


        /// <summary>
        /// Doesn't work for accuracy/evasion
        /// </summary>
        /// <param name="boost"></param>
        /// <returns></returns>
        private float convertBoost(int boost)
        {
            float mod = 1.0f;
            if (boost > 0)
            {
                mod = boost / 2;
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

            if (status == Status.STATE_PAR && stat == "spe")
                statusmod = 0.25f;
            else if (status == Status.STATE_BRN && stat == "atk")
                statusmod = 0.5f;

            return real * statusmod * itemmod;
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

            float additional = 1;
            if (this.item == "choiceband" || this.item == "choicespecs")
                additional = 1.5f;
            else if (this.item == "lifeorb")
                additional = 1.3f;

            return (dmg * mult * stab * ability * additional * immunity);
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

        public bool immunityCheck(Type t, BattlePokemon p)
        {
            if (t.value == "ground" && p.mon.hasAbility("levitate"))
                return true;
            if (t.value == "fire" && p.mon.hasAbility("flashfire"))
                return true;
            return false;
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
    }
}
