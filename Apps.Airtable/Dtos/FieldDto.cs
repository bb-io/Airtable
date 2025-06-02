namespace Apps.Airtable.Dtos;

public class FieldDto
{
    public string Type { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public OptionsDto? Options { get; set; }
}

public class OptionsDto
{
    public IEnumerable<ChoiceDto> Choices { get; set; }
}

public class ChoiceDto
{
    public string Id { get; set; }
    public string? Color { get; set; }
    public string Name { get; set; }
}
