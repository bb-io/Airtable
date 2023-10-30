namespace Apps.Airtable.Webhooks.Payload;

public class WebhookPayload
{
    public WebhookIdWrapper Webhook { get; set; }
}

public class WebhookIdWrapper
{
    public string Id { get; set; }
}

