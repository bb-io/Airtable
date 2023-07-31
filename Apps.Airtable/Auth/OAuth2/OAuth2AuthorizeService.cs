using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace Apps.Airtable.Authorization.OAuth2
{
    public class OAuth2AuthorizeService : IOAuth2AuthorizeService
    {
        public string GetAuthorizationUrl(Dictionary<string, string> values)
        {
            string oauthUrl = "https://airtable.com/oauth2/v1/authorize";

            string sha256CodeVerifierStr = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] sha256CodeVerifier = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(ApplicationConstants.CodeVerifier));
                sha256CodeVerifierStr = Convert.ToBase64String(sha256CodeVerifier).Replace("=", "").Replace("+", "-").Replace("/", "_");
            }

            var parameters = new Dictionary<string, string>
            {
                { "client_id", ApplicationConstants.ClientId},
                { "redirect_uri", ApplicationConstants.RedirectUri },
                { "response_type", "code"},
                { "state", values["state"] },
                { "scope", ApplicationConstants.Scope },
                { "code_challenge", sha256CodeVerifierStr},
                { "code_challenge_method", "S256"}
            };
            return QueryHelpers.AddQueryString(oauthUrl, parameters);
        }
    }
}
