using System.Text.Json;
using SOTM.InfraredEyepiece.Importers;

ImporterAggregator ia = new ImporterAggregator()
    .AddFromImporter(new CoreGameImporter())
    .AddFromImporter(new ModImporter("2437294168")) // Cauldron
    .AddFromImporter(new ModImporter("2789983243")) // Cauldron Promos
    .AddFromImporter(new ModImporter("2436980109")) // Case Files
    .AddFromImporter(new ModImporter("2616657693")) // Menagerie of the Multiverse
    .AddFromImporter(new ModImporter("2436888590")) // Spooky Ghostwriter Comics
    .ResolveHangingVariants();

Console.WriteLine(JsonSerializer.Serialize(ia, new JsonSerializerOptions() { WriteIndented = true }));
