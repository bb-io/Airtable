using System.Text;
using Apps.Airtable.Dtos;
using Apps.Airtable.Models.Identifiers;
using Apps.Airtable.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;
using Apps.Airtable.Models.Responses;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Airtable.Actions;

[ActionList]
public class RecordActions : BaseInvocable
{
    private readonly IEnumerable<AuthenticationCredentialsProvider> _credentials;
    private readonly AirtableClient _client;

    private readonly JsonSerializerSettings _jsonSerializerSettings =
        new() { MissingMemberHandling = MissingMemberHandling.Ignore };

    public RecordActions(InvocationContext invocationContext) : base(invocationContext)
    {
        _credentials = invocationContext.AuthenticationCredentialsProviders;
        _client = new AirtableClient(_credentials, new AirtableContentUrlBuilder());
    }

    #region GET

    [Action("List records", Description = "List all records in the table.")]
    public async Task<ListRecordsResponse> ListRecords([ActionParameter] TableIdentifier tableIdentifier)
    {
        var request = new AirtableRequest($"/{tableIdentifier.TableId}", Method.Get, _credentials);
        var records = await _client.ExecuteWithErrorHandling<ListRecordsResponse>(request);
        return records;
    }

    [Action("Get value of string field", Description = "Get the value of a string field (e.g. single line text, " +
                                                       "long text, phone number, email, URL, single select).")]
    public async Task<FieldValueResponse<string>> GetStringFieldValue([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier);
        return new FieldValueResponse<string> { Value = field?.ToString() ?? string.Empty };
    }
    
    [Action("Get value of number field", Description = "Get the value of a number field (e.g. number, currency, percent, " +
                                                       "rating).")]
    public async Task<FieldValueResponse<double?>> GetNumberFieldValue([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier);
        return new FieldValueResponse<double?> { Value = (double?)field };
    }
    
    [Action("Get value of date field", Description = "Get the value of a date field.")]
    public async Task<FieldValueResponse<DateTimeOffset?>> GetDateFieldValue([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier);
        return new FieldValueResponse<DateTimeOffset?> { Value = (DateTime?)field };
    }
    
    [Action("Get value of boolean field", Description = "Get the value of a boolean field (e.g. checkbox).")]
    public async Task<FieldValueResponse<bool>> GetBooleanFieldValue([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier);
        return new FieldValueResponse<bool> { Value = field != null };
    }
    
    [Action("Download files from attachment field", Description = "Download files from an attachment field.")]
    public async Task<FilesResponse> DownloadFilesFromAttachmentField([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier);
        var files = JsonConvert.DeserializeObject<IEnumerable<FileDto>>(field?.ToString() ?? string.Empty,
            _jsonSerializerSettings) ?? new FileDto[] { };
        var downloadedFiles = new List<FileWrapper>();
        
        foreach (var file in files)
        {
            var client = new RestClient(file.Url);
            var response = await client.ExecuteAsync(new RestRequest(""));
            downloadedFiles.Add(new FileWrapper
            {
                File = new File(response.RawBytes)
                {
                    Name = file.Filename,
                    ContentType = file.Type
                }
            });
        }

        return new FilesResponse { Files = downloadedFiles };
    }

    private async Task<object?> GetFieldValue(TableIdentifier tableIdentifier, RecordIdentifier recordIdentifier, 
        FieldIdentifier fieldIdentifier)
    {
        await CheckIfFieldExistsInTable(tableIdentifier, fieldIdentifier);
        var request = new AirtableRequest($"/{tableIdentifier.TableId}/{recordIdentifier.RecordId}", Method.Get, 
            _credentials);

        try
        {
            var record = await _client.ExecuteWithErrorHandling<FullRecordDto>(request);
            record.Fields.TryGetValue(fieldIdentifier.FieldName, out var field);
            return field;
        }
        catch
        {
            throw new Exception("Record with Record ID provided was not found.");
        }
    }
    
    #endregion

    #region PATCH

    [Action("Update value of string field", Description = "Update the value of a string field (e.g. single line text, " +
                                                          "long text, phone number, email, URL, single select).")]
    public async Task<RecordDto> UpdateStringFieldValue([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier, 
        [ActionParameter] [Display("New value")] string newValue)
    {
        var jsonBody = $@"
        {{
            ""fields"": {{
                ""{fieldIdentifier.FieldName}"": ""{newValue}""
            }}
        }}";
        var record = await UpdateFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier, jsonBody);
        return record;
    }
    
