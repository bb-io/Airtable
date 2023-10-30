using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.Airtable.UrlBuilders;

public class AirtableContentUrlBuilder : IAirtableUrlBuilder
{
    public Uri BuildUrl(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var baseId = authenticationCredentialsProviders.First(p => p.KeyName == "BaseId").Value;
        return new($"https://api.airtable.com/v0/{baseId}");
    }
}