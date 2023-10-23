using Apps.Airtable.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Airtable;

public abstract class AirtableClient : BlackBirdRestClient
{
    protected override JsonSerializerSettings JsonSettings =>
        new() { MissingMemberHandling = MissingMemberHandling.Ignore };

    protected AirtableClient(RestClientOptions options) : base(options) { }
    
    protected override Exception ConfigureErrorException(RestResponse response)
    {
        try
        {
            var error = JsonConvert.DeserializeObject<ErrorObjectDtoWrapper>(response.Content, JsonSettings);
            return new(error.Error.Message);
        }
        catch (JsonSerializationException)
        {
            var error = JsonConvert.DeserializeObject<ErrorStringDto>(response.Content, JsonSettings);
            return new(error.Error);
        }
    }
}

public class AirtableContentClient : AirtableClient 
{
    public AirtableContentClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
        : base(new() { ThrowOnAnyError = false, BaseUrl = GetBaseUrl(authenticationCredentialsProviders) }) { }

    private static Uri GetBaseUrl(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var baseId = authenticationCredentialsProviders.First(p => p.KeyName == "BaseId").Value;
        return new($"https://api.airtable.com/v0/{baseId}");
    }
}

public class AirtableMetaClient : AirtableClient 
{
    public AirtableMetaClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
        : base(new() { ThrowOnAnyError = false, BaseUrl = GetBaseUrl(authenticationCredentialsProviders) }) { }

    private static Uri GetBaseUrl(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var baseId = authenticationCredentialsProviders.First(p => p.KeyName == "BaseId").Value;
        return new($"https://api.airtable.com/v0/meta/bases/{baseId}");
    }
}