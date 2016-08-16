using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowdownBot.dataobj
{
    public class StatSpread
    {
        public int atkEV { get; set; } = 0;
        public int atkIV { get; set; } = 31;
        public float atkNatureMod { get; set; } = 1;

        public int defEV { get; set; } = 0;
        public int defIV { get; set; } = 31;
        public float defNatureMod { get; set; } = 1;

        public int spaEV { get; set; } = 0;
        public int spaIV { get; set; } = 31;
        public float spaNatureMod { get; set; } = 1;

        public int spdEV { get; set; } = 0;
        public int spdIV { get; set; } = 31;
        public float spdNatureMod { get; set; } = 1;

        public int speEV { get; set; } = 0;
        public int speIV { get; set; } = 31;
        public float speNatureMod { get; set; } = 1;

        public int hpEV { get; set; } = 0;
        public int hpIV { get; set; } = 31;
    }

    public class StatSpreadPhysical : StatSpread
    {
        public StatSpreadPhysical()
        {
            atkEV = 252;
            speEV = 252;
            spaNatureMod = 0.9f;
            atkNatureMod = 1.1f;
        }
    }
    public class StatSpreadSpecial : StatSpread
    {
        public StatSpreadSpecial()
        {
            spaEV = 252;
            speEV = 252;
            spaNatureMod = 1.1f;
            atkNatureMod = 0.9f;
        }
    }

    public class StatSpread_PhysicallyDefensive : StatSpread
    {
        public StatSpread_PhysicallyDefensive()
        {
            hpEV = 252;
            defEV = 252;
            spdEV = 4;
            atkNatureMod = 0.9f;
            defNatureMod = 1.1f;
            atkIV = 0;
        }
    }

    public class StatSpread_SpeciallyDefensive : StatSpread
    {
        public StatSpread_SpeciallyDefensive()
        {
            hpEV = 252;
            spdEV = 252;
            defEV = 4;
            atkNatureMod = 0.9f;
            spdNatureMod = 1.1f;
            atkIV = 0;
        }
    }
}
