# AIO Sandbox

This project is an attempt to make a plugin based "All In One" sandbox server that supports all the pre-release builds of WoW with an emphasis on exploration. 
You'll need [.Net Core 4.6.1](https://www.microsoft.com/en-gb/download/details.aspx?id=49981) to run this. This project was built with Visual Studio 2017.

##### Builds: #####

Clients for the below can be found [on this thread](http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-model-editing/406638-collection-exploration-patches-of-various-locations.html).

| Build                   | Status         |
| ----------------------- | :------------- |
| Alpha (0.5.3)           | Working		   |
| Beta 1 (0.5.5)          | Not Implemented|
| Beta 2 (0.6.0)          | Working		   |
| Beta 3 (0.7.X)          | Usable		   |
| Beta 3 (0.8.0)          | Usable		   |
| Beta 3 (0.9.0)          | Usable		   |
| Beta 3 (0.10.0 - 0.11.0)| Usable		   |
| Beta 3 (0.12.0 - 1.0.X) | Usable		   |
| TBC Alpha (2.0.0)		  | Can't login	   |

\* Those marked 'Usable' have intermittent area triggers however the below commands negate this issue.

##### Commands: #####
* **.demorph** : resets current morph state
* **.help** : lists all commands and their parameters
* **.gps** : displays your current co-ordinates
* **.go {name}** : teleports you to the specified location; partial locations will suggest available options
* **.go {x} {y} {z} Optional: {mapid}** : teleports you to the supplied co-ordinates and map
* **.go instance {name}** : teleports inside the specified instance; partial locations will suggest available options
* **.go instance {id}** : teleports inside the specified areatrigger
* **.morph {id}** : morphs the player to the specified model
* **.nudge [1-100]** : teleports you forward X * one step in the direction you're facing; ideal for invisible walls
* **.speed [0.1 - 10] Optional: {run | swim | all}** : sets your speed; defaults to 'all' if no type is supplied

For some interesting places to visit have a look at [Marlamin's map viewer](https://newmaps.marlam.in) - co-ordinates can be toggled with the "Enable technical details?" checkbox.
