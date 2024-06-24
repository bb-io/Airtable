using Apps.Airtable.Dtos;
using Apps.Airtable.Webhooks.Payload.Records;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Webhooks.Responses;

public class ChangedDataResponse
{
    [Display("Table")]
    public string TableId { get; set; }

    [Display("Changed records", Description = "IDs of added/updated/removed records")]
    public List<string> ChangedRecords { get; set; }

    [Display("Changed fields", Description = "IDs of added/updated/removed fields")]
    public List<string> ChangedFields { get; set; }

    [Display("Changed metadata")]
    public CurrentMetadata ChangedMetadata { get; set; }
}