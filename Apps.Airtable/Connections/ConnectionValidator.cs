using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.Airtable.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        var client = new AirtableClient(authenticationCredentialsProviders, new AirtableMetaUrlBuilder());
        var request = new AirtableRequest("/tables", Method.Get, authenticationCredentialsProviders);
        
        try
        {
            await client.ExecuteWithErrorHandling(request);
            return new()
            {
                IsValid = true,
                Message = "Success"
            };
        }
        catch (Exception)
        {
            return new()
            {
                IsValid = false,
                Message = "Please enter correct Base ID value."
            };
        }
    }
}