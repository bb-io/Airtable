namespace Apps.Airtable.Webhooks.Payload.Records;

public class CreatedDataPayload
{
    public Dictionary<string, CreatedRecordData> CreatedRecordsById { get; set; }
    public Dictionary<string, ChangedRecordData> ChangedRecordsById { get; set; }
    public List<string> DestroyedRecordIds { get; set; }


    public Dictionary<string, object> CreatedFieldsById { get; set; }
    public Dictionary<string, object> ChangedFieldsById { get; set; }
    public List<string> DestroyedFieldsIds { get; set; }
}