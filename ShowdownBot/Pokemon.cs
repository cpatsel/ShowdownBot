using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowdownBot
{
   
    struct Type
    {
        public string value;
        public Type[] se;    //2x against
        public Type[] res;   //0.5x against
        public Type[] nl;    //0x against
        public Type(string v) { value = v; se = null; res = null; nl = null; }
    }


    /// <summary>
    /// If a move is non damaging (0 bp) it is considered
    /// to be a status move.
    /// </summary>
    struct Move
    {
       public Type type;
       public float bp;
       bool selfStatus;
       public Move(Type t, float p) { type = t; bp = p; selfStatus = false; }
       public Move(Type t, bool s) { type = t; bp = 0.0f; selfStatus = s; }

    }

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
        string ability1 = "NA", ability2 = "NA";
        string deftype = "any";
        string item = "NA";
        string role = "any";
        int speed = 0;
        float apct = 100f;
        string firstmove = "NA";
        Dictionary<string,Type> types;
        #endregion

        public Pokemon(string d)
        {
            data = d;
            setupTypes();
            //initialize the two types to a default
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
    /// <returns></returns>
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
                        return -0.5f;
                }
            }
            return 1;
        }

        /// <summary>
        /// Predicts how well the pokemon matches up against
        /// the opponent
        /// </summary>
        /// <param name="enemy">the opposing pokemon</param>
        /// <returns>a float in range 0-2</returns>
        public float checkTypes(Pokemon enemy)
        {
            if (enemy == null) return 0;
            float[] p = {1,1,1,1};
            p[0] += damageCalc(enemy.type1, type1);
            p[1] += damageCalc(enemy.type1, type2);
            p[2] += damageCalc(enemy.type2, type1);
            p[3] += damageCalc(enemy.type2, type2);
              return p.Average();
        }

        public float checkKOChance(Pokemon enemy)
        {
            float chance = 0;
            //First check if we are faster.
            if (enemy.item == "cscarf")
                enemy.speed *= 2;
            if (speed > enemy.speed)
            {
                chance += 0.4f;
            }
            //Now check if we are a suitable attacker
            if ( ((role == "ph") && (enemy.deftype == "sp")) ||
                 ((role == "sp") && (enemy.deftype == "ph")) ||
                 (enemy.deftype == "any") )
            {
                chance += 0.4f;
            }
            //Todo: add other parameters here. Decrement chance for unsuitable matchings, etc.
            //Todo also: adjust chance scale. do some calcs to find out what produces more favorable results.
            return chance;

        }

        public float attacks(Move move, Pokemon enemy)
        {
            //Simple method of determining damage.
            //TODO: factor in stab and other params.
            float dmg = damageCalc(move.type, enemy.type1);
            dmg += damageCalc(move.type, enemy.type2);
            return dmg;
        }


        private void setupTypes()
        {
            types = new Dictionary<string, Type>();

            #region Declarations
            Type fire = new Type("fire");
            Type water = new Type("water");
            Type grass = new Type("grass");
            Type ice = new Type("ice");
            Type dark = new Type("dark");
            Type steel = new Type("steel");
            Type fairy = new Type("fairy");
            Type poison = new Type("poison");
            Type psychic = new Type("psychic");
            Type bug = new Type("bug");
            Type flying = new Type("flying");
            Type ground = new Type("ground");
            Type electric = new Type("electric");
            Type dragon = new Type("dragon");
            Type normal = new Type("normal");
            Type ghost = new Type("ghost");
            Type rock = new Type("rock");
            Type fighting = new Type("fighting");
            #endregion

            #region Characteristics
            fire.se = new Type[] { grass, ice, steel };
            fire.res = new Type[] { rock, water, steel, dragon };
            types.Add(fire.value, fire);
            water.se = new Type[] { fire, rock, ground };
            water.res = new Type[] { dragon, water, grass };
            types.Add(water.value, water);
            grass.se = new Type[] { ground, rock, water };
            grass.res = new Type[] { bug, dragon, fire, flying, grass, poison, steel };
            types.Add(grass.value, grass);
            ice.se = new Type[] { dragon, flying, grass, ground };
            ice.res = new Type[] { fire, ice, steel, water };
            types.Add(ice.value, ice);
            dark.se = new Type[] { ghost, psychic };
            dark.res = new Type[] { dark, fairy, fighting };
            types.Add(dark.value, dark);
            steel.se = new Type[] { ice, fairy, rock };
            steel.res = new Type[] { electric, fire, steel, water };
            types.Add(steel.value, steel);
            fairy.se = new Type[] { dark, dragon, fighting };
            fairy.res = new Type[] { fire, poison, steel };
            types.Add(fairy.value, fairy);
            poison.se = new Type[] { fairy, grass };
            poison.res = new Type[] { ghost, ground, poison, rock };
            poison.nl = new Type[] { steel };
            types.Add(poison.value, poison);
            psychic.se = new Type[] { fighting, poison };
            psychic.res = new Type[] { psychic, steel };
            psychic.nl = new Type[] { dark };
            types.Add(psychic.value, psychic);
            bug.se = new Type[] { dark, grass, psychic };
            bug.res = new Type[] { fairy, fighting, fire, flying, ghost, poison, steel };
            types.Add(bug.value, bug);
            flying.se = new Type[] { bug, fighting, grass };
            flying.res = new Type[] { electric, rock, steel };
            types.Add(flying.value, flying);
            ground.se = new Type[] { electric, fire, poison, rock, steel };
            ground.res = new Type[] { bug, grass };
            ground.nl = new Type[] { flying };
            types.Add(ground.value, ground);

            electric.se = new Type[] { flying, water };
            electric.res = new Type[] { dragon, electric, grass };
            electric.nl = new Type[] { ground };
            types.Add(electric.value, electric);
            dragon.se = new Type[] { dragon };
            dragon.res = new Type[] { steel };
            dragon.nl = new Type[] { fairy };
            types.Add(dragon.value, dragon);
            normal.res = new Type[] { rock, steel };
            normal.nl = new Type[] { ghost };
            types.Add(normal.value, normal);
            ghost.se = new Type[] { ghost, psychic };
            ghost.res = new Type[] { dark };
            ghost.nl = new Type[] { normal };
            types.Add(ghost.value, ghost);
            rock.se = new Type[] { flying, ice, fire, bug };
            rock.res = new Type[] { fighting, ground, steel };
            types.Add(rock.value, rock);
            fighting.se = new Type[] { dark, ice, normal, rock, steel };
            fighting.res = new Type[] { bug, fairy, flying, poison, psychic };
            fighting.nl = new Type[] { ghost };
            types.Add(fighting.value, fighting);
            #endregion

            Global.types = this.types;

        }
    
    
    
    }
}
