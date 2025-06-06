﻿using System.Text;
using Apps.Airtable.Dtos;
using Apps.Airtable.Models.Entities;
using Apps.Airtable.Models.Identifiers;
using Apps.Airtable.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;
using Apps.Airtable.Models.Responses;
using Apps.Airtable.Models.Responses.Records;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using Apps.Airtable.Invocables;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Apps.Airtable.DataSourceHandlers;

namespace Apps.Airtable.Actions;

[ActionList]
public class RecordActions : AirtableInvocable
{
    private readonly IEnumerable<AuthenticationCredentialsProvider> _credentials;

    private readonly JsonSerializerSettings _jsonSerializerSettings =
        new() { MissingMemberHandling = MissingMemberHandling.Ignore };

    public RecordActions(InvocationContext invocationContext) : base(invocationContext)
    {
        _credentials = invocationContext.AuthenticationCredentialsProviders;
    }

    [Action("List records", Description = "List all records in the table.")]
    public async Task<ListRecordsResponse> ListRecords([ActionParameter] TableIdentifier tableIdentifier)
    {
        var request = new AirtableRequest($"/{tableIdentifier.TableId}", Method.Get, _credentials);
        var records = await ContentClient.Paginate<RecordsPaginationResponse, RecordResponse>(request);

        return new()
        {
            Records = records.Select(x => new RecordEntity(x))
        };
    }

    [Action("Search record", Description = "Search for a single record in the table.")]
    public async Task<RecordEntity> SearchRecord([ActionParameter] SingleFieldIdentifier identifier, [ActionParameter][Display("Equals")] string equals)
    {
        var request = new AirtableRequest($"/{identifier.TableId}", Method.Get, _credentials);
        request.AddQueryParameter("filterByFormula", $"{identifier.FieldId}=\"{equals}\"");
        var records = await ContentClient.Paginate<RecordsPaginationResponse, RecordResponse>(request);

        return records.Select(x => new RecordEntity(x)).FirstOrDefault() ?? new RecordEntity(new RecordResponse { Id = null, CreatedTime = DateTime.MinValue, Fields = new Dictionary<string, object> { } }); 
    }

    [Action("Add new record", Description = "Add a new record to the table, with at least the table's primary field value")]
    public async Task<RecordEntity> AddRecord([ActionParameter] TableIdentifier identifier, [ActionParameter][Display("Primary field value")] string value)
    {
        var primaryFieldId = await GetTablePrimaryFieldId(identifier.TableId);
        var request = new AirtableRequest($"/{identifier.TableId}", Method.Post, _credentials);
        var jsonBody = $@"
        {{
            ""fields"": {{
                ""{primaryFieldId}"": ""{value}""
            }},
            ""returnFieldsByFieldId"": true
        }}";
        request.AddJsonBody(jsonBody);
        var response = await ContentClient.ExecuteWithErrorHandling<RecordResponse>(request);

        return new RecordEntity(response) ;
    }

    [Action("Delete record", Description = "Delete a specific record from the table.")]
    public async Task DeleteRecord([ActionParameter] RecordIdentifier identifier)
    {
        var request = new AirtableRequest($"/{identifier.TableId}/{identifier.RecordId}", Method.Delete, _credentials);
        await ContentClient.ExecuteWithErrorHandling(request);
    }

    #region Field Getters

