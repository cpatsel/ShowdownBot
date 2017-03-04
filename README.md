# ShowdownBot
A bot for Pokemon Showdown made using C# and the Selenium library.

For a list of recent changes and features, see [the changelog](ShowdownBot/changelog.md)

##Requirements
### Windows
  * [.NET Framework 4.6](https://www.microsoft.com/en-us/download/details.aspx?id=48130)
  * Google Chrome version 54-56

### Linux
 * [See the install/compile guide.](https://github.com/Deviach/ShowdownBot/wiki/Linux-Compilation-and-Install)

### Mac
 * [Install mono](http://www.mono-project.com/docs/getting-started/install/mac/).
 * Download or compile the source and run with mono. See the linux section for more info (it should be a similar process).

##Getting started
Before running the program, be sure to edit the botInfo.txt file. The file contains the following parameters.

* username - Name of the bot's account.
* password - its password.
* owner - Name of your account. Used for some things, but not necessary.
* userdata_path - Path to your google chrome user data folder. For Windows 7, it should resemble the default, substituting your computer username.
* profile - The name of the profile. If you don't wish to use a profile, leave it as is.
* update_onstart - Whether to check for updates at startup.
* default_module - Default [module](https://github.com/Deviach/ShowdownBot/wiki/Modules) that the bot starts in.
* [Biased Moveslot weights](https://github.com/Deviach/ShowdownBot/wiki/Modules#biased)
* show_debug - Set this to true if you want to see debug messages in the console, false if you do not.
* chromeargs - (Relative) filepath to a file containing arguments passed to chrome when starting the browser.

If you don't know what the last three mean, don't worry about them, they're fine to leave default.


### Setting up a profile

If you want to be able to save a team to use with the bot, you will need to set up a profile. This can be done in google chrome by clicking the icon in the top-right corner. Once you've made a profile, you may visit showdown while on the profile and set up the teams you want.

Afterwards, navigate to your Google Chrome User Data folder and copy the path into the [USERDATA_PATH] parameter in botInfo.txt. After you have done this, find the folder that corresponds to the profile you've made (it will be something like Profile 1, Profile 2, etc) and write the name of the folder into the [PROFILE] parameter. 

Now you've got a working profile!

### Starting the bot.

Once you have the prerequisites configured, to start the bot, use the command **start**. This will attempt to log in as the specified user in the config.
If you want to log in as another account, or not register an account, you can call the start command with the following arguments:

``start -u <username> -p <password>``

Where ``<username>`` is the username of the account and ``<password>`` is the (optional) password to log into a registered account.

To start a battle, switch to the desired module (see below) and type **challenge** or **cp** to challenge a player, or leave it blank to challenge the owner. 
Additionally, to have the bot challenge a random player on the ladder, type **search**.

Each module has a default format it will search for matches or challenge players to. This can be changed with the **format** command. This command is case-sensitive and is typically the generation + whatever the format is called on Pokemon Showdown with no spaces and all lowercase. So Random Battle is "gen7randombattle" and OU would be "gen7ou" and so on. A more comprehensive list will be available soon, but in the meantime if you are unsure, inspect the element in your browser and copy the "value" of the format you want.


## Modules

The bot works by using various modules. They are:

  * Random - The bot randomly picks moves and pokemon, and does not switch pokemon unless one faints.
  * Biased - The same as Random, except moveslots are weighted.
  * Analytic - The bot compares the two pokemon battling and acts accordingly.

[See a more detailed explanation here](https://github.com/Deviach/ShowdownBot/wiki/Modules)

To switch modules, simply type **module** or **m** followed by one of the module types above, in lowercase.
Do note, that when switching modules with the module command, the bot's format will be reset to whatever its default is defined as.


##Other commands
* **info** - Displays info about the current bot, or about certain moves or Pokemon.
* **kill** - Stops the bot.
* **exit** - Kills the bot and closes the console and browser.
* See the **help** command for a full list of commands available, and use ``help <command>`` to see more information about it.

##Bug reporting
You may submit bug reports or questions/suggestions to the github issue tracker for the project. For bug reports, it is recommended that you submit your error.txt (if you have one) and use the **save** command if you can to save console output to the logs folder (it will be created if you do not have one).