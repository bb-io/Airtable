using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using RestSharp;

namespace Apps.Airtable;

public class AirtableRequest : BlackBirdRestRequest
{
    public AirtableRequest(string endpoint, Method method, 
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
        : base(endpoint, method, authenticationCredentialsProviders) { }

    protected override void AddAuth(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        // TODO REVERT
        this.AddHeader("Authorization", "Bearer pat2YL7eRLjkpj7RX.75d6f290143c6b93130bf2e67b18b8dae4900f4cc95e0f1b6864c90d9e716731");
        // this.AddHeader("Authorization", authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value);
    }
}