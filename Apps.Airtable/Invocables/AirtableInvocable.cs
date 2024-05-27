using Apps.Airtable.Dtos;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Airtable.Invocables;

public class AirtableInvocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected AirtableClient ContentClient { get; }
    protected AirtableClient MetaClient { get; }
    
    public AirtableInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        ContentClient = new(Creds, new AirtableContentUrlBuilder());
        MetaClient = new (Creds, new AirtableMetaUrlBuilder());
    }

    protected async Task<string> GetTablePrimaryFieldId(string tableId)
    {
        var tableRequest = new AirtableRequest("/tables", Method.Get, Creds); ;
        var tables = await MetaClient.ExecuteWithErrorHandling<TableDtoWrapper<TableDto>>(tableRequest);

        var table = tables.Tables.FirstOrDefault(x => x.Id == tableId);

        if (table == null) throw new Exception($"Could not find table with ID {tableId}");

        return table.PrimaryFieldId;
    }
}