using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace RokBjc.ShellyRbTools.ShellySwaggerGenerator;

internal static class ShellyGen1DocsProcessor
{
    private static Regex GroupSplitRegex { get; } = new("<h1 id=\"(?'id'.*?)\">(.*?)<\\/h1>.*?", RegexOptions.Singleline | RegexOptions.Compiled);
    private static Regex EndpointSplitRegex { get; } = new("<h2 id=\"(?'id'.{1,40})\">.*?<code>(\\/.+?)<\\/code>\\s*<\\/h2>.*?<p>(.*?)<\\/p>", RegexOptions.Singleline | RegexOptions.Compiled);
    private static Regex EndpointInfoRegex { get; } = new("<h2 id=\"(?'id'.{1,40})\">.*?<code>(?'endpoint'\\/.+?)<\\/code>.*?<p>(?'description'.*?)<\\/p>", RegexOptions.Singleline | RegexOptions.Compiled);
    private static Regex AttributesRegex { get; } = new("<h3 id=\"attributes.*?\">Attributes<\\/h3>.*?<tbody>(?'rows'.*?)<\\/tbody>", RegexOptions.Singleline | RegexOptions.Compiled);
    private static Regex ParametersRegex { get; } = new("<h3 id=\"parameters.*?\">Parameters<\\/h3>.*?<tbody>(?'rows'.*?)<\\/tbody>", RegexOptions.Singleline | RegexOptions.Compiled);

    private const string CommonApiId = "common-http-api";
    private static readonly ISet<string> InvalidApiIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "swagger-shelly-family-overview" };

    public static void ProcessAllContentToFile(string fullDocsContent)
    {

        var groupMatches = GroupSplitRegex.Matches(fullDocsContent);

        Console.WriteLine($"{groupMatches.Count} groups found");
        int? previousGroupStartIndex = null;
        string? previousId = null;
        var allApiEndpoints = new Dictionary<string, Dictionary<string, OpenApiPathItem>>();
        foreach (Match match in groupMatches)
        {
            var index = match.Index;
            if (previousId != null && !InvalidApiIds.Contains(previousId))
            {
                var apiEndpoints = new Dictionary<string, OpenApiPathItem>();
                ProcessGroupContentToFile(apiEndpoints, fullDocsContent, previousId, previousGroupStartIndex.Value, index);
                allApiEndpoints.Add(previousId, apiEndpoints);
            }

            previousGroupStartIndex = index;
            previousId = match.Groups["id"].Value;
        }
        var apiEndpoints2 = new Dictionary<string, OpenApiPathItem>();
        ProcessGroupContentToFile(apiEndpoints2, fullDocsContent, previousId, previousGroupStartIndex.Value, fullDocsContent.Length - 1);
        allApiEndpoints.Add(previousId, apiEndpoints2);


        var commonApiEndpoints = allApiEndpoints[CommonApiId];

        foreach (var apiEndpoints in allApiEndpoints)
        {
            if (apiEndpoints.Key.Equals(CommonApiId)) continue;
            var jointEndpoints = new Dictionary<string, OpenApiPathItem>(commonApiEndpoints);

            foreach (var apiEndpoint in apiEndpoints.Value)
            {
                if (jointEndpoints.ContainsKey(apiEndpoint.Key)) jointEndpoints.Remove(apiEndpoint.Key);
                jointEndpoints.Add(apiEndpoint.Key, apiEndpoint.Value);
            }

            var openApiDoc = OpenApi.CreateOpenApiDocument(apiEndpoints.Key, jointEndpoints);

            var openApiFi = Files.EnsureEmptySwaggerFileInfo(apiEndpoints.Key);

            using var streamWriter = openApiFi.CreateText();
            var writer = new OpenApiYamlWriter(streamWriter);
            openApiDoc.SerializeAsV3(writer);
        }
    }

    private static void ProcessGroupContentToFile(Dictionary<string, OpenApiPathItem> allEndpoints, string docContent, string groupId, int startIndex, int endIndex)
    {
        var content = docContent[startIndex..(endIndex - 1)];
        var fi = Files.EnsureEmptyGroupFileInfo(groupId);
        File.WriteAllText(fi.FullName, content);

        var endpointMatches = EndpointSplitRegex.Matches(content);
        Console.WriteLine($"{endpointMatches.Count} groups found in group {groupId}");

        int? previousEndpointStartIndex = null;
        string? previousId = null;
        foreach (Match match in endpointMatches)
        {
            var index = match.Index;
            if (previousId != null)
            {
                var i = ProcessEndpointContentToFile(content, groupId, previousId, previousEndpointStartIndex.Value, index);
                var key = $"{i.Path}";
                if (allEndpoints.ContainsKey(key)) allEndpoints.Remove(key);
                allEndpoints.Add(key, i.Definition);
            }

            previousEndpointStartIndex = index;
            previousId = match.Groups["id"].Value;
        }

        if (previousId != null)
        {
            var j = ProcessEndpointContentToFile(content,
                groupId,
                previousId,
                previousEndpointStartIndex.Value,
                content.Length - 1);
            var key = $"{j.Path}";
            if (allEndpoints.ContainsKey(key)) allEndpoints.Remove(key);
            allEndpoints.Add(key, j.Definition);
        }


    }

    private static (string Path, OpenApiPathItem Definition) ProcessEndpointContentToFile(string docContent, string groupId, string endpointId, int startIndex, int endIndex)
    {
        var fi = Files.EnsureEndpointHtmlFileInfo(groupId, endpointId);
        var content = docContent[startIndex..(endIndex + 1)];

        File.WriteAllText(fi.FullName, content);

        var endpointInfoMatch = EndpointInfoRegex.Match(content);
        var result = new OpenApiPathItem();
        var id = endpointInfoMatch.Groups["id"].Value;
        var endpoint = endpointInfoMatch.Groups["endpoint"].Value;
        Debug.Assert(!string.IsNullOrEmpty(endpoint));
        result.Description = endpointInfoMatch.Groups["description"].Value;


        var parametersMatch = ParametersRegex.Match(content);
        if (parametersMatch.Success)
        {
            var parametersData = parametersMatch.Groups["rows"].Value;
            result.Parameters = OpenApi.Parse(groupId, endpointId, parametersMatch, parametersData).ToList();
        }
        if (endpoint.Contains("{index}"))
        {
            result.Parameters.Add(new OpenApiParameter
            {
                Name = "index",
                In = ParameterLocation.Path,
                Required = true,
                Schema = new OpenApiSchema()
                {
                    Type = "integer",
                }
            });

        }

        var attributesMatch = AttributesRegex.Match(content);
        if (attributesMatch.Success)
        {
            var attributesData = parametersMatch.Groups["rows"].Value;
            result.Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = new()
                {
                    OperationId = id,
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new()
                        {
                            Description = "OK",
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                ["application/json"] = new()
                                {
                                    Schema = OpenApi.CreateResponseReference(groupId, endpoint, attributesMatch, attributesData)
                                }
                            }
                        }
                    },
                    Tags = new List<OpenApiTag>() { new() { Name = groupId } }
                }
            };
        }
        else
        {
            result.Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = new()
                {
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new()
                        {
                            Description = "OK",
                        }
                    },
                    Tags = new List<OpenApiTag>() { new() { Name = groupId } }
                }
            };
        }

        fi = Files.EnsureEndpointJsonFileInfo(groupId, endpointId);
        File.WriteAllText(fi.FullName, System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } }));
        return (endpoint, result);
    }
}