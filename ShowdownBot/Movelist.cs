using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowdownBot
{

    /// <summary>
    /// If a move is non damaging (0 bp) it is considered
    /// to be a status move.
    /// Moves not defined below are considered unknown, and
    /// do not account for BP in decision making, only typing.
    /// Therefore it is advisable to define all commonly used
    /// status moves.
    /// </summary>
    public class Move
    {
        public Type type;
        public string name;
        public float bp = 0;
        public string group = "none"; //special or physical
        public int priority = 0;
        public bool unknown = false; //Given to moves which are not explicitly defined within this file.
        public bool boost = false; //Is a boosting move? ie. Swords Dance
        public bool status = false; //Is a status move? ie. Toxic
        public bool support = false; //Is a supporting move? ie. Baton Pass
        public bool phase = false; //Is a phasing move? ie. Whirlwind
        public bool field = false; //Hazard move?
        public Move(string n, Type t, float p) { name = n; type = t; bp = p; }
        public Move(string n, Type t) { name = n; type = t; unknown = true; bp = -1; }

    }


    class Movelist
    {
        public Movelist()
        {
        }

        public void initialize()
        {
           /// The way the bot functions, it's imperative to at least define status moves that it will use, otherwise it is likely that
           /// it will either never pick them or pick them at inappropriate times.
        #region Status Moves
            Move toxic = new Move("Toxic", Global.types["poison"], 0); toxic.status = true; Global.moves.Add("Toxic", toxic);
            Move willo = new Move("Will-O-Wisp", Global.types["fire"], 0); willo.status = true; Global.moves.Add("Will-O-Wisp", willo);
            Move swordsdance = new Move("Swords Dance", Global.types["normal"], 0); swordsdance.boost = true; Global.moves.Add("Swords Dance", swordsdance);
            Move dragondance = new Move("Dragon Dance", Global.types["dragon"], 0); dragondance.boost = true; Global.moves.Add(dragondance.name, dragondance);
            Move roost = new Move("Roost", Global.types["flying"], 0); roost.support = true; Global.moves.Add(roost.name, roost);
            Move leechseed = new Move("Leech Seed", Global.types["grass"], 0); leechseed.status = true; Global.moves.Add(leechseed.name, leechseed);
            Move sr = new Move("Stealth Rock", Global.types["rock"], 0); sr.field = true; Global.moves.Add(sr.name, sr);
        #endregion

            #region Fire Moves
            Move fireblast = new Move("Fire Blast", Global.types["fire"], 110); fireblast.group = "special"; Global.moves.Add(fireblast.name, fireblast);
            Move flareblitz = new Move("Flare Blitz", Global.types["fire"], 120); flareblitz.group = "physical"; Global.moves.Add(flareblitz.name, flareblitz);
            Move flamethrower = new Move("Flamethrower", Global.types["fire"], 90); flamethrower.group = "special"; Global.moves.Add(flamethrower.name, flamethrower);
            #endregion

            #region Water
            Move hydro = new Move("Hydro Pump", Global.types["water"], 110); hydro.group = "special"; Global.moves.Add(hydro.name, hydro);
            Move scald = new Move("Scald", Global.types["water"], 80); hydro.group="special"; Global.moves.Add(scald.name,scald);
            Move jet = new Move("Aqua Jet", Global.types["water"], 40); hydro.group = "physical"; Global.moves.Add(jet.name, jet);
            Move waterfall = new Move("Waterfall", Global.types["water"], 80); waterfall.group = "physical"; Global.moves.Add(waterfall.name, waterfall);

            #endregion

            #region Grass
            #endregion

            #region Bug
            Move bbite = new Move("Bug Bite", Global.types["bug"], 60); bbite.group = "physical"; Global.moves.Add(bbite.name, bbite);
            Move xscissor = new Move("X-Scissor", Global.types["bug"], 80); xscissor.group = "physical"; Global.moves.Add(xscissor.name, xscissor);
            Move bbuzz = new Move("Bug Buzz", Global.types["bug"], 80); bbuzz.group = "special"; Global.moves.Add(bbuzz.name, bbuzz);
            #endregion

            #region Ground
            Move eq = new Move("Earthquake", Global.types["ground"], 100); eq.group = "physical"; Global.moves.Add(eq.name, eq);
            Move ep = new Move("Earth Power", Global.types["ground"], 90); ep.group = "special"; Global.moves.Add(ep.name, ep);
            #endregion

            #region Dark
            Move knockoff = new Move("Knock Off", Global.types["dark"], 65); knockoff.group = "physical"; Global.moves.Add(knockoff.name, knockoff);

            #endregion

            #region Dragon
            Move dclaw = new Move("Dragon Claw", Global.types["dragon"], 80); dclaw.group = "physical"; Global.moves.Add(dclaw.name, dclaw);

            #endregion

            #region Steel
            Move gyro = new Move("Gyro Ball", Global.types["steel"], -2); gyro.group = "physical"; Global.moves.Add(gyro.name, gyro);
            Move flashc = new Move("Flash Cannon", Global.types["steel"], 80); flashc.group = "special"; Global.moves.Add(flashc.name, flashc);
            Move ih = new Move("Iron Head", Global.types["steel"], 80); ih.group = "physical"; Global.moves.Add(ih.name, ih);
            
            #endregion
            
            #region Electric
            Move vosw = new Move("Volt Switch", Global.types["electric"], 70); vosw.group = "special"; Global.moves.Add(vosw.name, vosw);

            #endregion

            #region Ghost
            Move sball = new Move("Shadow Ball", Global.types["ghost"], 80); sball.group = "special"; Global.moves.Add(sball.name, sball);

            #endregion

            #region Fighting
            Move focbla = new Move("Focus Blast", Global.types["fighting"], 120); focbla.group = "special"; Global.moves.Add(focbla.name, focbla);

            #endregion

            #region Poison
            Move swave = new Move("Sludge Wave", Global.types["poison"], 95); swave.group = "special"; Global.moves.Add(swave.name, swave);

            #endregion

            #region Flying
            Move brabir = new Move("Brave Bird", Global.types["flying"], 120); brabir.group = "physical"; Global.moves.Add(brabir.name, brabir);
            #endregion

            #region Fairy
            Move plyr = new Move("Play Rough", Global.types["fairy"], 90); plyr.group = "physical"; Global.moves.Add(plyr.name, plyr);

            #endregion
        }
    }
}
