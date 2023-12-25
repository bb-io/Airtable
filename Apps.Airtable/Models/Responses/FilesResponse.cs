
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Airtable.Models.Responses;

public class FilesResponse
{
    public IEnumerable<FileWrapper> Files { get; set; }
}

public class FileWrapper
{
    public FileReference File { get; set; }
}