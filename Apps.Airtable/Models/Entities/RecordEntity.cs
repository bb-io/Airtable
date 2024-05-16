using Apps.Airtable.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Models.Entities;

public class RecordEntity
{
    [Display("Record ID")] public string RecordId { get; set; }

    [Display("Created date and time")] public DateTime CreatedTime { get; set; }
    
    public IEnumerable<FieldEntity>? Fields { get; set; }

    public RecordEntity(RecordResponse record)
    {
        RecordId = record.Id;
        CreatedTime = record.CreatedTime;
        Fields = record.Fields?.Select(x => new FieldEntity()
        {
            Id = x.Key,
            Value = x.Value?.ToString() ?? string.Empty
        });
    }
}