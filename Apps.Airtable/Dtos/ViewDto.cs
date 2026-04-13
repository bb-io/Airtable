namespace Apps.Airtable.Dtos;

public class ViewDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class ViewDtoWrapper<T> where T : ViewDto
{
    public IEnumerable<T> Views { get; set; }
}