using System.Text.Json;
using SOTM.InfraredEyepiece.Importers;
using Microsoft.Extensions.Configuration;
using SOTM.Shared.Models;
using System.Text;
using ICSharpCode.Decompiler.CSharp.Syntax;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();


CollectionImporter importer;
if (config["Command"] == "ImportVanilla")
{
    importer = new CoreGameImporter(config);
}
else if (config["Command"] == "ImportMod")
{
    importer = new ModImporter(config["ModPublishedFileId"], config);
}
else
{
    importer = null;
    Console.WriteLine("Command must either be \"ImportVanilla\" or \"ImportMod\"");
    System.Environment.Exit(1);
}

var collection = importer.ParseResourcesV2();
var content = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(collection, new JsonSerializerOptions() { WriteIndented = true }) + '\n');

var output = File.Open(importer.GetOutputFilePath(), FileMode.Create);
output.Write(content);
output.Flush();
Console.WriteLine($"Parsed \"{collection.title}\" data saved at \"{importer.GetOutputFilePath()}\".");


string manifestPath = Path.Combine(config["OutputPath"], "manifest.json");
CollectionManifest manifest = new();
try
{
    var manifestFile = File.OpenRead(manifestPath);
    manifest = JsonSerializer.Deserialize<CollectionManifest>(manifestFile) ?? manifest;
    manifestFile.Dispose();
}
catch (FileNotFoundException)
{
    Console.WriteLine("manifest.json not found. Creating");
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
