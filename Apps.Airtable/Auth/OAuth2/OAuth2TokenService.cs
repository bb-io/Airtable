using Apps.Airtable.Actions;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Apps.Airtable.Authorization.OAuth2
{
    public class OAuth2TokenService : IOAuth2TokenService
    {
        private static string TokenUrl = "";

        public bool IsRefreshToken(Dictionary<string, string> values)
        {
            return false;
        }

        public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, string?>> RequestToken(
            string state, 
            string code, 
            Dictionary<string, string> values, 
            CancellationToken cancellationToken)
        {
            TokenUrl = "https://airtable.com/oauth2/v1/token";
            const string grant_type = "authorization_code";
            var bodyParameters = new Dictionary<string, string>
            {
                { "client_id", ApplicationConstants.ClientId },
                { "grant_type", grant_type },
                { "redirect_uri", ApplicationConstants.RedirectUri },
                { "code_verifier", ApplicationConstants.CodeVerifier },
                { "code", code }
            };
            return await RequestToken(bodyParameters, cancellationToken);
        }

        public Task RevokeToken(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }

        private async Task<Dictionary<string, string>> RequestToken(Dictionary<string, string> bodyParameters, CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;
            using HttpClient httpClient = new HttpClient();
            using var httpContent = new FormUrlEncodedContent(bodyParameters);
            using var response = await httpClient.PostAsync(TokenUrl, httpContent, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)?.ToDictionary(r => r.Key, r => r.Value?.ToString())
                ?? throw new InvalidOperationException($"Invalid response content: {responseContent}");
            return resultDictionary;
        }
    }
}
