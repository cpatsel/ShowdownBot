# ShowdownBot
A bot for Pokemon Showdown made using C# and the Selenium library.

For a list of recent changes and features, see [the changelog](ShowdownBot/changelog.md)

##Requirements
  * .NET Framework 4.0
  * Firefox 47.0.1

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
Additionally, to have the bot challenge a random player, type **search**.

Each module has a default format it will search for matches or challenge players to. This can be changed with the **format** command. This command is case-sensitive and is typically whatever the format is called on Pokemon Showdown with no spaces and all lowercase. So Random Battle is "randombattle" and OU would be "ou". A more comprehensive list will be available soon.


## Modules
The bot works by using various modules. They are:
  * Random - The bot randomly picks moves and pokemon, and does not switch pokemon unless one faints.
  * Biased - The same as Random, except moveslots are weighted.
  * Analytic - The bot compares the two pokemon battling and acts accordingly.

To switch modules, simply type **module** or **m** followed by one of the module types above, in lowercase.
Do note, that when switching modules with the module command, the bot's format will be reset. 


##Other commands
* info - Displays the bot's current mode and state.
* kill - Stops the bot.
* exit - Kills the bot and closes the console and browser.
* See the help command for a full list of commands available.
