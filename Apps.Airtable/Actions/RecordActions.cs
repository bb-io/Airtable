using System.Text;
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
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using ClosedXML.Excel;
using System.Globalization;

namespace Apps.Airtable.Actions;

[ActionList("Records")]
public class RecordActions : AirtableInvocable
{
    private readonly IEnumerable<AuthenticationCredentialsProvider> _credentials;
    private readonly IFileManagementClient _fileManagementClient;

    private readonly JsonSerializerSettings _jsonSerializerSettings =
        new() { MissingMemberHandling = MissingMemberHandling.Ignore };

    public RecordActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _credentials = invocationContext.AuthenticationCredentialsProviders;
        _fileManagementClient = fileManagementClient;
    }

    [Action("Search records", Description = "Search all records in the table")]
    public async Task<ListRecordsResponse> ListRecords([ActionParameter] TableIdentifier tableIdentifier,
        [ActionParameter] string? View)
    {
        var request = new AirtableRequest($"/{tableIdentifier.TableId}", Method.Get, _credentials);
        if (!String.IsNullOrEmpty(View))
        { request.AddQueryParameter("view",View); }
        var records = await ContentClient.Paginate<RecordsPaginationResponse, RecordResponse>(request);

        return new()
        {
            Records = records.Select(x => new RecordEntity(x))
        };
    }

    [Action("Export table as Excel file", Description = "Export table records to an Excel file.")]
    public async Task<FileWrapper> ExportTableAsExcelFile([ActionParameter] ExportTableAsExcelRequest input)
    {
        var table = await GetTable(input.TableId);
        var request = new AirtableRequest($"/{input.TableId}", Method.Get, _credentials);

        if (!string.IsNullOrWhiteSpace(input.View))
            request.AddQueryParameter("view", input.View);

        var selectedFields = input.Fields?.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var exportedFields = selectedFields == null || selectedFields.Count == 0
            ? table.Fields.ToList()
            : table.Fields.Where(x => selectedFields.Contains(x.Id)).ToList();

        foreach (var field in exportedFields)
        {
            request.AddQueryParameter("fields[]", field.Name);
        }

        var records = await ContentClient.Paginate<RecordsPaginationResponse, RecordResponse>(request);

        using var workbook = new XLWorkbook();
        var sheetName = string.IsNullOrWhiteSpace(table.Name) ? "Records" : table.Name;
        if (sheetName.Length > 31)
            sheetName = sheetName[..31];

        var worksheet = workbook.Worksheets.Add(sheetName);
        var fieldNames = exportedFields.Select(x => x.Name).ToList();
        var headers = new List<string> { "Record ID", "Created time" };
        headers.AddRange(fieldNames);

        for (var i = 0; i < headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        for (var rowIndex = 0; rowIndex < records.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 2;
            var record = records[rowIndex];

            worksheet.Cell(rowNumber, 1).Value = record.Id;
            worksheet.Cell(rowNumber, 2).Value = record.CreatedTime;
            worksheet.Cell(rowNumber, 2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";

            for (var fieldIndex = 0; fieldIndex < fieldNames.Count; fieldIndex++)
            {
                var fieldName = fieldNames[fieldIndex];
                if (record.Fields == null || !record.Fields.TryGetValue(fieldName, out var value))
                    continue;

                SetExcelCellValue(worksheet.Cell(rowNumber, fieldIndex + 3), value);
            }
        }

        var headerRange = worksheet.Range(1, 1, 1, headers.Count);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
        worksheet.SheetView.FreezeRows(1);
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"{SanitizeFileName(table.Name ?? string.Empty)}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        var file = await _fileManagementClient.UploadAsync(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

        return new FileWrapper { File = file };
    }

    [Action("Update table from file", Description = "Update Airtable records from an exported Excel file using a unique key field.")]
    public async Task<UpdateTableFromFileResponse> UpdateTableFromFile([ActionParameter] UniqueFieldIdentifier identifier,
        [ActionParameter] FileRequest file)
    {
        var table = await GetTable(identifier.TableId);
        var uniqueKeyField = table.Fields.FirstOrDefault(x => x.Id == identifier.FieldId);

        if (uniqueKeyField == null)
            throw new PluginMisconfigurationException(ErrorMessages.FieldDoesNotExist);

        await using var fileStream = await _fileManagementClient.DownloadAsync(file.File);
        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);
        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
            throw new PluginMisconfigurationException("The provided Excel file is empty.");

        var headerRow = usedRange.FirstRowUsed();
        if (headerRow == null)
            throw new PluginMisconfigurationException("The provided Excel file does not contain a header row.");

        var headers = headerRow.Cells()
            .Select((cell, index) => new
            {
                Index = index + 1,
                Header = cell.GetString().Trim()
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Header))
            .ToList();

        var uniqueKeyHeader = uniqueKeyField.Name;
        var uniqueKeyColumn = headers.FirstOrDefault(x => string.Equals(x.Header, uniqueKeyHeader, StringComparison.OrdinalIgnoreCase));

        if (uniqueKeyColumn == null)
            throw new PluginMisconfigurationException($"The Excel file does not contain the unique key column '{uniqueKeyHeader}'.");

        var updatableFieldsByName = table.Fields
            .Where(IsSupportedImportFieldType)
            .ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        var ignoredColumns = headers
            .Where(x => x.Header is not "Record ID" and not "Created time" && !updatableFieldsByName.ContainsKey(x.Header))
            .Select(x => x.Header)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var currentRecordsRequest = new AirtableRequest($"/{identifier.TableId}", Method.Get, _credentials);
        var currentRecords = await ContentClient.Paginate<RecordsPaginationResponse, RecordResponse>(currentRecordsRequest);
        var duplicateExistingKeys = currentRecords
            .Select(record => new
            {
                Record = record,
                Key = record.Fields != null && record.Fields.TryGetValue(uniqueKeyHeader, out var value)
                    ? NormalizeAirtableValue(value)
                    : null
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .GroupBy(x => x.Key!, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateExistingKeys.Any())
            throw new PluginMisconfigurationException($"The unique key field '{uniqueKeyHeader}' contains duplicate values in Airtable.");

        var recordsByUniqueKey = currentRecords
            .Select(record => new
            {
                Record = record,
                Key = record.Fields != null && record.Fields.TryGetValue(uniqueKeyHeader, out var value)
                    ? NormalizeAirtableValue(value)
                    : null
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .ToDictionary(x => x.Key!, x => x.Record, StringComparer.OrdinalIgnoreCase);

        var rows = usedRange.RowsUsed().Skip(1).ToList();
        var recordsToUpdate = new List<object>();
        var skippedRows = 0;
        var processedUniqueKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var uniqueKeyValue = NormalizeExcelCell(row.Cell(uniqueKeyColumn.Index));
            if (string.IsNullOrWhiteSpace(uniqueKeyValue))
            {
                skippedRows++;
                continue;
            }

            if (!recordsByUniqueKey.TryGetValue(uniqueKeyValue, out var existingRecord))
            {
                skippedRows++;
                continue;
            }

            if (!processedUniqueKeys.Add(uniqueKeyValue))
                throw new PluginMisconfigurationException($"The Excel file contains duplicate values for the unique key column '{uniqueKeyHeader}'.");

            var fieldsToUpdate = new Dictionary<string, object?>();

            foreach (var header in headers)
            {
                if (header.Header is "Record ID" or "Created time")
                    continue;

                if (!updatableFieldsByName.TryGetValue(header.Header, out var field))
                    continue;

                var parsedValue = ParseExcelCell(row.Cell(header.Index), field.Type);
                if (parsedValue.ShouldInclude)
                    fieldsToUpdate[field.Name] = parsedValue.Value;
            }

            if (fieldsToUpdate.Count == 0)
            {
                skippedRows++;
                continue;
            }

            recordsToUpdate.Add(new
            {
                id = existingRecord.Id,
                fields = fieldsToUpdate
            });
        }

        foreach (var chunk in recordsToUpdate.Chunk(10))
        {
            var request = new AirtableRequest($"/{identifier.TableId}", Method.Patch, _credentials);
            request.AddJsonBody(new
            {
                records = chunk,
                typecast = true,
                returnFieldsByFieldId = false
            });

            await ContentClient.ExecuteWithErrorHandling(request);
        }

        return new UpdateTableFromFileResponse
        {
            UpdatedRecords = recordsToUpdate.Count,
            SkippedRows = skippedRows,
            IgnoredColumns = ignoredColumns
        };
    }

    [Action("Find record", Description = "Find a single record in the table.")]
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
        return new FieldValueResponse<string> { Value = field ?? string.Empty };
    }


    [Action("Get value of number field", Description =
     "Get the value of a number field (e.g. number, currency, percent, " +
     "rating).")]
    public async Task<FieldValueResponse<double?>> GetNumberFieldValue(
     [ActionParameter] NumberFieldAndRecordIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId, fieldIdentifier.FieldId);

        if (string.IsNullOrEmpty(field))
            return new() { Value = null };

        if (double.TryParse(field, out var result))
            return new() { Value = result };

        throw new PluginMisconfigurationException($"Provided field is not a number type. Actual field value: {field}");
    }

    [Action("Get value of date field", Description = "Get the value of a date field.")]
    public async Task<FieldValueResponse<DateTimeOffset?>> GetDateFieldValue(
    [ActionParameter] DateFieldAndRecordIdentifier fieldIdentifier)
    {
        var field = await GetFieldValue(fieldIdentifier.TableId, fieldIdentifier.RecordId, fieldIdentifier.FieldId);

        if (string.IsNullOrEmpty(field))
            return new() { Value = null };

        if (DateTimeOffset.TryParse(field, out var result))
            return new() { Value = result };

        throw new PluginMisconfigurationException($"Provided field is not a date type. Actual field value: {field}");
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

        RecordResponse? record = null;
        try
        {
            record = await ContentClient.ExecuteWithErrorHandling<RecordResponse>(request);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            throw;
        }

        if (record is null)
            return string.Empty;

        if (!record.Fields.TryGetValue(fieldId, out var rawValue))
            return string.Empty;

        var schema = table.Fields.First(x => x.Id == fieldId);

        return schema.Type switch
        {
            "multipleLookupValues" when rawValue is JArray { Count: > 0 } arr
                => arr[0].ToString() ?? string.Empty,

            "multipleLookupValues" => string.Empty,

            _ => rawValue?.ToString() ?? string.Empty
        };
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
        var table = await GetTable(tableId);

        if (table is null)
            throw new PluginMisconfigurationException(ErrorMessages.TableNotFound);

        var field = table.Fields.FirstOrDefault(field => field.Id == fieldId);

        if (field == null)
            throw new PluginMisconfigurationException(ErrorMessages.FieldDoesNotExist);

        return table;
    }

    private async Task<FullTableDto> GetTable(string tableId)
    {
        var request = new AirtableRequest("/tables", Method.Get, _credentials);
        var tables = await MetaClient.ExecuteWithErrorHandling<TableDtoWrapper<FullTableDto>>(request);
        var table = tables.Tables.FirstOrDefault(x => x.Id == tableId || x.Name == tableId);

        return table ?? throw new PluginMisconfigurationException(ErrorMessages.TableNotFound);
    }

    private static void SetExcelCellValue(IXLCell cell, object? value)
    {
        if (value is null)
        {
            cell.Value = string.Empty;
            return;
        }

        switch (value)
        {
            case JValue jValue:
                SetExcelCellValue(cell, jValue.Value);
                return;
            case JArray jArray:
                cell.Value = string.Join(", ",
                    jArray.Select(x => x?.ToString())
                        .Where(x => !string.IsNullOrWhiteSpace(x)));
                return;
            case JObject jObject:
                cell.Value = jObject.ToString(Formatting.None);
                return;
            case DateTime dateTime:
                cell.Value = dateTime;
                return;
            case DateTimeOffset dateTimeOffset:
                cell.Value = dateTimeOffset.UtcDateTime;
                return;
            case bool boolean:
                cell.Value = boolean;
                return;
            case byte byteValue:
                cell.Value = byteValue;
                return;
            case sbyte sbyteValue:
                cell.Value = sbyteValue;
                return;
            case short shortValue:
                cell.Value = shortValue;
                return;
            case ushort ushortValue:
                cell.Value = ushortValue;
                return;
            case int intValue:
                cell.Value = intValue;
                return;
            case uint uintValue:
                cell.Value = uintValue;
                return;
            case long longValue:
                cell.Value = longValue;
                return;
            case ulong ulongValue:
                cell.Value = ulongValue.ToString();
                return;
            case float floatValue:
                cell.Value = floatValue;
                return;
            case double doubleValue:
                cell.Value = doubleValue;
                return;
            case decimal decimalValue:
                cell.Value = decimalValue;
                return;
            default:
                cell.Value = value.ToString() ?? string.Empty;
                return;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "airtable-export" : sanitized;
    }

    private static bool IsSupportedImportFieldType(FieldDto field)
    {
        return field.Type is
            "singleLineText" or
            "multilineText" or
            "richText" or
            "email" or
            "url" or
            "phoneNumber" or
            "number" or
            "percent" or
            "currency" or
            "rating" or
            "checkbox" or
            "date" or
            "dateTime" or
            "singleSelect";
    }

    private static string NormalizeAirtableValue(object? value)
    {
        if (value == null)
            return string.Empty;

        return value switch
        {
            JValue jValue => NormalizeAirtableValue(jValue.Value),
            JArray jArray => string.Join(", ", jArray.Select(x => x?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x))),
            JObject jObject => jObject.ToString(Formatting.None),
            bool boolean => boolean.ToString().ToLowerInvariant(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
            _ => value.ToString()?.Trim() ?? string.Empty
        };
    }

    private static string NormalizeExcelCell(IXLCell cell)
    {
        if (cell.IsEmpty())
            return string.Empty;

        return cell.DataType switch
        {
            XLDataType.Boolean => cell.GetBoolean().ToString().ToLowerInvariant(),
            XLDataType.Number => cell.GetDouble().ToString(CultureInfo.InvariantCulture),
            XLDataType.DateTime => cell.GetDateTime().ToString("o", CultureInfo.InvariantCulture),
            XLDataType.TimeSpan => cell.GetTimeSpan().ToString("c", CultureInfo.InvariantCulture),
            _ => cell.GetString().Trim()
        };
    }

    private static (bool ShouldInclude, object? Value) ParseExcelCell(IXLCell cell, string fieldType)
    {
        if (cell.IsEmpty())
            return (true, null);

        return fieldType switch
        {
            "checkbox" => cell.DataType == XLDataType.Boolean
                ? (true, cell.GetBoolean())
                : bool.TryParse(cell.GetString(), out var boolValue)
                    ? (true, boolValue)
                    : (true, null),

            "number" or "percent" or "currency" or "rating" => cell.DataType == XLDataType.Number
                ? (true, cell.GetDouble())
                : double.TryParse(cell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var numberValue)
                    ? (true, numberValue)
                    : (true, null),

            "date" => cell.DataType == XLDataType.DateTime
                ? (true, cell.GetDateTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                : (true, cell.GetString()),

            "dateTime" => cell.DataType == XLDataType.DateTime
                ? (true, cell.GetDateTime().ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture))
                : (true, cell.GetString()),

            _ => (true, cell.GetString())
        };
    }
}
