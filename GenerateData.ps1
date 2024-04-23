$OutputPath = ".\MissionControl\wwwroot\data"

# Vanilla
dotnet run --project InfraredEyepiece --property WarningLevel=0 --OutputPath $OutputPath --CreateManifest true --Command ImportVanilla

# Cauldron
dotnet run --project InfraredEyepiece --property WarningLevel=0 --OutputPath $OutputPath --CreateManifest true --ModPublishedFileId 2437294168

# Cauldron Promos
dotnet run --project InfraredEyepiece --property WarningLevel=0 --OutputPath $OutputPath --CreateManifest true --ModPublishedFileId 2789983243

# Case Files
dotnet run --project InfraredEyepiece --property WarningLevel=0 --OutputPath $OutputPath --CreateManifest true --ModPublishedFileId 2436980109

# Menagerie of the Multiverse
dotnet run --project InfraredEyepiece --property WarningLevel=0 --OutputPath $OutputPath --CreateManifest true --ModPublishedFileId 2616657693

# Spooky Ghostwriter Comics
dotnet run --project InfraredEyepiece --property WarningLevel=0 --OutputPath $OutputPath --CreateManifest true --ModPublishedFileId 2436888590
