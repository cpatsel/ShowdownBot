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
        string path = MOVELISTPATH;
        public Movelist()
        {
        }

        public void initialize()
        {
            readText();
        }
        private void readText()
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
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("#") && line != "")
                    {
                        string[] pairs = line.Split(',');
                        Move m = new Move("",types["error"]);
                        for (int i = 0; i < pairs.Length; i++)
                        {
                            assignValue(pairs[i],ref m);
                        }
                        m.unknown = false;
                        moves.Add(m.name,m);
                        
                    }
                }
            }
        }
        private void assignValue(string kv,ref Move m)
        {
            string key = kv.Split(':')[0];
            string val = kv.Split(':')[1];
            TextInfo ti = new CultureInfo("en-us", false).TextInfo;
            if (key == "name") { m.name = ti.ToTitleCase(val); }
            else if (key == "type") { m.type = types[val]; }
            else if (key == "bp") { m.bp = int.Parse(val); }
            else if (key == "accuracy") { m.accuracy = (float.Parse(val) / 100f); }
            else if (key == "group") { m.group = val.ToLower(); }
            else if (key == "status") { m.status = isSet(val); }
            else if (key == "boost") { m.boost = isSet(val); }
            else if (key == "support") { m.support = isSet(val); }
            else if (key == "phase") { m.phase = isSet(val); }
            else if (key == "field") { m.field = isSet(val); }
            else if (key == "priority") { m.priority = int.Parse(val); }
            else
            {
                cwrite("Unknown key " + key + " in movelist.txt", "[!]", COLOR_WARN);
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
