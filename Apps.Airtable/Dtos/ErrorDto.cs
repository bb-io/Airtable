namespace Apps.Airtable.Dtos;

public class ErrorObjectDto
{
    public string Type { get; set; }
    public string Message { get; set; }
}

public class ErrorStringDto
{
    public string Error { get; set; }
}

public class ErrorObjectDtoWrapper
{
    public ErrorObjectDto Error { get; set; }
}