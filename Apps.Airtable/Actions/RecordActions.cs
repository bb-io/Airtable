using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Airtable.Models.Requests;
using RestSharp;
using Apps.Airtable.Models.Responses;
using Blackbird.Applications.Sdk.Common.Actions;

namespace Apps.Airtable.Actions
{
    [ActionList]
    public class RecordActions
    {
        [Action("List all records", Description = "List all records")]
        public ListRecordsResponse ListRecords(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            [ActionParameter] ListRecordsRequest input)
        {
            var client = new AirtableClient(authenticationCredentialsProviders);
            var request = new AirtableRequest($"/v0/{input.BaseId}/{input.TableId}",
                Method.Get, authenticationCredentialsProviders);
            return client.Get<ListRecordsResponse>(request);
        }
    }
}
