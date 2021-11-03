namespace ShellyApiGen1ToSwagger;

internal class EndpointDescription
{
    public string Endpoint { get; set; }
    public PropertyDescription[] RequestArguments { get; set; }
    public PropertyDescription[] ResponseProperites { get; set; }
    public string GroupId { get; set; }
    public string Id { get; set; }
    public string Description { get; set; }
}