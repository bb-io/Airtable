namespace Apps.Airtable.Dtos;

public class WebhookDto
{
    public string Id { get; set; }
    public string NotificationUrl { get; set; }
    public WebhookSpecification Specification { get; set; }
}

public class WebhookDtoWrapper
{
    public IEnumerable<WebhookDto> Webhooks { get; set; }
}

public class WebhookSpecification
{
    public WebhookSpecificationOptions Options { get; set; }
}

public class WebhookSpecificationOptions
{
    public WebhookSpecificationOptionsFilters Filters { get; set; }
}

public class WebhookSpecificationOptionsFilters
{
    public IEnumerable<string> DataTypes { get; set; }
    public IEnumerable<string> ChangeTypes { get; set; }
}