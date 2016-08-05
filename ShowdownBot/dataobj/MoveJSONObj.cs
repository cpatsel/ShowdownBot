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
        public int pp { get; set; }
        public int priority { get; set; }
        public Flags flags { get; set; }
        public List<int> drain { get; set; }
        public Secondary secondary { get; set; }
        public string target { get; set; }
        public string type { get; set; }
        public string contestType { get; set; }
        public bool isViable { get; set; }


        public class Self
        {
            int chance { get; set; }
            public Boosts boosts { get; set; }
        }

    }
    public class Flags
    {
        public int protect { get; set; }
        public int mirror { get; set; }
        public int heal { get; set; }
        public int bullet { get; set; }
        public int contact { get; set; }
        public int punch { get; set; }
        public int sound { get; set; }
        public int bite { get; set; }
        public int charge { get; set; }
        public int defrost { get; set; }
        public int gravity { get; set; }
        public int powder { get; set; }
        public int pulse { get; set; }
        public int recharge { get; set; }
        public int reflectable { get; set; }
        public int snatch { get; set; }
    }
    public class Boosts
    {
        public int atk { get; set; }
        public int spa { get; set; }
        public int def { get; set; }
        public int spd { get; set; }
        public int spe { get; set; }
        public int accuracy { get; set; }
        public int evasion { get; set; }
    }


    public class Secondary
    {
        public int chance { get; set; }
        public Boosts boosts { get; set; }
    }
}
