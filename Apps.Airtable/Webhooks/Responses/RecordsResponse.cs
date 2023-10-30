using Apps.Airtable.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Webhooks.Responses;

public class RecordsResponse
{
    [Display("Table")]
    public string TableId { get; set; }
    
    public IEnumerable<RecordDto> Records { get; set; }
}