using Apps.Airtable.Dtos;
using Newtonsoft.Json;

namespace Apps.Airtable.Models.Responses.Records;

public class RecordsPaginationResponse : PaginationResponse<RecordResponse>
{
    [JsonProperty("records")]
    public override IEnumerable<RecordResponse> Items { get; set; }
}