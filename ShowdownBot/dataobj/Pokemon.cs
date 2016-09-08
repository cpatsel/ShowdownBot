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
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
   

    public class Pokemon
    {      
        #region Values

        public string name = "NONAME";
        public Type type1, type2;
        public string item = "none";
        //float apct = 100f;
        //string firstmove = "NA";
        float weight = 50f;
        BaseStats stats;
        Dictionary<string, int> realStats;
        Abilities abilities;
        DefenseType deftype;
        Role role;
        public StatSpread statSpread;
        public StatSpread alternativeStatSpread = null;
        Dictionary<string,Type> types;
       
        #endregion

        public Pokemon(PokeJSONObj obj)
        {
            basicInit(obj);           
            initRoles();
            RoleOverride ro = getRoleOverride();
            /*If the pokemon has a "personal" override, create a new pokemon data object to hold the information
            /* regarding our personal pokemon to keep it seperate from the generic "expected" pokemon 
             * Otherwise, modify the role as roleoverride is intended.
            */
            if (!Object.ReferenceEquals(ro, null))
            {
                if (!Object.ReferenceEquals(ro.personal, null))
                {
                    if (ro.personal)
                    {
                        Pokemon personal = new Pokemon(obj, ro);
                        if (!Global.pokedex.ContainsKey(personal.name))
                            Global.pokedex.Add(personal.name, personal); //Only allow 1 personal version of each pkmn.
                    }
                    else
                        modifyRole();
                }
            }
            
            setRealStats();
        }

        public Pokemon (PokeJSONObj baseMon, RoleOverride newStats)
        {
            basicInit(baseMon);
            initRoles();
            modifyRole();
            setRealStats();
            name = GlobalConstants.PERSONAL_PRE + name;
        }
        

        private void basicInit(PokeJSONObj obj)
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

        private RoleOverride getRoleOverride()
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
                        return ro;
                    }
                    current = current.Next;

                }
            }
            return null;
        }

        public void modifyRole()
        {
            RoleOverride ro = getRoleOverride();
            if (ro != null) { 
                        if (!Object.ReferenceEquals(ro.role, null))
                            this.role = ro.role;
                        if (!Object.ReferenceEquals(ro.deftype, null))
                            this.deftype = ro.deftype;
                        if (!Object.ReferenceEquals(ro.statspread, null))
                            this.statSpread = ro.statspread;
                        if (!Object.ReferenceEquals(ro.item, null))
                            this.item = ro.item;
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
            if (abilities.a1.ToLower() == ability.ToLower()) return true;
            if (!Object.ReferenceEquals(abilities.a2,null))
                if(abilities.a2.ToLower() == ability.ToLower()) return true;
            if (!Object.ReferenceEquals(abilities.H, null))
                if (abilities.H.ToLower() == ability.ToLower()) return true;
            return false;
        }

     
        private void setStatsFromSpread(StatSpread s)
        {
            StatSpread spread;
            if (hasAltStatSpread())
                spread = alternativeStatSpread;
            else
                spread = s;

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

        /// <summary>
        /// Whether there is an alternative stat spread, indicating that this pokemon has been modified.
        /// </summary>
        /// <returns></returns>
        public bool hasAltStatSpread()
        {
            return !Object.ReferenceEquals(this.alternativeStatSpread, null);
        }
        //end of class
    }
}
