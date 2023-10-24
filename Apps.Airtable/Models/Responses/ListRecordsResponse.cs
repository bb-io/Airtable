using Apps.Airtable.Dtos;

namespace Apps.Airtable.Models.Responses;

public class ListRecordsResponse
{
    public IEnumerable<RecordDto> Records { get; set; }
}