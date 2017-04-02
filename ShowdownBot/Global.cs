using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static ShowdownBot.GlobalConstants;
namespace ShowdownBot
{
    public enum State
    {
        IDLE,
        BATTLE,
        SEARCH,
        CHALLENGE,
        FORFEIT,
        BUSY

    };

    public enum Weather
    {
        RAIN,
        SUN,
        SAND,
        HEAVYRAIN,
        HARSHSUN,
        STRONGWIND,
        HAIL,
        NONE
    }
    /// <summary>
    /// Contains variables utilized by multiple classes.
    /// </summary>

    public static class Global
    {
        //---------Helper Information

        //Options
        public static bool showDebug = false;
        public static bool botIsReady = false;
        public static bool updateOnStart = true;
        public static float m1wgt = 0.4f;
        public static float m2wgt = 0.3f;
        public static float m3wgt = 0.2f;
        public static float m4wgt = 0.1f;
        public static string PROFILE_NAME = "Profile 1";
        public static string CD_ARGS = "--log-level=3";
        public static string USERDATA_PATH = @"C:/User_Data";
        public static string DBPATH = @"./data/mtdb.sdb";
        public static string POKEBASEPATH = @"./data/pokedex.js";
        public static string MOVELISTPATH = @"./data/moves.js";
        public static string ERRLOGPATH = @"./error.txt";
        public static string ROLEPATH = @"./data/roleOverride.js";
        public static string HELPPATH = @"./data/help.xml";
        
        //Encyclopedia
        public static Dictionary<string, Type> types;
        public static Dictionary<string, Move> moves;
        public static Dictionary<string, Pokemon> pokedex;
        public static IWebDriver gBrowserInstance;

        public static Queue<String> outputBuffer = new Queue<string>(100);

        /// <summary>
        /// Initialization method for the type dictionary. This MUST be called during intializaiton fo the bot.
        /// </summary>
        public static void setupTypes()
        {
            types = new Dictionary<string, Type>();

            #region Declarations
            Type fire = new Type("fire");
            Type water = new Type("water");
            Type grass = new Type("grass");
            Type ice = new Type("ice");
            Type dark = new Type("dark");
            Type steel = new Type("steel");
            Type fairy = new Type("fairy");
            Type poison = new Type("poison");
            Type psychic = new Type("psychic");
            Type bug = new Type("bug");
            Type flying = new Type("flying");
            Type ground = new Type("ground");
            Type electric = new Type("electric");
            Type dragon = new Type("dragon");
            Type normal = new Type("normal");
            Type ghost = new Type("ghost");
            Type rock = new Type("rock");
            Type fighting = new Type("fighting");

            Type error = new Type("error");
            #endregion

            #region Characteristics
            fire.se = new Type[] { grass, ice, steel };
            fire.res = new Type[] { rock, water, steel, dragon };
            types.Add(fire.value, fire);
            water.se = new Type[] { fire, rock, ground };
            water.res = new Type[] { dragon, water, grass };
            types.Add(water.value, water);
            grass.se = new Type[] { ground, rock, water };
            grass.res = new Type[] { bug, dragon, fire, flying, grass, poison, steel };
            types.Add(grass.value, grass);
            ice.se = new Type[] { dragon, flying, grass, ground };
            ice.res = new Type[] { fire, ice, steel, water };
            types.Add(ice.value, ice);
            dark.se = new Type[] { ghost, psychic };
            dark.res = new Type[] { dark, fairy, fighting };
            types.Add(dark.value, dark);
            steel.se = new Type[] { ice, fairy, rock };
            steel.res = new Type[] { electric, fire, steel, water };
            types.Add(steel.value, steel);
            fairy.se = new Type[] { dark, dragon, fighting };
            fairy.res = new Type[] { fire, poison, steel };
            types.Add(fairy.value, fairy);
            poison.se = new Type[] { fairy, grass };
            poison.res = new Type[] { ghost, ground, poison, rock };
            poison.nl = new Type[] { steel };
            types.Add(poison.value, poison);
            psychic.se = new Type[] { fighting, poison };
            psychic.res = new Type[] { psychic, steel };
            psychic.nl = new Type[] { dark };
            types.Add(psychic.value, psychic);
            bug.se = new Type[] { dark, grass, psychic };
            bug.res = new Type[] { fairy, fighting, fire, flying, ghost, poison, steel };
            types.Add(bug.value, bug);
            flying.se = new Type[] { bug, fighting, grass };
            flying.res = new Type[] { electric, rock, steel };
            types.Add(flying.value, flying);
            ground.se = new Type[] { electric, fire, poison, rock, steel };
            ground.res = new Type[] { bug, grass };
            ground.nl = new Type[] { flying };
            types.Add(ground.value, ground);

            electric.se = new Type[] { flying, water };
            electric.res = new Type[] { dragon, electric, grass };
            electric.nl = new Type[] { ground };
            types.Add(electric.value, electric);
            dragon.se = new Type[] { dragon };
            dragon.res = new Type[] { steel };
            dragon.nl = new Type[] { fairy };
            types.Add(dragon.value, dragon);
            normal.res = new Type[] { rock, steel };
            normal.nl = new Type[] { ghost };
            types.Add(normal.value, normal);
            ghost.se = new Type[] { ghost, psychic };
            ghost.res = new Type[] { dark };
            ghost.nl = new Type[] { normal };
            types.Add(ghost.value, ghost);
            rock.se = new Type[] { flying, ice, fire, bug };
            rock.res = new Type[] { fighting, ground, steel };
            types.Add(rock.value, rock);
            fighting.se = new Type[] { dark, ice, normal, rock, steel };
            fighting.res = new Type[] { bug, fairy, flying, poison, psychic };
            fighting.nl = new Type[] { ghost };
            types.Add(fighting.value, fighting);

            types.Add(error.value, error);
            #endregion

        }