    [Action("Update value of number field", Description = "Update the value of a number field (e.g. number, currency, " +
                                                          "percent, rating).")]
    public async Task<RecordDto> UpdateNumberFieldValue([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier, 
        [ActionParameter] [Display("New value")] double newValue)
    {
        var jsonBody = $@"
        {{
            ""fields"": {{
                ""{fieldIdentifier.FieldName}"": {newValue}
            }}
        }}";
        var record = await UpdateFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier, jsonBody);
        return record;
    }

    [Action("Update value of date field", Description = "Update the value of a date field.")]
    public async Task<RecordDto> UpdateDateFieldValue([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier, 
        [ActionParameter] [Display("New value")] DateTime newValue)
    {
        var jsonBody = $@"
        {{
            ""fields"": {{
                ""{fieldIdentifier.FieldName}"": ""{newValue.ToString("O")}""
            }}
        }}";
        var record = await UpdateFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier, jsonBody);
        return record;
    }

    [Action("Update value of boolean field", Description = "Update the value of a boolean field (e.g. checkbox).")]
    public async Task<RecordDto> UpdateBooleanFieldValue([ActionParameter] TableIdentifier tableIdentifier,
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] [Display("New value")] bool newValue)
    {
        var jsonBody = $@"
        {{
            ""fields"": {{
                ""{fieldIdentifier.FieldName}"": {newValue.ToString().ToLowerInvariant()}
            }}
        }}";
        var record = await UpdateFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier, jsonBody);
        return record;
    }
    
    //[Action("Upload file to attachment field", Description = "Upload a file to an attachment field.")]
    public async Task<RecordDto> UploadFileToAttachmentField([ActionParameter] TableIdentifier tableIdentifier, 
        [ActionParameter] RecordIdentifier recordIdentifier, [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] FileRequest file)
    {
        var field = await GetFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier);
        var files = JsonConvert.DeserializeObject<IEnumerable<FileDto>>(field?.ToString() ?? string.Empty,
            _jsonSerializerSettings) ?? new FileDto[] { };

        var jsonBody = new StringBuilder();
        jsonBody.AppendLine("{");
        jsonBody.AppendLine("\"fields\": {");
        jsonBody.AppendLine($"\"{fieldIdentifier.FieldName}\": [");

        foreach (var fileDto in files)
        {
            jsonBody.AppendLine($"{{ \"id\": \"{fileDto.Id}\" }},");
        }

        jsonBody.AppendLine("{");
        jsonBody.AppendLine($"\"url\": \"{file.File.DownloadUrl}\",");
        jsonBody.AppendLine($"\"filename\": \"{file.File.Name}\"");
        jsonBody.AppendLine("}");
        jsonBody.AppendLine("]");
        jsonBody.AppendLine("}");
        jsonBody.AppendLine("}");
        
        var record = await UpdateFieldValue(tableIdentifier, recordIdentifier, fieldIdentifier, jsonBody.ToString());
        return record;
    }

    private async Task<RecordDto> UpdateFieldValue(TableIdentifier tableIdentifier, RecordIdentifier recordIdentifier, 
        FieldIdentifier fieldIdentifier, string jsonBody)
    {
        await CheckIfFieldExistsInTable(tableIdentifier, fieldIdentifier);
        var request = new AirtableRequest($"/{tableIdentifier.TableId}/{recordIdentifier.RecordId}", Method.Patch, 
            _credentials);
        request.AddJsonBody(jsonBody);

        try
        {
            var record = await _client.ExecuteWithErrorHandling<RecordDto>(request);
            return record;
        }
        catch
        {
            throw new Exception("Record with Record ID provided was not found.");
        }
    }

    #endregion
    
    private async Task CheckIfFieldExistsInTable(TableIdentifier tableIdentifier, FieldIdentifier fieldIdentifier)
    {
        var client = new AirtableClient(_credentials, new AirtableMetaUrlBuilder());
        var request = new AirtableRequest("/tables", Method.Get, _credentials);
        var tables = await client.ExecuteWithErrorHandling<TableDtoWrapper<FullTableDto>>(request);
        var table = tables.Tables.First(table =>
            table.Id == tableIdentifier.TableId || table.Name == tableIdentifier.TableId);
        var field = table.Fields.FirstOrDefault(field => field.Name == fieldIdentifier.FieldName);

        if (field == null)
            throw new Exception("Field with the specified name does not exist in the table.");
    }
}
