# ShowdownBot
A bot for Pokemon Showdown made using C# and the WatiN library.

##Getting started
To start the bot, use the command **start**. This will attempt to log in as the specified user in the config. Since it's a bit buggy, for now just use **startf**, which skips authetication.

Once the bot has been started, you will need to change it's state.

##States
The bot is controlled through manipulating various states. They are:
* Battlerandom - Initiates a Random Battle.
* BattleOU - Initiates an OU battle
* Idle - The bot waits for further instruction
* ChallengePlr - Challenges a specific player (unimplemented)

Currently, both battlerandom and battleou are set to challenge the "owner", instead of battling on the ladder.

The state can be changed with the command **changestate** and any of the above parameters.
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
