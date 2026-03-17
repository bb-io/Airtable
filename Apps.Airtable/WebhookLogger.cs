using System.Text;
using Newtonsoft.Json;

namespace Apps.Airtable;

public class WebhookLogger
{
    private const string Url = "https://webhook.site/#!/view/087b2e13-6625-4986-be22-45e97cd11352"; 
    private static readonly HttpClient _client = new();

    public static void Log(object message)
    {
        var json = JsonConvert.SerializeObject(message);

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, Url) { Content = content };

        _client.Send(request);
    }
}
