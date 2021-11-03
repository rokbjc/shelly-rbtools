namespace RokBjc.ShellyRbTools.ShellySwaggerGenerator;

internal static class Files
{
    public static FileInfo EnsureEmptySwaggerFileInfo(string fileId)
    {
        var result =  new FileInfo(Path.Combine("..", "..", "..", "..", "..", "..", "swagger", $"swagger-{fileId}.yaml"));
        if (result.Exists)
        {
            result.Delete();
            Thread.Sleep(200);
        }

        return result;
    }

    public static FileInfo EnsureEmptyGroupFileInfo(string groupId)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..",".temp", "groups", $"{groupId}.html");
        var fi = new FileInfo(filePath);
        if (!fi.Directory.Parent.Exists) fi.Directory.Parent.Create();
        if (!fi.Directory.Exists) fi.Directory.Create();
        if (fi.Exists) fi.Delete();
        return fi;

    }

    public static FileInfo EnsureEndpointHtmlFileInfo(string groupId, string endpointId)
    {
        var fileHtmlPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", ".temp", "endpoints", $"{groupId}.{endpointId}.html");
        var fi = new FileInfo(fileHtmlPath);
        if (!fi.Directory.Parent.Exists) fi.Directory.Parent.Create();
        if (!fi.Directory.Exists) fi.Directory.Create();
        if (fi.Exists)
        {
            fi.Delete();
            Thread.Sleep(100);
        }

        return fi;
    }
    public static FileInfo EnsureEndpointJsonFileInfo(string groupId, string endpointId)
    {
        var fileJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", ".temp", "endpoints", $"{groupId}.{endpointId}.json");
        var fi = new FileInfo(fileJsonPath);
        if (!fi.Directory.Parent.Exists) fi.Directory.Parent.Create();
        if (!fi.Directory.Exists) fi.Directory.Create();
        if (fi.Exists) fi.Delete();
        return fi;
    }

    public static FileInfo GetShellyDocsGen1FileInfo()
    {
        var result =  new FileInfo(Path.Combine("..", "..", "..", "..", "..", "..", "shelly-api-docs.shelly.cloud", "gen1.html"));
        return result;
    }

}
