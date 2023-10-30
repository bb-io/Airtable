using Apps.Airtable.Dtos;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using RestSharp;

namespace Apps.Airtable.Webhooks.Handlers;

public class BaseWebhookHandler : BaseInvocable, IWebhookEventHandler, IAsyncRenewableWebhookEventHandler
{
    private readonly string _subscriptionEvent;
    private readonly string _dataType;
    private readonly AirtableClient _client;

    protected BaseWebhookHandler(InvocationContext invocationContext, string subscriptionEvent, string dataType) 
        : base(invocationContext)
    {
        _subscriptionEvent = subscriptionEvent;
        _dataType = dataType;
        _client = new AirtableClient(invocationContext.AuthenticationCredentialsProviders, new AirtableWebhookUrlBuilder());
    }

    public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var request = new AirtableRequest("", Method.Post, authenticationCredentialsProviders);
        request.AddJsonBody(new
        {
            notificationUrl = values["payloadUrl"],
            specification = new
            {
                options = new
                {
                    filters = new
                    {
                        dataTypes = new[] { _dataType },
                        changeTypes = new[] { _subscriptionEvent }
                    }
                }
            }
        });

        await _client.ExecuteWithErrorHandling(request);
    }

    [Period(10000)]
    public async Task RenewSubscription(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var getWebhooksRequest = new AirtableRequest("", Method.Get, authenticationCredentialsProviders);
        var webhooks = await _client.ExecuteWithErrorHandling<WebhookDtoWrapper>(getWebhooksRequest);
        var webhook = webhooks.Webhooks.First(webhook => webhook.NotificationUrl == values["payloadUrl"]);
        
        var refreshWebhookRequest = new AirtableRequest($"/{webhook.Id}/refresh", Method.Post, 
            authenticationCredentialsProviders);
        await _client.ExecuteWithErrorHandling(refreshWebhookRequest);
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var getWebhooksRequest = new AirtableRequest("", Method.Get, authenticationCredentialsProviders);
        var webhooks = await _client.ExecuteWithErrorHandling<WebhookDtoWrapper>(getWebhooksRequest);
        var webhook = webhooks.Webhooks.First(webhook => webhook.NotificationUrl == values["payloadUrl"]);
        
        var deleteWebhookRequest = new AirtableRequest($"/{webhook.Id}", Method.Delete, authenticationCredentialsProviders);
        await _client.ExecuteWithErrorHandling(deleteWebhookRequest);

        var bridgeService = new BridgeService();
        await bridgeService.DeleteValue(webhook.Id);
    }
}