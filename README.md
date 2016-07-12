# ShowdownBot
A bot for Pokemon Showdown made using C# and the Selenium library.

For a list of recent changes and features, see [the changelog](ShowdownBot/changelog.md)

##Requirements
  * .NET Framework 4.0
  * Firefox 46 (There seems to be problems for 47, at least for me)

##Getting started
Before running the program, be sure to edit the botInfo.txt file. The file contains the following parameters.
* username - Name of the bot's account.
* password - its password.
* profile - This is the name of the firefox profile used by the bot.
* show_debug - Set this to true if you want to see debug messages in the console, false if you do not.

To set up a Firefox profile for the bot to use, open the run console (Windows key + R) and type

`firefox.exe -ProfileManager -no-remote`

Click create a profile. Make sure the name of this profile corresponds to the name in the botInfo.txt. If you do not create a profile, the bot will automatically use its own, however it is recommended to do so as there is no other way to save team and site information.

Once you have the prerequisites configured, to start the bot, use the command **start**. This will attempt to log in as the specified user in the config.
If you leave an account logged in on the browser, you can use **startf** to skip the login/logout process.

To start a battle, switch to the desired module (see below) and type **challenge** or **cp** to challenge a player, or leave it blank to challenge the owner.

## Modules
The bot works by using various modules. They are:
  * Random - The bot randomly picks moves and pokemon, and does not switch pokemon unless one faints.
  * Biased - The same as Random, except moveslots are weighted.
  * Analytic - The bot compares the two pokemon battling and acts accordingly.

To switch modules, simply type **module** or **m** followed by one of the module types above, in lowercase.



##Other commands
* info - Displays the bot's current mode and state.
* kill - Stops the bot.
* exit - Kills the bot and closes the console and browser.
* See the help command for a full list of commands available.
