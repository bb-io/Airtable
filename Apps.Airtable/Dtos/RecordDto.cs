using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Dtos;

public class RecordDto
{
    [Display("Created time")]
    public DateTime CreatedTime { get; set; }

    [Display("Record ID")]
    public string Id { get; set; }
}