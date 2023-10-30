using Apps.Airtable.Dtos;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Airtable;

public class AirtableClient : BlackBirdRestClient
{
    protected override JsonSerializerSettings JsonSettings =>
        new() { MissingMemberHandling = MissingMemberHandling.Ignore };

    public AirtableClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        IAirtableUrlBuilder urlBuilder)
        : base(new RestClientOptions
            { ThrowOnAnyError = false, BaseUrl = urlBuilder.BuildUrl(authenticationCredentialsProviders) })
    { }

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