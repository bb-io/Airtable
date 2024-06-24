using System.Collections.Generic;
using System.Net;
using Apps.Airtable.Dtos;
using Apps.Airtable.Models.Identifiers;
using Apps.Airtable.Models.Requests;
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
    
    [Webhook("On data changed", typeof(BaseWebhookHandler),
        Description = "This webhook is triggered when data is changed")]
    public async Task<WebhookResponse<ChangedDataResponse>> OnRecordsAdded(
        WebhookRequest request, 
        [WebhookParameter(true)] WebhookConfigRequest webhookConfigRequest,
        [WebhookParameter] TableIdentifier table)
    {
        var (changedTables, webhookId) = await ProcessWebhookRequest<ChangedItemWrapperPayload>(request);

        if (!changedTables.Payloads.Any(p => p.ChangedTablesById != null && p.ChangedTablesById.ContainsKey(table.TableId)))
            return new()
            {
                HttpResponseMessage = new(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };

        var records = changedTables.Payloads
            .Select(payload => payload.ChangedTablesById)
            .Where(changedTable => changedTable != null && changedTable.ContainsKey(table.TableId))
            .Select(changedTable => JsonConvert.DeserializeObject<ChangedDataPayload>
                (changedTable[table.TableId].ToString(), _jsonSerializerSettings))
            .ToList();
        
        var resultRecords = new List<string>();
        var resultFields = new List<string>();
        CurrentMetadata changedMetadata = null;

        if (webhookConfigRequest.DataType == "tableData")
        {
            var createdRecord = records.Select(record => record.CreatedRecordsById ?? new()).SelectMany(x => x.Keys.Select(k => new RecordResponse() { Id = k }));
            var changedRecords = records.Select(record => record.ChangedRecordsById ?? new()).SelectMany(x => x.Keys.Select(k => new RecordResponse() { Id = k }));
            var removedRecords = records.SelectMany(record => record?.DestroyedRecordIds?.Select(k => new RecordResponse() { Id = k }) ?? new List<RecordResponse>());

            if(webhookConfigRequest.ChangeType == "add")
                resultRecords.AddRange(createdRecord.Select(x => x.Id));
            else if (webhookConfigRequest.ChangeType == "update")
                resultRecords.AddRange(changedRecords.Select(x => x.Id));
            else if (webhookConfigRequest.ChangeType == "remove")
                resultRecords.AddRange(removedRecords.Select(x => x.Id));
        }
        else if(webhookConfigRequest.DataType == "tableFields")
        {
            if (webhookConfigRequest.ChangeType == "add")
                resultFields.AddRange(records.Select(record => record.CreatedFieldsById ?? new()).SelectMany(x => x.Keys));
            else if (webhookConfigRequest.ChangeType == "update")
                resultFields.AddRange(records.Select(record => record.ChangedFieldsById ?? new()).SelectMany(x => x.Keys));
            else if (webhookConfigRequest.ChangeType == "remove")
                resultFields.AddRange(records.SelectMany(record => record.DestroyedFieldsIds));
        }
        else if(webhookConfigRequest.DataType == "tableMetadata")
        {
            changedMetadata = records.LastOrDefault().ChangedMetadata.Current;
        }
        
        StoreCursor(changedTables.Cursor.ToString(), webhookId);

        return new()
        {
            HttpResponseMessage = new(HttpStatusCode.OK),
            Result = new()
            {
                TableId = table.TableId,
                ChangedRecords = resultRecords,
                ChangedFields = resultFields,
                ChangedMetadata = changedMetadata
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