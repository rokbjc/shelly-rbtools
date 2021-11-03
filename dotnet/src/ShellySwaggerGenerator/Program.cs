using System.Runtime.CompilerServices;
using RokBjc.ShellyRbTools.ShellySwaggerGenerator;


var fullDocsContent = File.ReadAllText(Files.GetShellyDocsGen1FileInfo().FullName);

Console.WriteLine($"Documentation file contains {fullDocsContent.Length / 1024:0.0} KiB");
ShellyGen1DocsProcessor.ProcessAllContentToFile(fullDocsContent);

Console.WriteLine("done!");