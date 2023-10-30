using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Airtable.Webhooks.Handlers;

public class RecordAddedWebhookHandler : BaseWebhookHandler
{
    private const string SubscriptionEvent = "add";
    private const string DataType = "tableData";

    public RecordAddedWebhookHandler(InvocationContext invocationContext) 
        : base(invocationContext, SubscriptionEvent, DataType) { }
}