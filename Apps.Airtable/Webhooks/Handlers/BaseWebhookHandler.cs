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
    private readonly string _bridgePayloadUrl;

    protected BaseWebhookHandler(InvocationContext invocationContext, string subscriptionEvent, string dataType) 
        : base(invocationContext)
    {
        _subscriptionEvent = subscriptionEvent;
        _dataType = dataType;
        _client = new AirtableClient(invocationContext.AuthenticationCredentialsProviders, new AirtableWebhookUrlBuilder());
        _bridgePayloadUrl = $"{invocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/webhooks/{ApplicationConstants.AppName}";
    }

    public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var targetWebhook = await GetTargetWebhook(authenticationCredentialsProviders);
        var bridgeService = new BridgeService(InvocationContext);
        string webhookId;

        if (targetWebhook == null)
        {
            var request = new AirtableRequest("", Method.Post, authenticationCredentialsProviders);
            request.AddJsonBody(new
            {
                notificationUrl = _bridgePayloadUrl,
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

            var createdWebhook = await _client.ExecuteWithErrorHandling<CreatedWebhookDto>(request);
            webhookId = createdWebhook.Id;

            await bridgeService.StoreValue(webhookId, "1");
        }
        else
            webhookId = targetWebhook.Id;
        
        await bridgeService.Subscribe(values["payloadUrl"], webhookId, _subscriptionEvent);
    }

    [Period(10000)]
    public async Task RenewSubscription(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var targetWebhook = await GetTargetWebhook(authenticationCredentialsProviders);
        var refreshWebhookRequest = new AirtableRequest($"/{targetWebhook.Id}/refresh", Method.Post, 
            authenticationCredentialsProviders);
        await _client.ExecuteWithErrorHandling(refreshWebhookRequest);
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var targetWebhook = await GetTargetWebhook(authenticationCredentialsProviders);
        var webhookId = targetWebhook.Id;
        
        var bridgeService = new BridgeService(InvocationContext);
        var webhooksLeft = await bridgeService.Unsubscribe(values["payloadUrl"], webhookId, _subscriptionEvent);

        if (webhooksLeft == 0)
        {
            await bridgeService.DeleteValue(webhookId);
            var deleteWebhookRequest = new AirtableRequest($"/{webhookId}", Method.Delete, authenticationCredentialsProviders);
            await _client.ExecuteWithErrorHandling(deleteWebhookRequest);
        }
    }

    private async Task<WebhookDto?> GetTargetWebhook(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var getWebhooksRequest = new AirtableRequest("", Method.Get, authenticationCredentialsProviders);
        var webhooks = await _client.ExecuteWithErrorHandling<WebhookDtoWrapper>(getWebhooksRequest);
        var webhook = webhooks.Webhooks.FirstOrDefault(webhook => webhook.NotificationUrl == _bridgePayloadUrl 
                                                                  && webhook.Specification.Options.Filters.ChangeTypes.Contains(_dataType) 
                                                                  && webhook.Specification.Options.Filters.DataTypes.Contains(_dataType));
        return webhook;
    }
}