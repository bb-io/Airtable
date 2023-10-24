using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Utils.Extensions.String;

namespace Apps.Airtable.Auth.OAuth2;

public class OAuth2AuthorizeService : IOAuth2AuthorizeService
{
    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {
        const string oauthUrl = "https://airtable.com/oauth2/v1/authorize";
        var parameters = new Dictionary<string, string>
        {
            { "client_id", ApplicationConstants.ClientId},
            { "redirect_uri", ApplicationConstants.RedirectUri },
            { "response_type", "code"},
            { "state", values["state"] },
            { "scope", ApplicationConstants.Scope },
            { "code_challenge", ApplicationConstants.CodeChallenge},
            { "code_challenge_method", "S256"}
        };
        return oauthUrl.WithQuery(parameters);
    }
}
