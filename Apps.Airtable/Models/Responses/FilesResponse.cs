using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Airtable.Models.Responses;

public class FilesResponse
{
    public IEnumerable<FileWrapper> Files { get; set; }
}

public class FileWrapper
{
    public File File { get; set; }
}