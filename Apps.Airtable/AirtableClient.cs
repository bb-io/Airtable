using Apps.Airtable.Dtos;
using Apps.Airtable.Models.Responses;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
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
        : base(new() { ThrowOnAnyError = false, BaseUrl = urlBuilder.BuildUrl(authenticationCredentialsProviders) })
    {
    }

    public async Task<List<TV>> Paginate<T, TV>(RestRequest request) where T : PaginationResponse<TV>
    {
        var baseUrl = request.Resource;
        int? offset = 0;
        const int pageSize = 100;

        var result = new List<TV>();

        do
        {
            request.Resource = baseUrl
                .SetQueryParameter("pageSize", pageSize.ToString())
                .SetQueryParameter("offset", offset.ToString());

            var response = await ExecuteWithErrorHandling<T>(request);

            result.AddRange(response.Items);
            offset = response.Offset;
        } while (offset != null);

        return result;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        try
        {
            var error = JsonConvert.DeserializeObject<ErrorObjectDtoWrapper>(response.Content, JsonSettings);
            return new PluginApplicationException(error.Error.Message);
        }
        catch (JsonSerializationException)
        {
            var error = JsonConvert.DeserializeObject<ErrorStringDto>(response.Content, JsonSettings);
            return new PluginApplicationException(error.Error);
        }
    }
}