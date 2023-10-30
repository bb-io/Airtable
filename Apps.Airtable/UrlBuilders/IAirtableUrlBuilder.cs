using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.Airtable.UrlBuilders;

public interface IAirtableUrlBuilder
{
    Uri BuildUrl(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders);
}