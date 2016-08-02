using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowdownBot
{
   
    public struct Type
    {
        public string value;
        public Type[] se;    //2x against
        public Type[] res;   //0.5x against
        public Type[] nl;    //0x against
        public Type(string v) { value = v; se = null; res = null; nl = null; }
        public static bool operator ==(Type rh, Type lh)
        {
            return (rh.value == lh.value);
        }
        public static bool operator !=(Type rh, Type lh)
        {
            return (rh.value != lh.value);
        }

        
    }
    public enum Status
    {
        STATE_PAR,
        STATE_TOX,
        STATE_BRN,
        STATE_SLP,
        STATE_FRZ,
        STATE_HEALTHY
    };

    public class Role
    {
        public bool lead { get; set; }
        public bool physical { get; set; }
        public bool special { get; set; }
        public bool stall { get; set; }
        public bool any { get; set; }
    }
  
    public class DefenseType
    {
        public bool physical { get; set; }
        public bool special { get; set; }
        public bool mixed { get; set; }
        public bool bulky { get; set; }
        public bool any { get; set; }
    }

    public class Pokemon
    {
        string data; //The string containing all data to be read for this pokemon.

        #region Values

        public string name = "NONAME";
        public Type type1, type2;
        string item = "NA";
        float apct = 100f;
        string firstmove = "NA";
        BaseStats stats;
        Abilities abilities;
        DefenseType deftype;
        Role role;
        Dictionary<string,Type> types;

        
        #endregion

        public Pokemon(PokeJSONObj obj)
        {
            types = Global.types;
            name = obj.species.ToLower();
            type1 = types[obj.types[0].ToLower()];
            if (obj.types.Count < 2)
                type2 = type1;
            else
                type2 = types[obj.types[0].ToLower()];
            stats = obj.baseStats;
            abilities = obj.abilities;
            deftype = new DefenseType();
            role = new Role();
            initRoles();
        }

        
        private void initRoles()
        {
            int max_for_bulk = 250;
            if (stats.def > stats.spd)
                deftype.physical = true;
            else if (stats.spd > stats.def)
                deftype.special = true;
            else if (stats.def == stats.spa)
                deftype.mixed = true;
            else
                deftype.any = true;

            if (stats.hp + stats.def + stats.spd >= max_for_bulk)
                deftype.bulky = true;
            

            if (stats.atk > stats.spa)
                role.physical = true;
            else if (stats.spa > stats.atk)
                role.special = true;

            int max_for_stall = 50; //maximum base  special/attack to be considered 'stall'
            if (deftype.bulky)
            {
                if(role.physical)
                    role.stall = (stats.atk < max_for_stall) ? true: false;
                else if (role.special)
                    role.stall = (stats.spa < max_for_stall) ? true: false;
            }
        }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="t1">attacking move</param>
    /// <param name="t2">defending type</param>
    /// <returns>The damage multiplier</returns>
        public float damageCalc(Type type1,Type type2)
        {
            Type t1 = types[type1.value];
            Type t2 = types[type2.value];
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

        /// <summary>
        /// Calculates the total damage a move will do to a particular pokemon,
        /// with respect to abilities, types, STAB, common items, etc.
        /// </summary>
        /// <param name="m">Move used by (this) pokemon.</param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public float damageCalcTotal(Move m, Pokemon enemy)
        {
            float mult = damageCalc(m.type, enemy.type1);
            mult = mult * damageCalc(m.type, enemy.type2);
            float dmg = getRealBP(m);
            float stab = 1;
            float ability = 1;
            float immunity = 1;
            if (immunityCheck(m.type,enemy))
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
        /// Used to handle moves whose BP is unknown. -ate ability moves are handled
        /// here to prevent miscalculation, as well as other moves who change type or have
        /// varying power.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private float getRealBP(Move m)
        {
            if (m.bp > -1)
                return m.bp;
            else if (m.bp == -2)
            {
                if (m.name == "Gyro Ball")
                    return 70; //calculate speeds later
            }
            else
            {
                if (m.name == "Return" || m.name == "Frustration")
                    return 102;
                if (m.name == "Hidden Power")
                    return 60;
            }
            return 20; //assume default
        }

        /// <summary>
        /// Predicts how well the pokemon matches up against
        /// the opponent. This only takes into account the pokemon's
        /// typing as compared to it's opponent's.
        /// </summary>
        /// <param name="enemy">the opposing pokemon</param>
        /// <returns>a float in range 0-8</returns>
        public float checkTypes(Pokemon defender)
        {
            float val = damageCalc(this.type1,defender.type1);
            if (defender.type1.value != defender.type2.value ||
                defender.type2.value != null)
                val = val * damageCalc(this.type1, defender.type2);
            if (this.type1 != this.type2 || this.type2 != null)
            {
                val = val * damageCalc(this.type2, defender.type2);
                if (defender.type1.value != defender.type2.value ||
                defender.type2.value != null)
                    val = val * damageCalc(this.type2, defender.type2);
            }
            return val;
            
        }


        public bool hasAbility(string ability)
        {
            if (abilities.a1 == ability) return true;
            else if (abilities.a2 == ability) return true;
            else if (abilities.H == ability) return true;
            else return false;
        }

        public bool immunityCheck(Type t, Pokemon p)
        {
            if (t.value == "ground" && p.hasAbility("levitate"))
                return true;
            if (t.value == "fire" && p.hasAbility("flashfire")) 
                return true;
            return false;
        }


        public float matchup(Pokemon p)
        {
            if (Object.ReferenceEquals(p, null))
            {
                //in a test case where volt-switch killed an opponent "p" was null
                //todo: return a more helpful indicator
                return 1;
            }
            return heuristic(p.type1, p,false) + heuristic(p.type2, p,false);
        }

        public float heuristic(Move m, Pokemon p)
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
        public float heuristic(Type t, Pokemon p, bool countStab)
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
        /// 
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns>A number between 0,1 that determines
        /// the liklihood of this mon KOing the enemy</returns>
        public float checkKOChance(Pokemon enemy)
        {
            float chance = 0;
            //First check if we are faster.
            if (enemy.item == "cscarf")
                enemy.stats.spe *= 2;
            if (stats.spe > enemy.stats.spe)
            {
                chance += 0.1f;
            }
            //Now check if we are a suitable attacker
            if ( ((role.physical) && (enemy.deftype.special)) ||
                 ((role.special) && (enemy.deftype.physical)) ||
                 (enemy.deftype.any) )
            {
                chance += 0.4f;
            }

            //Todo: consider typing
            //Todo: add other parameters here. Decrement chance for unsuitable matchings, etc.
            //Todo also: adjust chance scale. do some calcs to find out what produces more favorable results.
            return chance;

        }

        public float attacks(Move move, Pokemon enemy)
        {
            //Simple method of determining damage.
            //TODO: factor in stab and other params.
            float dmg = damageCalc(move.type, enemy.type1);
            if (enemy.type1.value != enemy.type2.value ||
                enemy.type2.value != null)
                dmg = damageCalc(move.type, enemy.type2) * dmg;
            return dmg;
        }


      
    
    
    
    }
}
