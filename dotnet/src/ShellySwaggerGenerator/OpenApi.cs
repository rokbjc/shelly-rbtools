using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;

namespace RokBjc.ShellyRbTools.ShellySwaggerGenerator;

internal class OpenApi
{
    private static readonly Regex propertyRegex =
        new(
            "<tr>\\s*<td><code>(?'name'.*?)<\\/code><\\/td>\\s*<td>(?'type'.*?)<\\/td>\\s*<td>(?'description'.*?)<\\/td>\\s*<\\/tr>",
            RegexOptions.Singleline | RegexOptions.Compiled);


    public static OpenApiDocument CreateOpenApiDocument(string apiId, Dictionary<string, OpenApiPathItem> openApiPaths)
    {
        var document = new OpenApiDocument();
        document.Info = new OpenApiInfo
        {
            Version = "1.0.0",
            Title = "Shelly Gen1 REST API",
        };
        document.Servers = new List<OpenApiServer>
        {
//                      new OpenApiServer { Url = "http://192.168.33.1" }
        };

        document.Paths = new OpenApiPaths();

        foreach (var openApiPath in openApiPaths)
        {
            document.Paths.Add(openApiPath.Key, openApiPath.Value);

        }
        return document;
    }

    public static OpenApiSchema CreateResponseReference(string groupId, string endpointId, Match parentMatch, string data)
    {
        var result = new OpenApiSchema();
        result.Type = "object";
        result.Properties = new Dictionary<string, OpenApiSchema>();

        var rowsMatches = propertyRegex.Matches(data);

        foreach (Match match in rowsMatches)
        {
            var parameter = Parse(match);

            var propertySchema = new OpenApiSchema();
            propertySchema.Type = parameter.Schema.Type;
            propertySchema.Description = parameter.Description;
            propertySchema.Items = parameter.Schema.Items;
            if (result.Properties.ContainsKey(parameter.Name))
            {
                Console.WriteLine($"Duplicate property found in group {groupId}, endpoint {endpointId}");
                continue;
            }

            result.Properties.Add(parameter.Name, propertySchema);
        }

        return result;
    }

    public static OpenApiParameter Parse(Match match)
    {
        var name = match.Groups["name"].Value;
        Debug.Assert(!string.IsNullOrEmpty(name));

        var type = match.Groups["type"].Value;
        var description = match.Groups["description"].Value;

        var shellyType = Enum.Parse<PropertyType>(type.Replace(" ", ""), true);


        var oap = new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Description = description,
//            Required = true,
            Schema = new OpenApiSchema()
        };

        switch (shellyType)
        {
            case PropertyType.Hash:
            case PropertyType.String:
                oap.Schema.Type = "string";
                break;
            case PropertyType.Bool:
                oap.Schema.Type = "boolean";
                break;
            case PropertyType.Number:
            case PropertyType.Num:
                oap.Schema.Type = "number";
                break;
            case PropertyType.ArrayOfHashes:
            case PropertyType.ArrayOfStrings:
                oap.Schema.Type = "array";
                oap.Schema.Items = new OpenApiSchema() { Type = "string" };
                break;
            case PropertyType.Any:
                oap.Schema.Type = "object";
                break;
            case PropertyType.ArrayOfNumbers:
                oap.Schema.Type = "array";
                oap.Schema.Items = new OpenApiSchema() { Type = "number"};
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return oap;


    }

    public static IEnumerable<OpenApiParameter> Parse(string groupId, string endpoint, Match parentMatch, string data)
    {
        var result = new Dictionary<string, OpenApiParameter>();
        if (parentMatch.Success)
        {
            var rowsMatches = propertyRegex.Matches(data);

            foreach (Match match in rowsMatches)
            {
                var p = Parse(match);
                if (result.ContainsKey(p.Name))
                {
                    Console.WriteLine($"Duplicate property found in group {groupId}, endpoint {endpoint}");
                    continue;
                }
                result.Add(p.Name, p);
            }
        }

        return result.Values;
    }

}