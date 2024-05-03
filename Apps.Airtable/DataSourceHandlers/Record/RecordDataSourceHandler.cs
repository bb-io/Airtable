using Apps.Airtable.Dtos;
using Apps.Airtable.Invocables;
using Apps.Airtable.Models.Responses.Records;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Airtable.DataSourceHandlers.Record;

public class RecordDataSourceHandler : AirtableInvocable, IAsyncDataSourceHandler
{
    private readonly string _tableId;

    public RecordDataSourceHandler(InvocationContext invocationContext, string tableId) :
        base(invocationContext)
    {
        _tableId = tableId;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tableId))
            throw new("You should specify the Table ID first");

        var request = new AirtableRequest($"/{_tableId}", Method.Get, Creds);
        var records = await ContentClient.Paginate<RecordsPaginationResponse, RecordResponse>(request);

        return records
            .Select(x => (x.Id, x.Fields["Name"]?.ToString() ?? x.Id))
            .Where(x => context.SearchString is null || x.Item2.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(30)
            .ToDictionary(x => x.Id, x => x.Item2);
    }
}