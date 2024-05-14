namespace Apps.Airtable.Dtos;

public class TableDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string PrimaryFieldId { get; set; }
}

public class FullTableDto : TableDto
{   
    public IEnumerable<FieldDto> Fields { get; set; }
}

public class TableDtoWrapper<T> where T : TableDto
{
    public IEnumerable<T> Tables { get; set; }
}