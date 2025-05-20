namespace Apps.Airtable.Webhooks.Payload.Records
{
    public class ChangedRecordData
    {
        public RecordPayload Current { get; set; }
        public RecordPayload Previous { get; set; }
    }

    public class RecordPayload
    {
        public Dictionary<string, object> CellValuesByFieldId { get; set; }
    }
}
