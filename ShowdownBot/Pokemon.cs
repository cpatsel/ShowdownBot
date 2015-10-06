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
    class Pokemon
    {
        string data; //The string containing all data to be read for this pokemon.

        public string name = "NONAME";
        Type type1, type2;
        string ability1 = "NA", ability2 = "NA";
        string deftype = "NA";
        string item = "NA";
        int speed = 0;
        float apct = 1.0f;
        string firstmove = "NA";
        Dictionary<string,Type> types;
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
        public float damageCalc(Type t1,Type t2)
        {
            return 0; 
        }
        public float checkTypes(Pokemon enemy)
        {
            //although moves are capped at being 4x effective,
            //a type matchup can have a dis/advantage of 8x.
            float p = damageCalc(enemy.type1, type1);   //get first types matchup
            if (enemy.type1.value != enemy.type2.value)
                p += damageCalc(enemy.type2,type1);     //if it's not a monotype, get first type matchup with second
            if (type1.value != type2.value)
                p += damageCalc(enemy.type1,type2);
            if ( (enemy.type1.value != enemy.type2.value) && (type1.value != type2.value) ) //if neither are monotype
                p += damageCalc(type2, enemy.type2);
            return p;
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

        }
    
    
    
    }
}
