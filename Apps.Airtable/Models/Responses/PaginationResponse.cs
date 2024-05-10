namespace Apps.Airtable.Models.Responses;

public class PaginationResponse<T>
{
    public int? Offset { get; set; }    
    
    public virtual IEnumerable<T> Items { get; set; }
}