        /// <summary>
        /// Safer and easier method of looking up a pokemon in the pokedex
        /// than to just access the field directly.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Pokemon lookup(string name)
        {
            string _name = name.ToLower();
            Pokemon p;
            try
            {
                p = pokedex[_name];
            }
            catch (Exception)
            {
                //Tried to look up a personal version, however there was none found.
                if (_name.Contains(PERSONAL_PRE))
                {
                    try
                    {
                        p = pokedex[_name.Split('_')[1]]; //search for portion after "my_"
                        return p;
                    }
                    catch (Exception ex)
                    {
                        cwrite(ex.Message + "ON PKMN:"+_name.Split('_')[1], "warning", COLOR_WARN);
                    }
                }
                cwrite("Unknown pokemon " + _name, "warning", COLOR_WARN);
                return pokedex["error"];
            }
            return p;
        }

        /// <summary>
        /// Returns the specified Move object. Returns the dummy error move if not found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Move moveLookup(string name)
        {
            Move m;
            TextInfo ti = new CultureInfo("en-us").TextInfo;
            if (name != "U-turn")
                name = ti.ToTitleCase(name);
            try
            {
                m = moves[name];
            }
            catch (Exception e)
            {
                cwrite("ON MOVE LOOKUP " + name + ":\n" + e,"error",COLOR_ERR);
                return moves["error"];
            }
            return m;
        }
        /// <summary>
        /// Sleeps the thread for the specified time.
        /// </summary>
        /// <param name="timeInMiliseconds"></param>
        public static void wait(int timeInMiliseconds)
        {
            System.Threading.Thread.Sleep(timeInMiliseconds);
        }
        public static void wait()
        {
            //basic wait of 2 seconds
            wait(2000);
        }
        
        /// <summary>
        /// Searches for a web element by from within the container toSearch.
        /// </summary>
        /// <param name="toSearch"></param>
        /// <param name="by"></param>
        /// <returns></returns>
        public static IWebElement findWithin(IWebElement toSearch, By by)
        {
            try
            {
                return toSearch.FindElement(by);
            }
            catch
            {
                //cwrite("Unable to find element " + by + " in element " + toSearch);
                return null;
            }
        }

