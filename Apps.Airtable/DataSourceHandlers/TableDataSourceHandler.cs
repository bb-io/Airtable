using Apps.Airtable.Dtos;
using Apps.Airtable.Invocables;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Airtable.DataSourceHandlers;

public class TableDataSourceHandler : AirtableInvocable, IAsyncDataSourceHandler
{
    public TableDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new AirtableRequest("/tables", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var tables = await MetaClient.ExecuteWithErrorHandling<TableDtoWrapper<TableDto>>(request);
        return tables.Tables
            .Where(table => context.SearchString == null || table.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(table => table.Id, table => table.Name);
    }
}