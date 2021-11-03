using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace ShellyApiGen1ToSwagger
{
    internal class PropertyDescription
    {
        private static readonly Regex propertyRegex =
            new Regex(
                "<tr>\\s*<td><code>(?'name'.*?)<\\/code><\\/td>\\s*<td>(?'type'.*?)<\\/td>\\s*<td>(?'description'.*?)<\\/td>\\s*<\\/tr>",
                RegexOptions.Singleline | RegexOptions.Compiled);

        public string Name { get; set; }
        public PropertyType Type { get; set; }
        public string Description { get; set; }


      
        public static IEnumerable<OpenApiParameter> ParseOpenApiParameter(Match parentMatch, string data)
        {
            if (parentMatch.Success)
            {
                var rowsMatches = propertyRegex.Matches(data);

                foreach (Match match in rowsMatches)
                {
                    yield return Parse(match);
                }
            }
        }


        public string GetOpenApiType()
        {
            return Type.ToString();
        }

        public override string ToString()
        {
            return $"{Name}:{Type} ({Description})";
        }
    }
    
}
