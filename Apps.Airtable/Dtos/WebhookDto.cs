namespace Apps.Airtable.Dtos;

public class WebhookDto
{
    public string Id { get; set; }
    public string NotificationUrl { get; set; }
}

public class WebhookDtoWrapper
{
    public IEnumerable<WebhookDto> Webhooks { get; set; }
}