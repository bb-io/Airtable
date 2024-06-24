namespace Apps.Airtable.Webhooks.Payload.Records
{
    public class ChangedMetadata
    {
        public CurrentMetadata Current { get; set; }
    }

    public class CurrentMetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}