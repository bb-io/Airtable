using Apps.Airtable.Dtos;
using Apps.Airtable.Invocables;
using Apps.Airtable.Models.Identifiers;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using System.Linq;

namespace Apps.Airtable.DataSourceHandlers;

public class TextFieldDataSourceHandler : AirtableInvocable, IAsyncDataSourceHandler
{
    private readonly FieldAndRecordIdentifier _field;

    public TextFieldDataSourceHandler(InvocationContext invocationContext, [ActionParameter] FieldAndRecordIdentifier field) : base(
        invocationContext)
    {
        _field = field;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_field.TableId))
            throw new("You should specify the Table ID first");

        var tableRequest = new AirtableRequest("/tables", Method.Get, InvocationContext.AuthenticationCredentialsProviders); ;
        var tables = await MetaClient.ExecuteWithErrorHandling<TableDtoWrapper<FullTableDto>>(tableRequest);

        var table = tables.Tables.FirstOrDefault(x => x.Id == _field.TableId);

        if (table == null) throw new Exception($"Could not find table with ID {_field.TableId}");
        var textTypes = new List<string> { "singleLineText", "singleSelect", "multilineText", "phoneNumber", "email", "url", "barcode", "richText" };
        return table.Fields
            .Where(x => textTypes.Contains(x.Type) && (context.SearchString is null ||
                        x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)))
            .ToDictionary(x => x.Id, x => x.Name);
    }
}