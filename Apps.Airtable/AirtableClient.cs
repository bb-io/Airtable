using Apps.Airtable.Dtos;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Airtable;

public class AirtableClient : BlackBirdRestClient
{
    public AirtableClient() : base(new()
        { ThrowOnAnyError = false, BaseUrl = new Uri("https://api.airtable.com/v0") }) { }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var error = JsonConvert.DeserializeObject<ErrorDtoWrapper>(response.Content);
        return new(error.Error.Message);
    }
}