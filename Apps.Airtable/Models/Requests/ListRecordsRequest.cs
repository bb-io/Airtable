using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Models.Requests;

public class ListRecordsRequest
{
    [Display("Base ID")] 
    public string BaseId { get; set; }

    [Display("Table ID (or name)")] 
    public string TableId { get; set; }
}
