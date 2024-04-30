# Infrared Eyepiece

Infrared Eyepiece is a command-line tool that automatically generates JSON files for Sentinels of The Multiverse mods for use with [Mission Control](../MissionControl/README.md). This JSON file is a summary of the villain, hero, and environment decks contained by that mod, and is generated using the mod manifest and DLL (decompiled with ILSpy).

## Usage

To parse an installed Steam Workshop mod, we must first find its *published file ID*. 

Navigate to the installed mods directory for Sentinels of the Multiverse (on Windows it should be `C:\Program Files (x86)\Steam\steamapps\workshop\content\337150`), which should contain folders with numeric names as below:


Use the `manifest.json` file in each folder to find the mod you want to import. The published file ID is mentioned in the `publishedFileId` field (it should also be equal to the name of the folder containing `manifest.json`).


To create the JSON file containing the mod's data, run the following command:
```
dotnet run --project InfraredEyepiece --Command ImportMod --ModPublishedFileId <publishedFileId>
```
To add a mod server-side, a `ModManifest.json` file is required. This file can be created by adding the flag `--CreateManifest true` to the above command. If the file already exists in the output directory, it creates it.
