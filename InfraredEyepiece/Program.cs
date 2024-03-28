using System.Text.Json;
using SOTM.InfraredEyepiece.Importers;
using Microsoft.Extensions.Configuration;
using SOTM.Shared.Models;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

if (config["Command"] == "ImportVanilla")
{
    var importer = new CoreGameImporter(config);
    var file = new FileStream(importer.GetOutputFilePath(), FileMode.Create);
    JsonSerializer.Serialize(file, importer.ParseResourcesV2(), new JsonSerializerOptions() { WriteIndented = true });
    file.WriteByte((byte) '\n');
    file.Flush();
    Console.WriteLine($"Parsed vanilla deck data saved at \"{importer.GetOutputFilePath()}\".");
}
else if (config["Command"] == "ImportMod")
{
    var importer = new ModImporter(config["ModPublishedFileId"], config);
    var file = new FileStream(importer.GetOutputFilePath(), FileMode.Create);
    var collection = importer.ParseResourcesV2();
    JsonSerializer.Serialize(file, collection, new JsonSerializerOptions() { WriteIndented = true });
    file.WriteByte((byte) '\n');
    file.Flush();
    Console.WriteLine($"Parsed \"{collection.title}\" data saved at \"{importer.GetOutputFilePath()}\".");
}
else if (config["Command"] == "CreateManifest")
{
    CollectionManifest manifest = new();

    string[] files = Directory.GetFiles(config["OutputPath"], "*.json");
    foreach (string filepath in files)
    {
        if (!filepath.EndsWith("manifest.json"))
        {
            string content = File.ReadAllText(filepath);
            CollectionV2 collection = JsonSerializer.Deserialize<CollectionV2>(content);
            manifest.files.Add(
                collection.identifier.ToString(),
                new CollectionManifestEntry()
                {
                    file = Path.GetFileName(filepath),
                    hash = CollectionManifest.CalculateHash(content),
                    sortOrder = collection.sortOrder
                }
            );
        }
    }

    var file = new FileStream(Path.Combine(config["OutputPath"], "manifest.json"), FileMode.Create);
    JsonSerializer.Serialize(file, manifest, new JsonSerializerOptions() { WriteIndented = true });
    file.WriteByte((byte) '\n');
    file.Flush();
    Console.WriteLine("Manifest created successfully.");
}
