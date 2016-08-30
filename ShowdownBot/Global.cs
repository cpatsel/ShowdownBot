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
    /// <summary>
    /// Contains variables utilized by multiple classes.
    /// </summary>

    public static class Global
    {
        //---------Helper Information

        //Options
        public static bool showDebug = false;
        public static bool botIsReady = false;
        public static float m1wgt = 0.4f;
        public static float m2wgt = 0.3f;
        public static float m3wgt = 0.2f;
        public static float m4wgt = 0.1f;
        public static string FF_PROFILE = "sdb";
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

        public static string lastcmd = "";

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
                        p = pokedex[_name.TrimStart(PERSONAL_PRE.ToCharArray())];
                        return p;
                    }
                    catch
                    {

                    }
                }
                cwrite("Unknown pokemon " + _name, "warning", COLOR_WARN);
                return pokedex["error"];
            }
            return p;
        }
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
                Console.ForegroundColor = COLOR_WARN;
                Console.WriteLine("ON MOVE LOOKUP " + name + ":\n" + e);
                Console.ResetColor();
                return new Move(name, types["normal"]);
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
        /// Exception handling method for webbrowser.findElements
        /// Returns null on exceptions.
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public static IList<IWebElement> findElements(By by)
        {
            try
            {
               return gBrowserInstance.FindElements(by);
            }
            catch (Exception e)
            {
                cwrite("Something went horribly wrong finding some elements: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Exception handling method for finding elements within another web element.
        /// Returns null on exceptions.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="by"></param>
        /// <returns></returns>
        public static IList<IWebElement> findElementsFromWithin(IWebElement from, By by)
        {
            try
            {
                return from.FindElements(by);
            }
            catch (Exception e)
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
        /// or it reaches MAX_WAITS
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
        public static void cwrite(string t)
        {
            Console.WriteLine("[" + GetTimestamp() + "]" + t);
            Console.ResetColor();
        }
        public static void cwrite(string t, ConsoleColor c)
        {
            string date = GetTimestamp();
            Console.ForegroundColor = c;
            Console.WriteLine("[" + date + "]" + t);
            Console.ResetColor();

        }
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


        }
        public static string GetTimestamp()
        {
            string dt = DateTime.Now.ToString("HH:mm:ss");
            return dt;
        }


        public static void var_dump(object obj)
        {
            System.Type t = obj.GetType();
            FieldInfo[] props = t.GetFields();
            for (int i = 0; i < props.Length; i++)
            {
                try
                {
                    cwrite(props[i].Name +" | "+ props[i].GetValue(obj),"debug",COLOR_OK);
                    var_dump((object)props[i]);
                }
                catch (Exception)
                {
                   
                }
            }
        }

        public static void logError(Exception ex, bool fatal)
        {
            using (StreamWriter sw = new StreamWriter("error.txt", true))
            {

                sw.WriteLine("----------");
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] - On: " + Environment.OSVersion.ToString());
                sw.WriteLine("ERROR:" + ex.Message);
                sw.WriteLine(ex.StackTrace);

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
       