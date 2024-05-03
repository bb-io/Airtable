using Apps.Airtable.Dtos;
using Apps.Airtable.Invocables;
using Apps.Airtable.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Airtable.DataSourceHandlers;

public class FieldDataSourceHandler : AirtableInvocable, IAsyncDataSourceHandler
{
    private readonly FieldIdentifier _field;

    public FieldDataSourceHandler(InvocationContext invocationContext, [ActionParameter] FieldIdentifier field) : base(
        invocationContext)
    {
        _field = field;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_field.TableId))
            throw new("You should specify the Table ID first");

        if (string.IsNullOrWhiteSpace(_field.RecordId))
            throw new("You should specify the Record ID first");

        var request = new AirtableRequest($"/{_field.TableId}/{_field.RecordId}", Method.Get, Creds);
        var record = await ContentClient.ExecuteWithErrorHandling<RecordResponse>(request);

        return record.Fields
            .Where(x => context.SearchString is null ||
                        x.Key.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Key, x => x.Key);
    }
}