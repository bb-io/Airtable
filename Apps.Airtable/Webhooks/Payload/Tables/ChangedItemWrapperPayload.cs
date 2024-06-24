namespace Apps.Airtable.Webhooks.Payload.Tables;

public class ChangedItemWrapperPayload : BaseTablePayload
{
    public IEnumerable<ChangedTablesWrapper> Payloads { get; set; }
}

public class ChangedTablesWrapper
{
    public Dictionary<string, object>? ChangedTablesById { get; set; }
}