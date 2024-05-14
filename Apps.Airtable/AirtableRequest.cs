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
        this.AddHeader("Authorization", "Bearer patZ3gvhcLiSoUtn4.bec67bffbf4171e043e85e11b6b4ebd603c4aa8a86b3b02383d8f178901f16e4");
        // this.AddHeader("Authorization", authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value);
    }
}