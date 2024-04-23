using System.Text.Json;
using SOTM.InfraredEyepiece.Importers;
using Microsoft.Extensions.Configuration;
using SOTM.Shared.Models;
using System.Text;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

CollectionImporter importer;

bool createManifest = config.GetValue<bool>("CreateManifest", true);

if (config["Command"] == null)
{
    config["Command"] = config["ModPublishedFileId"] == null
        ? "ImportVanilla" 
        : "ImportMod";
}
if (config["Command"] == "ImportMod" && config["ModPublishedFileId"] == null)
{
    Console.WriteLine("If command is \"ImportMod\", \"ModPublishedFileId\" must not be null");
    System.Environment.Exit(1);
}
if (config["Command"] == "ImportVanilla")
{
    importer = new CoreGameImporter(config);
}
else
{
    importer = new ModImporter(config["ModPublishedFileId"], config);
}

var collection = importer.ParseResourcesV2();
var content = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(collection, new JsonSerializerOptions() { WriteIndented = true }) + '\n');

var output = File.Open(importer.GetOutputFilePath(), FileMode.Create);
output.Write(content);
output.Flush();
Console.WriteLine($"Parsed \"{collection.title}\" data saved at \"{importer.GetOutputFilePath()}\".");

if (createManifest)
{
    string manifestPath = Path.Combine(config["OutputPath"], "ModManifest.json");
    CollectionManifest manifest = new();
    try
    {
        var manifestFile = File.OpenRead(manifestPath);
        manifest = JsonSerializer.Deserialize<CollectionManifest>(manifestFile) ?? manifest;
        manifestFile.Dispose();
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine("ModManifest.json not found. Creating");
    }
    manifest.Add(
        collection.identifier.ToString(), 
        importer.GetOutputFileName(), 
        CollectionManifest.CalculateHash(content)
    );

    var manifestOutput = File.Open(manifestPath, FileMode.Create);
    JsonSerializer.Serialize(manifestOutput, manifest, new JsonSerializerOptions() { WriteIndented = true });
    manifestOutput.WriteByte((byte) '\n');
    manifestOutput.Flush();
}
