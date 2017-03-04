## Changelog ##


### v1.0.0 / Unreleased ###
----


New:

* Now uses google chrome as the browser.
* Weather support
* "certainAbility" attribute for roleOverride pokemon.
* Check for updates on startup option.
* Default AI mode set in botInfo.txt
* Gen 7 Pokemon and Z-Moves support
* Save log output command (save)


Fixes:

* Ground moves will rank 0 against foes with levitate.
* Milk Drink and other normal recovery moves are treated correctly as  healing moves.

### v0.5.0 / 2016-09-08 ###
----
Added:

* Personalized Pokemon through the personal role override tag.
* Item Support
* Status move support
* Lead picking (automatic or with the lead role override)
* Update feature (kind-of)
* New, more responsive move ranking system.
* Better help documentation.
* Specify format when using challengeplayer (requires using the -u command for players).
* Refresh command.

Removed:

* Dump log functionality

Fixes:
* Reworked how forfeiting is handled.
* Can no longer use commands that require to bot to be running when it is not running.
* Bot will wait longer (2 minutes) when waiting for a player challenge.
* Actually really recognizes -ate moves now.
* Bug where bot would attempt to switch its last Pokemon
* Bug where the bot could not identify alternate form pokemon.

### v0.4.1 / 2016-08-26 ###
----
Fixes

* Bug where megas weren't updated properly.
* Bug where randombattle would never update team.
* Bug where bot would not know its own team in randombattle.
* Bug where bot's teams would carry over from previous battles.
* Various crashes when looking for web elements.

### v0.4.0 / 2016-08-16 ###
----
Added

* Status tracking.
* HP tracking.
* Custom roles and stat spreads for pokemon.
* Boost tracking.
* Bot will consider hp and boosts when checking to switch.
* Consolidated Linux and Windows version.

Fixes

* Bot will recognize Mr. Mime and Mime Jr.
* Correctly processes Hidden Power 
* Correctly processes Return and Frustration, Gryo Ball
* -ate ability handling.
* Usernames that are too long are truncated.

### v0.3.0 / 2016-08-02 ###
----
Added

* Continuous battling
* Complete analytic databases (moves and pokemon)
* Linux support
* Specify login when starting
* Additional information commands (see "help info")
* Stop command
* Forfeit command
* Upgraded to C#6
* Format argument for match search

Fixes

* Error when attempting to log in.


### v0.2.2 / 2016-07-16 ###
----
Fixes

* Fixed lengthy wait times in-between actions.
* Fixed error when killing bot.
* Fixed errors finding nonexistent elements.
* Included readme with release.

### v0.2.1 / 2016-07-15 ###
----
Fixes

* Modules will now correctly choose leads when necessary.
* Bot will close the browser when killed.
* Fixed compatibility issues with Firefox 47.0.1
* Updated selenium to 2.53.1

### v0.2.0 / 2016-07-12 ###
----


Added

* Laddering (search for random opponent)
* Changelog
* Version command
* User-set firefox profile
* Customisable Biased weights.
* Change formats with the format command.
* Improved info command.
* Better error handling


Fixes

* Bot will leave battle when it's over again.
* Improved help



### v0.1.0 / 2016-06-29 ###
----

* Initial development release.



