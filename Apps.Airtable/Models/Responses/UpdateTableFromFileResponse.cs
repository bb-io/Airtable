using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Models.Responses;

public class UpdateTableFromFileResponse
{
    [Display("Updated records")]
    public int UpdatedRecords { get; set; }

    [Display("Skipped rows")]
    public int SkippedRows { get; set; }

    [Display("Ignored columns")]
    public IEnumerable<string> IgnoredColumns { get; set; }
}
