# Vanilla
dotnet run --project InfraredEyepiece --property WarningLevel=0 --Command ImportVanilla

# Cauldron
dotnet run --project InfraredEyepiece --property WarningLevel=0 --Command ImportMod --ModPublishedFileId 2437294168 --SortOrder 1

# Cauldron Promos
dotnet run --project InfraredEyepiece --property WarningLevel=0 --Command ImportMod --ModPublishedFileId 2789983243 --SortOrder 2

# Case Files
dotnet run --project InfraredEyepiece --property WarningLevel=0 --Command ImportMod --ModPublishedFileId 2436980109 --SortOrder 3

# Menagerie of the Multiverse
dotnet run --project InfraredEyepiece --property WarningLevel=0 --Command ImportMod --ModPublishedFileId 2616657693 --SortOrder 4

# Spooky Ghostwriter Comics
dotnet run --project InfraredEyepiece --property WarningLevel=0 --Command ImportMod --ModPublishedFileId 2436888590 --SortOrder 5

# List all in manifest
dotnet run --project InfraredEyepiece --property WarningLevel=0 --Command CreateManifest
