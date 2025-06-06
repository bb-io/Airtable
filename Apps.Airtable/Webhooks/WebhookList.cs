﻿using System.Collections.Generic;
using System.Net;
using Apps.Airtable.DataSourceHandlers;
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
using Blackbird.Applications.Sdk.Common.Dynamic;
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
        [WebhookParameter(true)] TableIdentifier table,
        [WebhookParameter(true)] WebhookConfigRequest webhookConfigRequest
        )
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

    [Webhook("On cells updated", typeof(OnFieldUpdatedHandler),
    Description = "This webhook is triggered when one or more cells have been changed")]
    public async Task<WebhookResponse<UpdatedFieldResponse>> OnCellsUpdated(
        WebhookRequest request,
        [WebhookParameter(true)] OptionalFieldAndRecordIdentifier fieldAndRecord,
        [WebhookParameter][Display("New value")] string? newValue
    )
    {
        var (changedTables, webhookId) = await ProcessWebhookRequest<ChangedItemWrapperPayload>(request);

        if (!changedTables.Payloads.Any(p => p.ChangedTablesById != null && p.ChangedTablesById.ContainsKey(fieldAndRecord.TableId)))
            return new()
            {
                HttpResponseMessage = new(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };

        var records = changedTables.Payloads
            .Select(payload => payload.ChangedTablesById)
            .Where(changedTable => changedTable != null && changedTable.ContainsKey(fieldAndRecord.TableId))
            .Select(changedTable => JsonConvert.DeserializeObject<ChangedDataPayload>
                (changedTable[fieldAndRecord.TableId].ToString(), _jsonSerializerSettings))
            .ToList();

        var resultFields = new List<string>();
        CurrentMetadata changedMetadata = null;

        var changedRecords = records.SelectMany(record => record.ChangedRecordsById ?? new());

        StoreCursor(changedTables.Cursor.ToString(), webhookId);

        var result = new UpdatedFieldResponse { ChangedRecords = new List<ChangedRecord>() };
        foreach(var changedRecord in changedRecords)
        {
            if (fieldAndRecord.RecordId != null && changedRecord.Key != fieldAndRecord.RecordId) continue;
            var fields = changedRecord.Value.Current.CellValuesByFieldId;
            if (fieldAndRecord.FieldId != null && !fields.ContainsKey(fieldAndRecord.FieldId)) continue;
            fields.TryGetValue(fieldAndRecord.FieldId ?? "", out var foundNewValue);
            if (foundNewValue == null) continue;
            if (newValue != null && foundNewValue.ToString() != newValue) continue;

            result.ChangedRecords.Add(new ChangedRecord
            {
                FieldId = fieldAndRecord.FieldId,
                RecordId = changedRecord.Key,
                NewValue = foundNewValue.ToString()
            });
        }

        if (!result.ChangedRecords.Any())
        {
            return new()
            {
                HttpResponseMessage = new(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        }        

        return new()
        {
            HttpResponseMessage = new(HttpStatusCode.OK),
            Result = result,
        };
    }

    [Webhook("On select updated", typeof(OnFieldUpdatedHandler), Description = "This webhook is triggered when a select field changes its status")]
    public async Task<WebhookResponse<UpdatedFieldResponse>> OnSelectUpdated(
    WebhookRequest request,
    [WebhookParameter(true)] OptionalSelectFieldAndRecordIdentifier fieldAndRecord,
    [WebhookParameter][DataSource(typeof(SingleSelectOptionsHandler))][Display("New value")] string? newValue
)
    {
        var (changedTables, webhookId) = await ProcessWebhookRequest<ChangedItemWrapperPayload>(request);

        if (!changedTables.Payloads.Any(p => p.ChangedTablesById != null && p.ChangedTablesById.ContainsKey(fieldAndRecord.TableId)))
            return new()
            {
                HttpResponseMessage = new(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };

        var records = changedTables.Payloads
            .Select(payload => payload.ChangedTablesById)
            .Where(changedTable => changedTable != null && changedTable.ContainsKey(fieldAndRecord.TableId))
            .Select(changedTable => JsonConvert.DeserializeObject<ChangedDataPayload>
                (changedTable[fieldAndRecord.TableId].ToString(), _jsonSerializerSettings))
            .ToList();

        var resultFields = new List<string>();
        CurrentMetadata changedMetadata = null;

        var changedRecords = records.SelectMany(record => record.ChangedRecordsById ?? new());

        StoreCursor(changedTables.Cursor.ToString(), webhookId);

        var result = new UpdatedFieldResponse { ChangedRecords = new List<ChangedRecord>() };
        foreach (var changedRecord in changedRecords)
        {
            if (fieldAndRecord.RecordId != null && changedRecord.Key != fieldAndRecord.RecordId) continue;
            var fields = changedRecord.Value.Current.CellValuesByFieldId;
            if (fieldAndRecord.FieldId != null && !fields.ContainsKey(fieldAndRecord.FieldId)) continue;
            fields.TryGetValue(fieldAndRecord.FieldId ?? "", out var foundNewValue);
            if (foundNewValue == null) continue;

            try
            {
                var newValueDto = JsonConvert.DeserializeObject<ChoiceDto>(foundNewValue?.ToString());
                foundNewValue = newValueDto?.Name;
            }
            catch { }

            if (newValue != null && foundNewValue.ToString() != newValue) continue;

            result.ChangedRecords.Add(new ChangedRecord
            {
                FieldId = fieldAndRecord.FieldId,
                RecordId = changedRecord.Key,
                NewValue = foundNewValue.ToString()
            });
        }

        if (!result.ChangedRecords.Any())
        {
            return new()
            {
                HttpResponseMessage = new(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        }

        return new()
        {
            HttpResponseMessage = new(HttpStatusCode.OK),
            Result = result,
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