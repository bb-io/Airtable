using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.Airtable.UrlBuilders;

public class AirtableWebhookUrlBuilder : IAirtableUrlBuilder
{
    public Uri BuildUrl(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var baseId = authenticationCredentialsProviders.First(p => p.KeyName == "BaseId").Value;
        return new($"https://api.airtable.com/v0/bases/{baseId}/webhooks");
    }
}