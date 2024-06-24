using Apps.Airtable.Dtos;
using Apps.Airtable.Webhooks.Payload.Records;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Webhooks.Responses;

public class ChangedDataResponse
{
    [Display("Table")]
    public string TableId { get; set; }
    
    public List<string> ChangedRecords { get; set; }
    public List<string> ChangedFields { get; set; }
    public CurrentMetadata ChangedMetadata { get; set; }
}