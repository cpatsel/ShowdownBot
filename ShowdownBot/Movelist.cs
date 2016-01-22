using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowdownBot
{

    /// <summary>
    /// If a move is non damaging (0 bp) it is considered
    /// to be a status move.
    /// </summary>
    public class Move
    {
        public Type type;
        public string name;
        public float bp = 0;
        public bool unknown = false; //Given to moves which are not explicitly defined within this file.
        public bool boost = false; //Is a boosting move? ie. Swords Dance
        public bool status = false; //Is a status move? ie. Toxic
        public bool support = false; //Is a supporting move? ie. Baton Pass
        public bool phase = false; //Is a phasing move? ie. Whirlwind and Haze

        public Move(string n, Type t, float p) { name = n; type = t; bp = p; }
        public Move(string n, Type t) { name = n; type = t; unknown = true; }

    }


    class Movelist
    {
        public Movelist()
        {
        }

        public void initialize()
        {
            Move toxic = new Move("Toxic", Global.types["poison"], 0); toxic.status = true; Global.moves.Add("Toxic", toxic);
            Move willo = new Move("Will-O-Wisp", Global.types["fire"], 0); willo.status = true; Global.moves.Add("Will-O-Wisp", willo);
            Move swordsdance = new Move("Swords Dance", Global.types["normal"], 0); swordsdance.boost = true; Global.moves.Add("Swords Dance", swordsdance);

        }
    }
}
