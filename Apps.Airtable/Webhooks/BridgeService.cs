using System.Net;
using RestSharp;

namespace Apps.Airtable.Webhooks;

public class BridgeService
{
    private const string AppName = ApplicationConstants.AppName;
    
    private readonly RestClient _bridgeClient;

    public BridgeService()
    {
        _bridgeClient = new RestClient(new RestClientOptions(ApplicationConstants.BridgeServiceUrl));
    }
    
    public async Task StoreValue(string key, string value)
    {
        var storeValueRequest = CreateBridgeRequest($"/storage/{AppName}/{key}", Method.Post);
        storeValueRequest.AddBody(value);
        await _bridgeClient.ExecuteAsync(storeValueRequest);
    }
    
    public async Task<string?> RetrieveValue(string key)
    {
        var retrieveValueRequest = CreateBridgeRequest($"/storage/{AppName}/{key}", Method.Get);
        var result = await _bridgeClient.ExecuteAsync(retrieveValueRequest);

        if (result.StatusCode == HttpStatusCode.NotFound)
            return null;
        
        return result.Content.Trim('"');
    }
    
    public async Task DeleteValue(string key)
    {
        var deleteValueRequest = CreateBridgeRequest($"/storage/{AppName}/{key}", Method.Delete);
        await _bridgeClient.ExecuteAsync(deleteValueRequest);
    }

    private RestRequest CreateBridgeRequest(string endpoint, Method method)
    {
        var request = new RestRequest(endpoint, method);
        request.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
        return request;
    }
}