using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.String;

namespace Apps.Airtable.Auth.OAuth2;

public class OAuth2AuthorizeService : BaseInvocable, IOAuth2AuthorizeService
{
    public OAuth2AuthorizeService(InvocationContext invocationContext) : base(invocationContext)
    {
    }
    
    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {
        const string oauthUrl = "https://airtable.com/oauth2/v1/authorize";
        //var bridgeOauthUrl = $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/oauth";
        var bridgeOauthUrl = $"https://bridge.blackbird.io/api/oauth";
        var parameters = new Dictionary<string, string>
        {
            { "client_id", ApplicationConstants.ClientId},
            //{ "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
            { "redirect_uri", $"https://bridge.blackbird.io/api/AuthorizationCode" },
            { "response_type", "code"},
            { "state", values["state"] },
            { "scope", ApplicationConstants.Scope },
            { "code_challenge", ApplicationConstants.CodeChallenge},
            { "code_challenge_method", "S256"},
            { "authorization_url", oauthUrl},
            { "actual_redirect_uri", InvocationContext.UriInfo.AuthorizationCodeRedirectUri.ToString() }
        };
        return bridgeOauthUrl.WithQuery(parameters);
    }
}