        /// <summary>
        /// Waits until an element is present and returns that element.
        /// Returns null if not found.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="maxw"></param>
        /// <returns></returns>
        public static IWebElement waitFind(By by, int maxw = MAX_WAIT_TIME_S)
        {
            WebDriverWait _wait = new WebDriverWait(gBrowserInstance, TimeSpan.FromSeconds(maxw));
                try
                {
                    return _wait.Until<IWebElement>(ExpectedConditions.ElementExists(by));
                }
                catch (NoSuchElementException )
                {
                    cwrite("Couldn't find element " + by.ToString());
                    return null;
                }
                catch (WebDriverTimeoutException )
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            
        }


       
        /// <summary>
        /// Waits until an element is available and then clicks it.
        /// </summary>
        /// <param name="by"></param>
        /// <returns>Whether the element was clicked.</returns>
        public static bool waitFindClick(By by, int maxw = MAX_WAIT_TIME_S)
        {
            IWebElement we = waitFind(by,maxw);
            if (we != null)
            {
                try
                {
                    we.Click();
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    //wait and try again.
                    wait();
                    we = waitFind(by);
                    if (we != null)
                    {
                        we.Click();
                        return true;
                    }
                    else
                        return false;
                }
            }
            else
            {
               return false;
            }
            
        }
        /// <summary>
        /// Returns whether an element matching the criteria by can be found.
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public static bool elementExists(By by)
        {
            try
            {
                gBrowserInstance.FindElement(by);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Waits until either the specified element exists,
        /// or it reaches MAX_WAITS.
        /// Returns true if elemement found.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="maxw"></param>
        /// <returns>true if the element was found
        ///          false if it times out while searching.
        /// </returns>
        public static bool waitUntilElementExists(By by, int maxw = 60)
        {
            if (waitFind(by, maxw) != null)
                return true;
            else
                return false;

        }
        /// <summary>
        /// Write a string to the console.
        /// </summary>
        /// <param name="t"></param>
        public static void cwrite(string t)
        {
            cwrite(t, COLOR_DEFAULT);
        }
        /// <summary>
        /// Writes a string of specified color to the console.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        public static void cwrite(string t, ConsoleColor c)
        {
            string date = GetTimestamp();
            Console.ForegroundColor = c;
            Console.WriteLine("[" + date + "]" + t);
            Console.ResetColor();
            saveOutputToConsoleBuffer("[" + date + "]" + t);

        }
        /// <summary>
        /// Writes a string of specified color to console, with a header prepended.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="header"></param>
        /// <param name="c"></param>
        public static void cwrite(string t, string header, ConsoleColor c)
        {
            header = header.Trim('[', ']').ToUpper();
            if ((!Global.showDebug) && (header == "DEBUG"))
            { Console.ResetColor(); return; }
            string date = GetTimestamp();
            Console.Write("[" + date + "]");
            Console.ForegroundColor = c;
            Console.Write("[" + header + "]");
            Console.ResetColor();
            Console.Write(t + "\n");
            saveOutputToConsoleBuffer("[" + date + "]" + "[" + header + "]" + t);

        }
        /// <summary>
        /// Gets the time in 24-Hours:Minutes:Seconds format
        /// </summary>
        /// <returns></returns>
        public static string GetTimestamp()
        {
            string dt = DateTime.Now.ToString("HH:mm:ss");
            return dt;
        }


        /// <summary>
        /// Saves string s to the console output buffer.
        /// </summary>
        /// <param name="s"></param>
        public static void saveOutputToConsoleBuffer(string s)
        {
            if (outputBuffer.Count > 99)
                outputBuffer.Dequeue(); //Clear out oldest after 100 entries.
            outputBuffer.Enqueue(s);
        }

        /// <summary>
        /// Writes the console output buffer to the logs folder.
        /// </summary>
        public static void writeConsoleOutput()
        {
            if (!Directory.Exists(@"./logs/"))
                Directory.CreateDirectory(@"./logs/");
            using (var writer = new StreamWriter(@"./logs/consoleout_" + DateTime.Now.ToString("yyyy-MM-dd H-mm-ss") + ".txt"))
            {
                while (outputBuffer.Count != 0)
                {
                    writer.WriteLine(outputBuffer.Dequeue());
                }
            }
           
        }

        /// <summary>
        /// Debugging method that enumerates some of the fields and their values for any given object.
        /// </summary>
        /// <param name="obj"></param>
        public static void var_dump(object obj)
        {
            System.Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields();
            PropertyInfo[] props = t.GetProperties();
            for (int i = 0; i < fields.Length; i++)
            {
                try
                {
                    cwrite(fields[i].Name +" | "+ fields[i].GetValue(obj),"debug",COLOR_OK);
                    
                }
                catch (Exception)
                {
                   
                }
            }
            if (Object.ReferenceEquals(props, null))
                return;
            foreach (PropertyInfo p in props)
            {
                try
                {
                    cwrite(p.Name + " | " + p.GetValue(obj), "debug", COLOR_OK);
                }
                catch(Exception)
                {

                }
            }
        }

        /// <summary>
        /// Writes the error to error.txt. If fatal, then the program exits. This method also writes the console output buffer to a sperate log.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="fatal"></param>
        public static void logError(Exception ex, bool fatal)
        {
            using (StreamWriter sw = new StreamWriter("error.txt", true))
            {

                sw.WriteLine("----------");
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] - On: " + Environment.OSVersion.ToString());
                sw.WriteLine("ERROR:" + ex.Message);
                sw.WriteLine(ex.StackTrace);
                writeConsoleOutput();

            }
            if (fatal)
            {
                cwrite("A fatal error has occured. See error.txt for more info.", COLOR_ERR);
                cwrite(ex.Message, "debug", COLOR_ERR);
#if !DEBUG
                Console.ReadLine();
                Environment.Exit(-1);
#endif
            }
            else
                cwrite("An error has occured:\n" + ex.Message, COLOR_WARN);
            

        }
    }
}
       