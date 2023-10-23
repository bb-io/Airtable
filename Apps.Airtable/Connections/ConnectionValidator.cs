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
        var client = new AirtableClient();
        var request = new AirtableRequest("/meta/whoami", Method.Get, authenticationCredentialsProviders);
        
        try
        {
            await client.ExecuteWithErrorHandling(request);
            return new ConnectionValidationResponse
            {
                IsValid = true,
                Message = "Success"
            };
        }
        catch (Exception)
        {
            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = "Ping failed"
            };
        }
    }
}