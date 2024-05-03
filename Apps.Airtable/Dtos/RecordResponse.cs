namespace Apps.Airtable.Dtos;

public class RecordResponse
{
    public string Id { get; set; }

    public DateTime CreatedTime { get; set; }

    public Dictionary<string, object> Fields { get; set; }

}