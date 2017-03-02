using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using System.IO;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
namespace ShowdownBot.modules
{

    class ComparativeModule: BotModule
    {
        public bool setup = false;
        List<string> db;
        public ComparativeModule( Bot m,IWebDriver b)
            : base(m, b)
        {
            format = "gen7ou";
            db = new List<string>();
        }


        public void buildDB()
        {
            if (setup)
            {
                cwrite("MT database already setup.");
                return;
            }
            using (StreamReader sr = new StreamReader(Global.DBPATH))
            {
                while (!sr.EndOfStream)
                {
                    db.Add(sr.ReadLine());
                    /*string[] s = sr.ReadLine().Split('|');
                    users.Add(Global.lookup(s[0]));
                    moves.Add(Global.moveLookup(s[1]));
                    targets.Add(Global.lookup(s[2]));*/
                    
                }
            }
            setup = true;
        }

        private Move pickMove(Pokemon you, Pokemon enemy)
        {
            List<string> l = db.FindAll(x => x.Contains(you.name + "|") && x.Contains("|" + enemy.name));
            string[] moves = new string[l.Count];
            Dictionary<string,int> counts;
            for (int i = 0; i < l.Count; i++)
            {
                moves[i] = l[i].Split('|')[1];
            }
            counts = moves.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var ranks = counts.OrderByDescending(x => x.Value);
            var best = ranks.ElementAt(0);
            return Global.moveLookup(best.Key);
        }

        public void simulate(Pokemon you, Pokemon enemy)
        {
            Move m = pickMove(you, enemy);
            cwrite("I would pick " + m.name,"simulator",COLOR_SYS);
        }

    }
}
