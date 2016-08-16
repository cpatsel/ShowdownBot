using ShowdownBot.dataobj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowdownBot
{
    /// <summary>
    /// A class used to store JSON information about a pokemon before passing it to the internal 
    /// Pokemon representation found in Pokemon.cs
    /// </summary>
    public class PokeJSONObj
    {

        public int num { get; set; }
        public string species { get; set; }
        public List<string> types { get; set; }
        public GenderRatio genderRatio { get; set; }
        public BaseStats baseStats { get; set; }
        public Abilities abilities { get; set; }
        public double heightm { get; set; }
        public double weightkg { get; set; }
        public string color { get; set; }
        public List<string> evos { get; set; }
        public List<string> eggGroups { get; set; }



        public class GenderRatio
        {
            public double M { get; set; }
            public double F { get; set; }
        }


    }


    //TODO: statspread will alter the bot's perception of opponent pokemon as well, maybe keep a backup of default stats.
    public class RoleOverride
    {
        public string name { get; set; }
        public Role role { get; set; }
        public DefenseType deftype { get; set; }
        public StatSpread statspread { get; set; }
    }

    public class Abilities
    {
        public string a1 { get; set; }
        public string a2 { get; set; }
        public string H { get; set; }
        
    }

    public class BaseStats
    {
        public int hp { get; set; }
        public int atk { get; set; }
        public int def { get; set; }
        public int spa { get; set; }
        public int spd { get; set; }
        public int spe { get; set; }
    }

}
