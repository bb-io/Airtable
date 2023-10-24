using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Dtos;

public class RecordDto
{
    [Display("Created date and time")]
    public DateTime CreatedTime { get; set; }

    [Display("Record ID")]
    public string Id { get; set; }
}

public class FullRecordDto : RecordDto
{
    public Dictionary<string, object> Fields { get; set; }
}