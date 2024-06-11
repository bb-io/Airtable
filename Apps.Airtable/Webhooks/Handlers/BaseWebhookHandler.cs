using Apps.Airtable.Dtos;
using Apps.Airtable.Models.Requests;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Airtable.Webhooks.Handlers;

public class BaseWebhookHandler : BaseInvocable, IWebhookEventHandler, IAsyncRenewableWebhookEventHandler
{
    private readonly WebhookConfigRequest _webhookConfig;
    private readonly AirtableClient _client;
    private readonly string _bridgePayloadUrl;

    public BaseWebhookHandler(InvocationContext invocationContext, [WebhookParameter(true)] WebhookConfigRequest webhookConfigRequest) 
        : base(invocationContext)
    {
        _webhookConfig = webhookConfigRequest;
        _client = new(invocationContext.AuthenticationCredentialsProviders, new AirtableWebhookUrlBuilder());
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

            var serializedPayload = JsonConvert.SerializeObject(new
            {
                notificationUrl = _bridgePayloadUrl,
                specification = new
                {
                    options = new
                    {
                        filters = new
                        {
                            dataTypes = _webhookConfig.DataTypes,
                            changeTypes = _webhookConfig.ChangeTypes,
                            fromSources = _webhookConfig.FromSources
                        }
                    }
                }
            },
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            request.AddStringBody(serializedPayload, DataFormat.Json);

            var createdWebhook = await _client.ExecuteWithErrorHandling<CreatedWebhookDto>(request);
            webhookId = createdWebhook.Id;

            await bridgeService.StoreValue(webhookId, "1");
        }
        else
            webhookId = targetWebhook.Id;
        foreach(var changeType in _webhookConfig.ChangeTypes)
            await bridgeService.Subscribe(values["payloadUrl"], webhookId, changeType);
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

        int webhooksLeft = 0;
        foreach (var changeType in _webhookConfig.ChangeTypes)
            webhooksLeft += await bridgeService.Unsubscribe(values["payloadUrl"], webhookId, changeType);

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
                                                                  && webhook.Specification.Options.Filters.ChangeTypes.Any(x => _webhookConfig.ChangeTypes.Contains(x)) 
                                                                  && webhook.Specification.Options.Filters.DataTypes.Any(x => _webhookConfig.DataTypes.Contains(x)));
        return webhook;
    }
}