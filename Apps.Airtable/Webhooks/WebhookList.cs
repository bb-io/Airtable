using System.Net;
using Apps.Airtable.Dtos;
using Apps.Airtable.Models.Identifiers;
using Apps.Airtable.UrlBuilders;
using Apps.Airtable.Webhooks.Handlers;
using Apps.Airtable.Webhooks.Payload;
using Apps.Airtable.Webhooks.Payload.Records;
using Apps.Airtable.Webhooks.Payload.Tables;
using Apps.Airtable.Webhooks.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Airtable.Webhooks;

[WebhookList]
public class WebhookList : BaseInvocable
{
    private static readonly object LockObject = new();
    
    private readonly IEnumerable<AuthenticationCredentialsProvider> _credentials;
    private readonly JsonSerializerSettings _jsonSerializerSettings = 
        new() { MissingMemberHandling = MissingMemberHandling.Ignore };

    public WebhookList(InvocationContext invocationContext) : base(invocationContext)
    {
        _credentials = invocationContext.AuthenticationCredentialsProviders;
    }
    
    [Webhook("On records added", typeof(RecordAddedWebhookHandler),
        Description = "This webhook is triggered when new records are added.")]
    public async Task<WebhookResponse<RecordsResponse>> OnRecordsAdded(WebhookRequest request,
        [WebhookParameter] TableIdentifier table)
    {
        var (changedTables, webhookId) = await ProcessWebhookRequest<ChangedTablesPayload>(request);

        if (!changedTables.Payloads.Any(p => p.ChangedTablesById != null && p.ChangedTablesById.ContainsKey(table.TableId)))
            return new WebhookResponse<RecordsResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };

        var records = changedTables.Payloads
            .Select(payload => payload.ChangedTablesById)
            .Where(changedTable => changedTable != null && changedTable.ContainsKey(table.TableId))
            .Select(changedTable => JsonConvert.DeserializeObject<CreatedRecordsPayload>
                (changedTable[table.TableId].ToString(), _jsonSerializerSettings))
            .ToList();
        
        var resultRecords = new List<RecordDto>();
        
        foreach (var recordData in records.Select(record => record.CreatedRecordsById))
        {
            foreach (var recordId in recordData.Keys)
            {
                resultRecords.Add(new RecordDto
                {
                    Id = recordId,
                    CreatedTime = recordData[recordId].CreatedTime
                });
            }
        }

        StoreCursor(changedTables.Cursor.ToString(), webhookId);

        return new WebhookResponse<RecordsResponse>
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = new RecordsResponse
            {
                TableId = table.TableId,
                Records = resultRecords
            }
        };
    }

    private async Task<(T, string)> ProcessWebhookRequest<T>(WebhookRequest request) where T : BaseTablePayload
    {
        var payload = JsonConvert.DeserializeObject<WebhookPayload>(request.Body.ToString(), _jsonSerializerSettings);
        var webhookId = payload.WebhookId; 
        var client = new AirtableClient(_credentials, new AirtableWebhookUrlBuilder());
        var getDataRequest = new AirtableRequest($"/{webhookId}/payloads?cursor={payload.Cursor}", Method.Get, _credentials);
        var getDataResponse = await client.ExecuteWithErrorHandling<T>(getDataRequest);
        return (getDataResponse, webhookId);
    }

    private void StoreCursor(string cursor, string webhookId)
    {
        var bridgeService = new BridgeService(InvocationContext);

        lock (LockObject)
        {
            var storedCursor = bridgeService.RetrieveValue(webhookId).Result;
            
            if (int.Parse(storedCursor ?? "0") < int.Parse(cursor))
                bridgeService.StoreValue(webhookId, cursor).Wait();
        }
        
    } 
}