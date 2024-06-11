using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Dtos;

public class RecordResponse
{
    [Display("Record ID")]
    public string Id { get; set; }

    [Display("Created time")]
    public DateTime CreatedTime { get; set; }

    public Dictionary<string, object> Fields { get; set; }
}