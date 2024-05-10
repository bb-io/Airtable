using Apps.Airtable.Models.Entities;

namespace Apps.Airtable.Models.Responses.Records;

public class ListRecordsResponse
{
    public IEnumerable<RecordEntity> Records { get; set; }
}