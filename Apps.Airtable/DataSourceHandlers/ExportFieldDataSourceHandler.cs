using Apps.Airtable.Dtos;
using Apps.Airtable.Invocables;
using Apps.Airtable.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Airtable.DataSourceHandlers;

public class ExportFieldDataSourceHandler : AirtableInvocable, IAsyncDataSourceHandler
{
    private readonly ExportTableAsExcelRequest _request;

    public ExportFieldDataSourceHandler(InvocationContext invocationContext, [ActionParameter] ExportTableAsExcelRequest request)
        : base(invocationContext)
    {
        _request = request;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_request.TableId))
            throw new("You should specify the Table ID first");

        var tableRequest = new AirtableRequest("/tables", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var tables = await MetaClient.ExecuteWithErrorHandling<TableDtoWrapper<FullTableDto>>(tableRequest);
        var table = tables.Tables.FirstOrDefault(x => x.Id == _request.TableId);

        if (table == null)
            throw new Exception($"Could not find table with ID {_request.TableId}");

        return table.Fields
            .Where(x => context.SearchString is null ||
                        x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Id, x => x.Name);
    }
}
