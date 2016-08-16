using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowdownBot.dataobj;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    

    //TODO: add more specific roles, like fast physical or tank,etc and give them specific spreads in setRealStats
    public class Role
    {
        //Mutually Exclusive:
        public bool lead { get; set; }
        public bool physical { get; set; }
        public bool special { get; set; }
        public bool mixed { get; set; }
        public bool stall { get; set; }
        public bool any { get; set; }
        public bool tank { get; set; }
        //Can be set in addition to above:
        public bool setup { get; set; } //Whether this mon ne
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
        float weight = 50f;
        BaseStats stats;
        Dictionary<string, int> realStats;
        Abilities abilities;
        DefenseType deftype;
        Role role;
        public StatSpread statSpread;
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
                type2 = types[obj.types[1].ToLower()];
            stats = obj.baseStats;
            abilities = obj.abilities;
            weight = (float)obj.weightkg;
            deftype = new DefenseType();
            role = new Role();
            realStats = new Dictionary<string, int>();
            initRoles();
            modifyRole();
            setRealStats();
        }

        
        private void initRoles()
        {
            int max_for_bulk = 250;
            if (Math.Abs(stats.spd - stats.def) < 10)
                deftype.mixed = true;
            else if(stats.def > stats.spd)
                deftype.physical = true;
            else if (stats.spd > stats.def)
                deftype.special = true;
            else
                deftype.any = true;

            if (stats.hp + stats.def + stats.spd >= max_for_bulk)
                deftype.bulky = true;

            if (Math.Abs(stats.spa - stats.atk) < 10)
                role.mixed = true;
            else if (stats.atk > stats.spa)
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


        public void modifyRole()
        {
            string path = Global.ROLEPATH;
            using (var reader = new StreamReader(path))
            {
                string json;
                json = reader.ReadToEnd();
                JObject jo = JsonConvert.DeserializeObject<JObject>(json);
                string allroles = jo.First.ToString();
                var current = jo.First;
                for (int i = 0; i < jo.Count; i++)
                {
                    RoleOverride ro = JsonConvert.DeserializeObject<RoleOverride>(current.First.ToString());
                    if (ro.name == this.name)
                    {
                        if (!Object.ReferenceEquals(ro.role, null))
                            this.role = ro.role;
                        if (!Object.ReferenceEquals(ro.deftype, null))
                            this.deftype = ro.deftype;
                    }
                    current = current.Next;

                }
            }
        }

        public int getRealStat(string stat)
        {
            if (realStats.ContainsKey(stat))
            {
                return realStats[stat];
            }
            else
            {
                return -1;
            }
        }

        public string getDefTypeToString()
        {
            string toreturn = "";
            if (deftype.physical)
                toreturn = "physical";
            if (deftype.special)
                toreturn = "special";
            if (deftype.mixed)
                toreturn = "mixed";
            if (deftype.any)
                toreturn = "any";
            if (deftype.bulky)
                toreturn = "bulky " + toreturn;
            return toreturn;
        }
        public Role getRole()
        {
            return role;
        }
        public DefenseType getDefType()
        {
            return deftype;
        }
        public string getRoleToString()
        {
            string toreturn = "";
            if (role.lead)
                toreturn = "lead";
            if (role.physical)
                toreturn = "physical";
            if (role.special)
                toreturn = "special";
            if (role.mixed)
                toreturn = "mixed";
            if (role.any)
                toreturn = "any";
            if (role.tank)
                toreturn = "tank";
            if (role.stall)
                toreturn = toreturn + " stall";
            
            return toreturn;
        }


        

        


        
        


        public bool hasAbility(string ability)
        {
            if (abilities.a1 == ability) return true;
            else if (abilities.a2 == ability) return true;
            else if (abilities.H == ability) return true;
            else return false;
        }

     
        private void setStatsFromSpread(StatSpread spread)
        {
            int atkval, defval, spaval, spdval, speval;
            int hpval;
            atkval = statCalc(this.stats.atk, spread.atkIV,spread.atkEV, spread.atkNatureMod); 
            defval = statCalc(this.stats.spa, spread.defIV, spread.defEV, spread.defNatureMod); 
            spaval = statCalc(this.stats.def, spread.spaIV, spread.spaEV, spread.spaNatureMod);
            spdval = statCalc(this.stats.spd, spread.spdIV, spread.spdEV, spread.spdNatureMod);
            speval = statCalc(this.stats.spe, spread.speIV, spread.speEV, spread.speNatureMod);
            hpval = hpCalc(this.stats.hp, spread.hpIV, spread.hpEV, 100);

            realStats.Add("atk", atkval);
            realStats.Add("def", defval);
            realStats.Add("spa", spaval);
            realStats.Add("spd", spdval);
            realStats.Add("spe", speval);
            realStats.Add("hp", hpval);

            statSpread = spread;

        }
        private void setRealStats()
        {
            if (this.role.physical)
            {
                setStatsFromSpread(new StatSpreadPhysical());
            }
            else if (this.role.special)
            {
                setStatsFromSpread(new StatSpreadSpecial());
            }
            else if (this.role.tank)
            {
                if (deftype.physical)
                {
                    setStatsFromSpread(new StatSpread_PhysicallyDefensive());
                }
                else if (deftype.special)
                {
                    setStatsFromSpread(new StatSpread_SpeciallyDefensive());
                }
                else
                {
                    setStatsFromSpread(new StatSpread());  
                }
            }
            else
            {
                setStatsFromSpread(new StatSpread());
            }
            
        }
        private int statCalc(int base_stat, int ivVal, int evVal, float nature, int level = 100)
        {
            float value = ((  (((2 * base_stat) + ivVal + (evVal / 4)) * level) / 100) + 5) * nature;
            return (int)value;
        }
        private int hpCalc(int base_stat, int ivVal, int evVal, int level)
        {
            if (this.name == "shedinja")
                return 1;
            else
            {
                float value = ((((2 * base_stat + ivVal + (evVal / 4)) * level) / 100) + level + 10);
                return (int)value;
            }
        }


        //end of class
    }
}
