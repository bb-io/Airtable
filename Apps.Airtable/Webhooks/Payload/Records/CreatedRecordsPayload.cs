namespace Apps.Airtable.Webhooks.Payload.Records;

public class CreatedRecordsPayload
{
    public Dictionary<string, CreatedRecordData> CreatedRecordsById { get; set; }
    public Dictionary<string, ChangedRecordData> ChangedRecordsById { get; set; }
    public List<string> DestroyedRecordIds { get; set; }
}