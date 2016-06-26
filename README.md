# ShowdownBot
A bot for Pokemon Showdown made using C# and the Selenium library.

##Requirements
  * .NET Framework 4.0
  * Firefox 46 (There seems to be problems for 47, at least for me)

##Getting started
Before running the program, be sure to edit the botInfo.txt file. The file contains the following parameters.
* username - Name of the bot's account.
* password - its password.
* owner - This is the name of your account. Technically, it's the name of the account who the bot will challenge
* show_debug - Set this to true if you want to see debug messages in the console, false if you do not.

Next you will need to set up a Firefox profile for the bot to use. Open the run console (Windows key + R) and type
> firefox.exe -ProfileManager -no-remote

Click create a profile. In the future, you can customise what name the profile will have, but for now, the bot will be looking for a profile named "sdb".

Once you have the prerequisites configured, to start the bot, use the command **start**. This will attempt to log in as the specified user in the config.
If you leave an account logged in on the browser, you can use **startf** to skip the login/logout process.

To start a battle, switch to the desired module (see below) and type **challenge** or **cp** to challenge the owner.

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
