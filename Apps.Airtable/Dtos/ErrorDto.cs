namespace Apps.Airtable.Dtos;

public class ErrorDto
{
    public string Type { get; set; }
    public string Message { get; set; }
}

public class ErrorDtoWrapper
{
    public ErrorDto Error { get; set; }
}