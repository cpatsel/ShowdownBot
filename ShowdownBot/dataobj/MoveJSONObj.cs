using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowdownBot
{
    /// <summary>
    /// Move version of PokemonJSONObj
    /// </summary>
    public class MoveJSONObj
    {
        public int num { get; set; }
        public int accuracy { get; set; }
        public int basePower { get; set; }
        public string category { get; set; }
        public string desc { get; set; }
        public string shortDesc { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string status { get; set; } = "none";
        public int pp { get; set; }
        public int priority { get; set; }
        public Flags flags { get; set; }
        public Boosts boosts { get; set; }
        public List<int> drain { get; set; }
        public Secondary secondary { get; set; }
        public string target { get; set; }
        public string type { get; set; }
        public string contestType { get; set; }
        public bool isViable { get; set; }
        public string sideCondition { get; set; } = "none";
        public string isZ { get; set; } = "false";
        public Boosts zMoveBoost { get; set; };
        public class Self
        {
            int chance { get; set; }
            public Boosts boosts { get; set; }
        }

    }
    public class Flags
    {
        public int protect { get; set; } = 0;
        public int mirror { get; set; } = 0;
        public int heal { get; set; } = 0;
        public int bullet { get; set; } = 0;
        public int contact { get; set; } = 0;
        public int punch { get; set; } = 0;
        public int sound { get; set; } = 0;
        public int bite { get; set; } = 0;
        public int charge { get; set; } = 0;
        public int defrost { get; set; } = 0;
        public int gravity { get; set; } = 0;
        public int powder { get; set; } = 0;
        public int pulse { get; set; } = 0;
        public int recharge { get; set; } = 0;
        public int reflectable { get; set; } = 0;
        public int snatch { get; set; } = 0;
    }
    public class Boosts
    {
        public int atk { get; set; } = 0;
        public int spa { get; set; } = 0;
        public int def { get; set; } = 0;
        public int spd { get; set; } = 0;
        public int spe { get; set; } = 0;
        public int accuracy { get; set; } = 0;
        public int evasion { get; set; } = 0;
        public int total()
        {
            return atk + spa + def + spd + spe + accuracy + evasion;
        }
        public bool hasBoosts()
        {
            return (atk > 0 || spa > 0 || def > 0 || spd > 0 || spe > 0 || accuracy > 0 || evasion > 0);
        }
        public bool hasDrops()
        {
            return (atk < 0 || spa < 0 || def < 0 || spd < 0 || spe < 0 || accuracy < 0 || evasion < 0);
        }
        public bool noStatChanges()
        {
            return (atk == 0 && spa == 0 && def == 0 && spd == 0 && spe == 0 && accuracy == 0 && evasion == 0);
        }
    }
    public class Secondary
    {
        public int chance { get; set; }
        public Boosts boosts { get; set; }
        public string status { get; set; }
    }
}
