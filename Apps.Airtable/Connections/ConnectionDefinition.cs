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
            Name = "OAuth2",
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredsNames.BaseId)
            }
        },
        new()
        {
            Name = "Personal access token",
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
        var token = values.First(v => v.Key == "access_token").Value;
        yield return new(
            "Authorization",
            $"Bearer {token}"
        );
        
        var baseId = values.First(v => v.Key == CredsNames.BaseId).Value;
        yield return new(
            "BaseId",
            baseId
        );
    }
}