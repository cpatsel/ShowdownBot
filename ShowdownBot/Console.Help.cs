using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
namespace ShowdownBot
{
    

    /// <summary>
    /// Contains the in-console help documentation for commands.
    /// </summary>
    public partial class Consol
    {
        private void DisplayHelp()
        {
            string cmnds = "";
            var query = from c in helpdoc.Root.Descendants("command")
                        select c.Element("name").Value;
            foreach (string s in query)
            {
                cmnds += s + ", ";
            }
            writef("Available commands are:\n"+ cmnds, "system", COLOR_SYS);
        }

        public class CmdParam
        {
            public string id { get; set; }
            public string arg { get; set; }
            public string description { get; set; }
        }
        public class Command
        {
            public string name { get; set; }
            public string alias { get; set; } = "None";
            public string desc { get; set; }
            public List<CmdParam> args { get; set; }
            
        }
        private void help(string cmnd)
        {
            if (cmnd.ToLower() == "me")
            {
                writef("I'm a robot, not a miracle worker.", "system", COLOR_SYS);
                return;
            }
            var query = from c in helpdoc.Root.Descendants("command")
                        where (c.Element("name").Value == cmnd || c.Element("alias").Value.Contains(cmnd))
                        select new Command()
                        {
                            name = c.Element("name").Value,
                            alias = c.Element("alias").Value,
                            desc = c.Element("description").Value,
                            args = c.Elements("param").Select(cmdarg => new CmdParam()
                            {
                                id = cmdarg.Element("id").Value,
                                arg = cmdarg.Element("arg").Value,
                                description = cmdarg.Element("description").Value

                            }).ToList()

                        };
            if (!query.Any())
            {
                writef("Unknown command " + cmnd, "system", COLOR_SYS);
                return;
            }
            Command _c = query.First();
            string text = _c.name + "\n" +
                    "Alias:" + _c.alias + "\n" +
                    "Description: "+_c.desc + "\n" +
                    "Args: \n";
            foreach (CmdParam cp in _c.args)
            {
                if (cp.description != "")
                {
                    text += "\t -" + cp.id + "\t" + cp.arg + " " + cp.description + "\n";
                }
            }
            writef(text, "system", COLOR_SYS);


            
        }

    }
}
