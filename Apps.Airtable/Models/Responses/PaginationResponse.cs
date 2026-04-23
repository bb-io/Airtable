namespace Apps.Airtable.Models.Responses;

public class PaginationResponse<T>
{
    public string? Offset { get; set; }
    
    public virtual IEnumerable<T> Items { get; set; }
}
