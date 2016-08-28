using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
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
    
    //TODO:
    public class Move
    {
        public Type type;
        public string name;
        public float bp = 0;
        public float accuracy = 1f;
        public string group = "none"; //special or physical
        public int priority = 0;
        public bool unknown = false; //Given to moves which are not explicitly defined within this file.
        public bool isBoost = false; //Is a boosting move? ie. Swords Dance
        public bool status = false; //Is a status move? ie. Toxic
        public bool support = false; //Is a supporting move? ie. Baton Pass
        public bool phase = false; //Is a phasing move? ie. Whirlwind
        public bool heal = false;
        public bool field = false; //Hazard move?
        public Boosts boosts;
        public string statuseffect = "none";
        public Move(string n, Type t, float p) { name = n; type = t; bp = p; }
        public Move(string n, Type t) { name = n; type = t; unknown = true; bp = -1; }
        public string desc;
        public Move(MoveJSONObj obj)
        {
            name = obj.name;
            type = Global.types[obj.type.ToLower()];
            bp = obj.basePower;
            accuracy = ((float)obj.accuracy / 100f);
            group = obj.category.ToLower();
            priority = obj.priority;
            boosts = obj.boosts;
            desc = obj.desc;
            statuseffect = obj.status;
            if (statuseffect != "none") status = true;
            if (hasBoosts()) isBoost = true;
            if (Convert.ToBoolean(obj.flags.heal)) heal = true; 
            
        }
        public bool hasBoosts()
        {
            if (!Object.ReferenceEquals(boosts, null))
                return (boosts.total() > 0) ? true : false; //TODO: follow a similar method for finding moves that drop stats.
            else
                return false;
        }

    }


    class Movelist
    {
        string path = MOVELISTPATH;
        public Movelist()
        {
        }

        public void initialize()
        {
            readJson();
        }

        private void readJson()
        {
            if (!File.Exists(path))
            {
                cwrite("Could not open movelist.txt", "error", COLOR_ERR);
                cwrite("Analytic mode will not work properly.", "[!]", COLOR_WARN);
                cwrite("Attempting to continue", COLOR_OK);
                return;
            }
            using (var reader = new StreamReader(path))
            {
                string json;
                json = reader.ReadToEnd();
                JObject jo = JsonConvert.DeserializeObject<JObject>(json);
                string allmoves = jo.First.ToString();
                var current = jo.First;
                for (int i = 0; i< jo.Count;i++)
                {
                    MoveJSONObj mv = JsonConvert.DeserializeObject<MoveJSONObj>(current.First.ToString());
                    Move move = new Move(mv);
                    Global.moves.Add(move.name, move);
                    current = current.Next;
          
                }
            }
        }
        

        private bool isSet(string v)
        {
            int a;
            if (int.TryParse(v,out a))
            {
                return Convert.ToBoolean(a);
            }
            else
            {
                try
                {
                    return Convert.ToBoolean(v);
                }
                catch
                {
                    cwrite("Bad value: " + v, "error", COLOR_ERR);
                    return false;
                }
            }
        }
    }
}
