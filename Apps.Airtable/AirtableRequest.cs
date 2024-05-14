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
        this.AddHeader("Authorization", authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value);
    }
}