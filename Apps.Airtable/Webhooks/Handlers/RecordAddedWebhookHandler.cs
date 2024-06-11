using Apps.Airtable.Models.Requests;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Airtable.Webhooks.Handlers;

public class RecordAddedWebhookHandler : BaseWebhookHandler
{
    private const string SubscriptionEvent = "add";
    private const string DataType = "tableData";

    public RecordAddedWebhookHandler(InvocationContext invocationContext, [WebhookParameter] WebhookConfigRequest webhookConfigRequest)  
        : base(invocationContext, webhookConfigRequest) { }
}