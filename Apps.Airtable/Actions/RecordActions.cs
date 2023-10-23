using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Airtable.Models.Requests;
using RestSharp;
using Apps.Airtable.Models.Responses;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Airtable.Actions;

[ActionList]
public class RecordActions : BaseInvocable
{
    private readonly IEnumerable<AuthenticationCredentialsProvider> _credentials;

    public RecordActions(InvocationContext invocationContext) : base(invocationContext)
    {
        _credentials = invocationContext.AuthenticationCredentialsProviders;
    }
    
    [Action("List all records", Description = "List all records")]
    public ListRecordsResponse ListRecords([ActionParameter] ListRecordsRequest input)
    {
        var client = new AirtableContentClient(_credentials);
        var request = new AirtableRequest($"/{input.BaseId}/{input.TableId}", Method.Get, _credentials);
        return client.Get<ListRecordsResponse>(request);
    }
}
