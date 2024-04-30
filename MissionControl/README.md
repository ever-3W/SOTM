# Mission Control

Mission Control is a C# web application designed for the Sentinels of The Multiverse digital card game. Inspired by the [Random Draft](https://liquipedia.net/dota2game/Game_Modes#Random_Draft) game mode from Dota 2, this app allows players to select a semi-random set of villain/hero/environment decks to play with.

### Usage

To run the application locally, run ```dotnet run --project MissionControl``` and navigate to [http://localhost:5294](http://localhost:5294) in a web browser to access the app.

By default, Mission Control will select 1 villain, 8 heroes, and 1 environment. Playtesting revealed that a selection of 8 hero options gave enough flexibility to form a team that synergizes well, while introducing enough variance to motivate picking a unique team each time.

Selecting decks for Team Villain Mode and OblivAeon mode are not yet supported.


## Data Storage

Mission Control stores all user-input data (collections, draft selections, settings) in your browser's [local storage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage), which makes it persist when the browser window is closed or the page is reloaded. *None of this data ever leaves your computer.*


## Importing Mods

Mission Control comes with the following *server-side* content loaded by default:
* All vanilla content (including all official expansion packs)
* All Earth-Prime content (including the Magical Mysteries Pack)
* [The Cauldron](https://steamcommunity.com/sharedfiles/filedetails/?id=2437294168) ([github](https://github.com/SotMSteamMods/CauldronMods))
* [Cauldron Promos](https://steamcommunity.com/sharedfiles/filedetails/?id=2789983243) ([github](https://github.com/SotMSteamMods/CauldronPromos))
* [Case Files](https://steamcommunity.com/sharedfiles/filedetails/?id=2436980109)
* [Menagerie of the Multiverse](https://steamcommunity.com/sharedfiles/filedetails/?id=2616657693)
* [Spooky Ghostwriter Comics](https://steamcommunity.com/sharedfiles/filedetails/?id=2436888590)

Additional mods can be loaded by parsing them using [Infrared Eyepiece](../InfraredEyepiece/README.md) into a JSON file and then importing them in from the **Collections** tab. 


## Additional Features

The **Settings** tab lets you adjust various settings, such as the number of heroes selected in the draft, or whether to weight each *hero* equally or each *hero variant* equally.

The **History** tab stores a log of all games completed in the app. This log can be exported/imported as a JSON file.
