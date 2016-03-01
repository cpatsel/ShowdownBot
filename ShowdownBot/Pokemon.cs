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
        public static bool operator ==(Type rh, string lh)
        {
            return (rh.value == lh.ToLower());
        }
        public static bool operator !=(Type rh, string lh)
        {
            return (rh.value != lh.ToLower());
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


  

    /// <summary>
    /// Information is read from file line by line, with the following parameters
    /// (changing them if present, defaulting if not):
    /// name, type1/2, ability 1/2, apct(chance the mon will have ability 1), speed(highest possible speed),item,
    /// deftype(Phys,Spec,Bulky-both high,mix-both low), 
    /// Some optional parameters are:
    ///     firstmove(move likely to be used first)
    ///     
    /// Parameters marked with NA are not relevant (or are less significant than others), and are effectively ignored.
    /// Formatting follows key1:value1,key2:value2
    /// </summary>
    /// Role: Ph,Sp,Mix,Lead,Status,Stall,any(unknown/versatile)
    /// Deftype: Ph,Sp,bulk,any
    class Pokemon
    {
        string data; //The string containing all data to be read for this pokemon.

        #region Values

        public string name = "NONAME";
        public Type type1, type2;
        public string ability1 = "NA", ability2 = "NA";
        string deftype = "any";
        string item = "NA";
        string role = "any";
        int speed = 0;
        float apct = 100f;
        string firstmove = "NA";
        Dictionary<string,Type> types;

        Status currentStatus = Status.STATE_HEALTHY;
        #endregion

        public Pokemon(string d)
        {
            data = d;
            //initialize the two types to a default
            types = Global.types;
            type1 = type2 = types["normal"];
            initValues();

        }

        private void initValues()
        {
            string[] fields = data.Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                setValue(fields[i]);
            }
            
        }
        private void setValue(string pair)
        {
            string[] temp = pair.Split(':');
            string key = temp[0];
            string value = temp[1];
            
            key = key.ToLower();
            value = value.ToLower();

            //if it's a dummy value just ignore it
            if ((value == "todo") || (value == "na"))
                return;
            //set the appropriate field
            switch (key)
            {
                case "name": name = value; break;
                case "type1": type1 = types[value]; break;
                case "type2": type2 = types[value]; break;
                case "ability1": ability1 = value; break;
                case "ability2": ability2 = value; break;
                case "apct": apct = Int32.Parse(value.TrimEnd('%')); break;
                case "speed": speed = Int32.Parse(value); break;
                case "item": item = value; break;
                case "deftype": deftype = value; break;
                case "firstmove": firstmove = value; break;
                   
            }
            apct = (float)apct / 100f; //convert percentage to a 0-1.0 value
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




        public bool immunityCheck(Type t, Pokemon p)
        {
            if (t == "ground" && p.ability1 == "levitate")
                return true;
            if (t == "fire" && p.ability1 == "flashfire")
                return true;
            return false;
        }


        public float matchup(Pokemon p)
        {
            return heuristic(p.type1, p) + heuristic(p.type2, p);
        }

        public float heuristic(Move m, Pokemon p)
        {
            return heuristic(m.type, p);
        }

        /// <summary>
        /// Gives a specialised value of the effectiveness
        /// of a move t from pokemon p towards the defender.
        /// </summary>
        /// <param name="t">Attacking move type</param>
        /// <param name="p">Pokemon attacking</param>
        /// <returns></returns>
        public float heuristic(Type t, Pokemon p)
        {
            float eff1 = damageCalc(t, this.type1);
            float eff2 = damageCalc(t, this.type2);
            float stab = 1;
            if (t == p.type1 || t == p.type2)
                stab = 1.5f;
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
                enemy.speed *= 2;
            if (speed > enemy.speed)
            {
                chance += 0.1f;
            }
            //Now check if we are a suitable attacker
            if ( ((role == "ph") && (enemy.deftype == "sp")) ||
                 ((role == "sp") && (enemy.deftype == "ph")) ||
                 (enemy.deftype == "any") )
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
