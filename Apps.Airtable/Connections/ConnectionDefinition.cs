using Apps.Airtable.Constants;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.Airtable.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups =>
    [
        new()
        {
            Name = ConnectionTypes.OAuth2,
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionProperties =
            [
                new("Base ID") { DisplayName = "Base ID" }
            ]
        },
        new()
        {
            Name = ConnectionTypes.PersonalAccessToken,
            AuthenticationType= ConnectionAuthenticationType.Undefined,
            ConnectionProperties = 
            [
                new(CredsNames.PersonalAccessToken) { DisplayName = "Personal access token", Sensitive = true },
                new(CredsNames.BaseId) { DisplayName = "Base ID" },
            ]
        }
    ];

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        WebhookLogger.Log(values);
        string? token = 
            values.FirstOrDefault(v => v.Key == "access_token").Value ??
            values.FirstOrDefault(v => v.Key == CredsNames.PersonalAccessToken).Value;

        if (!string.IsNullOrEmpty(token))
            yield return new("Authorization", $"Bearer {token}");

        string baseId = values.First(v => v.Key == "Base ID").Value;
        yield return new(CredsNames.BaseId, baseId);
    }
}