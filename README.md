# AIO Sandbox

This project is an attempt to make a plugin based "All In One" sandbox server supports all pre-Cata builds of WoW with an emphasis on exploration.

You'll need [.Net Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2) to run this. This project was built with Visual Studio 2017.

##### Builds: #####
Clients and Patches can be found at the following links:
- [www.ownedcore.com](http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-model-editing/406638-collection-exploration-patches-of-various-locations.html)
- [www.getmangos.eu](https://www.getmangos.eu/downloads/category/7-world-of-warcraft-patches/)
- [www.patches-scrolls.de](https://www.patches-scrolls.de/patch/4875/7/)


| Build                                  | Status                      |
| -------------------------------------- | :-------------------------- |
| Pre Release (0.5.3 - 1.0.1)            | Working                     |
| Vanilla (1.1.0 - 1.12.x)               | Working                     |
| Burning Crusade (2.0.0 - 2.4.3)        | Working                     |
| Wrath of the Lich King (3.0.1 - 3.3.5) | Working                     |
| Cataclysm (4.0.0-4.0.0.12319)          | Working but buggy           |
| Mists of Pandaria (5.0.1.15464)        | Can login and teleport maps |

##### Commands: #####
* **.demorph** : resets the current morph state
* **.help** : lists all commands and their parameters
* **.fly {on | off}**: enables/disables flying; supported from 2.0.0.5965
* **.gps** : displays your current co-ordinates
* **.go {name}** : teleports you to the specified location; partial locations will suggest available options
* **.go {x} {y} {z} Optional: {mapid}** : teleports you to the supplied co-ordinates and map
* **.go instance {name}** : teleports inside the specified instance; partial locations will suggest available options
* **.go instance {id}** : teleports inside the specified instance
* **.morph {id}** : morphs the player to the specified model
* **.nudge Optional: [0-100] {z offset}** : teleports you forward X * one step in the direction you're facing and optionally adjusts your Z co-ordinate
* **.speed [0.1 - 1000] Optional: {run | swim | fly | all}** : sets your speed; defaults to 'all' if no type is specified

For some interesting places to visit have a look at [Marlamin's map viewer](https://newmaps.marlam.in) - co-ordinates can be toggled with the "Enable technical details?" checkbox.

##### Note: ######
By default your account will be granted access to the latest expansion however if you have manually patched between expansions you will get a version mismatch error on login (your client won't be complete). To bypass this either change the `Expansion` field in the `settings.conf` file to `0` or reinstall using an official expansion disk.