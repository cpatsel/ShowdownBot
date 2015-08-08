# ShowdownBot
A bot for Pokemon Showdown made using C# and the WatiN library.

##Requirements
The bot may require .NET framework 4.0 in order to run. This may not be the case in the future, but for now it is. Installer Soon™

##Getting started
Before running the program, be sure to edit the ShowdownBot.exe.config file. The file contains the following parameters.
* username - Name of the bot's account.
* password - its password.
* owner - This is the name of your account. Technically, it's the name of the account who the will challenge
* site - The website the bot opens up. It defaults to the showdown website, but alternatives running the same software should be compatible (as long as they're formatted the same way).

To start the bot, use the command **start**. This will attempt to log in as the specified user in the config. Since it's a bit buggy, for now just use **startf**, which skips authentication.
Note that if using the **startf** command (and you should) the username/password field will not be used. Additionally, since the bot will neither log in nor out this way, leaving an account
logged in will allow the bot to use it.

Once the bot has been started, you will need to change it's state.

##States
The bot is controlled through manipulating various states. They are:
* Battlerandom - Initiates a Random Battle.
* BattleOU - Initiates an OU battle
* Idle - The bot waits for further instruction
* ChallengePlr - Challenges a specific player (unimplemented)

Currently, both battlerandom and battleou are set to challenge the "owner", instead of battling on the ladder.

The state can be changed with the command **changestate** and any of the above parameters. As an example, to change state to BattleOU type:
> changestate ou

Changing state within a battle may have unintended consequences, and for the time being should be avoided.

##Modes
The bot has different AI modes that determine how it will battle.
* Random - The bot will pick moves at random. It will not switch pokemon until one has fainted.
* Bias - The bot will pick a random move based on the predetermined weight for each moveslot.
* Analytic - (Unimplemented)

To change the mode, enter the **mode** command followed by the parameter (random / r, bias / b). ie.
> mode b

Modes can be changed amidst a battle.

##Other commands
* info - Displays the bot's current mode and state.
* kill - Stops the bot.