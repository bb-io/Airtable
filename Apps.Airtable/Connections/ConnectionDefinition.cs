using Apps.Airtable.Constants;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.Airtable.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = ConnectionTypes.OAuth2,
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionProperties =
            [
                new(CredsNames.BaseId)
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
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        var providers = values
             .Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value))
             .ToList();

        if (!values.TryGetValue(nameof(ConnectionPropertyGroup), out var connectionType))
            throw new ArgumentException($"Missing connection type key: {nameof(ConnectionPropertyGroup)}");

        if (!ConnectionTypes.SupportedConnectionTypes.Contains(connectionType))
            throw new ArgumentException($"Unknown connection type: {connectionType}");

        providers.Add(new AuthenticationCredentialsProvider(CredsNames.ConnectionType, connectionType));

        if (!values.TryGetValue(CredsNames.BaseId, out var baseId))
            throw new ArgumentException($"Missing base ID key: {CredsNames.BaseId}");

        providers.Add(new AuthenticationCredentialsProvider(CredsNames.BaseId, baseId));

        if (!values.TryGetValue("access_token", out var token) && !values.TryGetValue(CredsNames.PersonalAccessToken, out token))
            throw new ArgumentException("Access token or personal access token was not found");

        providers.Add(new AuthenticationCredentialsProvider("Authorization", $"Bearer {token}"));

        return providers;
    }
}