namespace Apps.Airtable.Webhooks.Payload.Records;

public class CreatedRecordsPayload
{
    public Dictionary<string, RecordData> CreatedRecordsById { get; set; }
}