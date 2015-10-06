using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowdownBot
{
    public enum Types
    {
        FIRE,
        WATER,
        GRASS,
        ICE,
        DARK,
        STEEL,
        FAIRY,
        POISON,
        PYSCHIC,
        BUG,
        FLYING,
        GROUND,
        ELECTRIC,
        DRAGON,
        NORMAL,
        GHOST,
        ROCK,
        FIGHTING
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
        string type1 = "Normal", type2 = "Normal", ability1 = "NA", ability2 = "NA";
        string deftype = "NA";
        string item = "NA";
        int speed = 0;
        float apct = 1.0f;
        string firstmove = "NA";

        public Pokemon(string d)
        {
            data = d;
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
            if (value == "todo" || value == "na")
                return;
            //set the appropriate field
            switch (key)
            {
                case "name": name = value; break;
                case "type1": type1 = value; break;
                case "type2": type2 = value; break;
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
    }
}