    [Action("Get value of text field", Description = "Get the value of a text field (e.g. single line text, " +
                                                       "long text, phone number, email, URL, single select).")]
    public async Task<FieldValueResponse<string>> GetStringFieldValue([ActionParameter] TextFieldAndRecordIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId, fieldIdentifier.FieldId);
        return new() { Value = field  };
    }

    [Action("Get value of number field", Description =
        "Get the value of a number field (e.g. number, currency, percent, " +
        "rating).")]
    public async Task<FieldValueResponse<double?>> GetNumberFieldValue(
        [ActionParameter] NumberFieldAndRecordIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId, fieldIdentifier.FieldId);

        try
        {
            return new() { Value = double.Parse(field) };
        }
        catch (FormatException)
        {
            throw new PluginMisconfigurationException($"Provided field is not a number type. Actual field value: {field}");
        }
    }

    [Action("Get value of date field", Description = "Get the value of a date field.")]
    public async Task<FieldValueResponse<DateTimeOffset?>> GetDateFieldValue(
        [ActionParameter] DateFieldAndRecordIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId, fieldIdentifier.FieldId);

        try
        {
            return new() { Value = DateTimeOffset.Parse(field) };
        }
        catch (FormatException)
        {
            throw new PluginMisconfigurationException($"Provided field is not a date type. Actual field value: {field}");
        }
    }

    [Action("Get value of boolean field", Description = "Get the value of a boolean field (e.g. checkbox).")]
    public async Task<FieldValueResponse<bool>> GetBooleanFieldValue([ActionParameter] BoolFieldAndRecordIdentifier fieldIdentifier)
    {
        var field = string.Empty;
        try
        {
            field = await GetFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId,
                fieldIdentifier.FieldId);
            return new() { Value = bool.Parse(field) };
        }
        catch (FormatException)
        {
            throw new PluginMisconfigurationException($"Provided field is not a boolean type. Actual field value: {field}");
        }
        catch (Exception ex)
        {
            if (ex.Message == ErrorMessages.EmptyRecordField)
                return new() { Value = false };

            throw new PluginApplicationException(ex.Message);
        }
    }

    [Action("Download files from attachment field", Description = "Download files from an attachment field.")]
    public async Task<FilesResponse> DownloadFilesFromAttachmentField([ActionParameter] AttachmentFieldAndRecordIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId, fieldIdentifier.FieldId);

        try
        {
            var files = JsonConvert.DeserializeObject<IEnumerable<FileDto>>(field, _jsonSerializerSettings)!;
            var downloadedFiles = new List<FileReference>();

            foreach (var file in files)
            {
                downloadedFiles.Add(new(new(HttpMethod.Get, file.Url), file.Filename, file.Type));
            }

            return new() { Files = downloadedFiles };
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError.Invoke($"Airtable field files download error. Exception: {ex}", null);
            throw new PluginMisconfigurationException($"Provided field is not a file type. Actual field value: {field}");
        }
    }

    #endregion

    #region Field setters

    [Action("Update value of text field", Description ="Update the value of a text field (e.g. long text, phone number, email, URL).")]
    public Task UpdateStringFieldValue([ActionParameter] FieldAndRecordIdentifier fieldIdentifier,
        [ActionParameter] [Display("New value")]
        string newValue)
    {
        var jsonBody = new
        {
            fields = new Dictionary<string, string> { { fieldIdentifier.FieldId, newValue } },
            returnFieldsByFieldId = true
        };
        return UpdateFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId,
            fieldIdentifier.FieldId, jsonBody);
    }

    [Action("Update value of select field", Description ="Update the value of a single text field")]
    public Task UpdateSelectFieldValue([ActionParameter] SelectFieldAndRecordIdentifier fieldIdentifier,
    [ActionParameter][DataSource(typeof(SingleSelectOptionsHandler))][Display("New value")] string newValue)
    {
        var jsonBody = new
        {
            fields = new Dictionary<string, string> { { fieldIdentifier.FieldId, newValue } },
            returnFieldsByFieldId = true
        };
        return UpdateFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId,
            fieldIdentifier.FieldId, jsonBody);
    }

    [Action("Update value of number field", Description =
        "Update the value of a number field (e.g. number, currency, " +
        "percent, rating).")]
    public Task UpdateNumberFieldValue([ActionParameter] FieldAndRecordIdentifier fieldIdentifier,
        [ActionParameter] [Display("New value")]
        double newValue)
    {
        var jsonBody = new
        {
            fields = new Dictionary<string, double> { { fieldIdentifier.FieldId, newValue } },
            returnFieldsByFieldId = true
        };
        return UpdateFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId,
            fieldIdentifier.FieldId, jsonBody);
    }

    [Action("Update value of date field", Description = "Update the value of a date field.")]
    public Task UpdateDateFieldValue([ActionParameter] FieldAndRecordIdentifier fieldIdentifier,
        [ActionParameter] [Display("New value")]
        DateTime newValue)
    {
        var jsonBody = new
        {
            fields = new Dictionary<string, DateTime> { { fieldIdentifier.FieldId, newValue } },
            returnFieldsByFieldId = true,
            typecast = true,
        };
        return UpdateFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId,
            fieldIdentifier.FieldId, jsonBody);
    }

    [Action("Update value of boolean field", Description = "Update the value of a boolean field (e.g. checkbox).")]
    public Task UpdateBooleanFieldValue([ActionParameter] FieldAndRecordIdentifier fieldIdentifier,
        [ActionParameter] [Display("New value")]
        bool newValue)
    {
        var jsonBody = new
        {
            fields = new Dictionary<string, bool> { { fieldIdentifier.FieldId, newValue } },
            returnFieldsByFieldId = true
        };
        return UpdateFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId,
            fieldIdentifier.FieldId, jsonBody);
    }

    //[Action("Upload file to attachment field", Description = "Upload a file to an attachment field.")]
    public async Task UploadFileToAttachmentField([ActionParameter] FieldAndRecordIdentifier fieldIdentifier,
        [ActionParameter] FileRequest file)
    {
        var field = await GetFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId, fieldIdentifier.FieldId);
        var files = JsonConvert.DeserializeObject<IEnumerable<FileDto>>(field,
            _jsonSerializerSettings) ?? new FileDto[] { };

        var jsonBody = new StringBuilder();
        jsonBody.AppendLine("{");
        jsonBody.AppendLine("\"fields\": {");
        jsonBody.AppendLine($"\"{fieldIdentifier.FieldId}\": [");

        foreach (var fileDto in files)
        {
            jsonBody.AppendLine($"{{ \"id\": \"{fileDto.Id}\" }},");
        }

        jsonBody.AppendLine("{");
        jsonBody.AppendLine($"\"url\": \"{file.File.Url}\",");
        jsonBody.AppendLine($"\"filename\": \"{file.File.Name}\"");
        jsonBody.AppendLine("}");
        jsonBody.AppendLine("]");
        jsonBody.AppendLine("}");
        jsonBody.AppendLine("}");

        await UpdateFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId,
            fieldIdentifier.FieldId, jsonBody.ToString());
    }

    #endregion

    private async Task<string> GetFieldValue(string tableId, string recordId, string fieldId)
    {
        var table = await GetFieldTable(tableId, fieldId);
        var request = new AirtableRequest($"/{tableId}/{recordId}", Method.Get, _credentials);
        request.AddQueryParameter("returnFieldsByFieldId", "true");

        try
        {
            var record = await ContentClient.ExecuteWithErrorHandling<RecordResponse>(request);
            if (!record.Fields.TryGetValue(fieldId, out var field))
                throw new PluginMisconfigurationException(ErrorMessages.EmptyRecordField);

            var fieldSchema = table.Fields.First(x => x.Id == fieldId);
            return fieldSchema.Type switch
            {
                "multipleLookupValues" => (field as JArray)!.First().ToString(),
                _ => field.ToString() ?? String.Empty
            };
        }
        catch (Exception ex)
        {
            if (ex.Message == "NOT_FOUND")
                throw new PluginMisconfigurationException(ErrorMessages.RecordNotFound);

            throw new PluginApplicationException(ex.Message);
        }
    }

    private async Task UpdateFieldValue(string tableId, string recordId, string fieldId,
        object jsonBody)
    {
        await GetFieldTable(tableId, fieldId);
        var request = new AirtableRequest($"/{tableId}/{recordId}", Method.Patch, _credentials)
            .AddJsonBody(jsonBody);
        try
        {
            await ContentClient.ExecuteWithErrorHandling(request);
        }
        catch (Exception ex)
        {
            if (ex.Message == "NOT_FOUND")
                throw new PluginMisconfigurationException(ErrorMessages.RecordNotFound);

            throw new PluginApplicationException(ex.Message);
        }
    }

    private async Task<FullTableDto> GetFieldTable(string tableId, string fieldId)
    {
        var request = new AirtableRequest("/tables", Method.Get, _credentials);
        var tables = await MetaClient.ExecuteWithErrorHandling<TableDtoWrapper<FullTableDto>>(request);
        var table = tables.Tables.FirstOrDefault(table => table.Id == tableId || table.Name == tableId);

        if (table is null)
            throw new PluginMisconfigurationException(ErrorMessages.TableNotFound);

        var field = table.Fields.FirstOrDefault(field => field.Id == fieldId);

        if (field == null)
            throw new PluginMisconfigurationException(ErrorMessages.FieldDoesNotExist);

        return table;
    }
}