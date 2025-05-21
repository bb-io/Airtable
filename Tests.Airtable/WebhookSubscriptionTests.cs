using Apps.Airtable;
using Apps.Airtable.DataSourceHandlers;
using Apps.Airtable.Dtos;
using Apps.Airtable.Models.Identifiers;
using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tests.Airtable.Base;

namespace Tests.Airtable;

[TestClass]
public class WebhookSubscriptionTests : TestBase
{
    private static string url = "https://webhook.site/072f72ce-be84-4afe-af9d-1a5ad6ffda80";
    private static string tableId = "tblcoiOOt2k67kTHF";

    [TestMethod]
    public async Task Subscribe()
    {
        var _client = new AirtableClient(InvocationContext.AuthenticationCredentialsProviders, new AirtableWebhookUrlBuilder());
        var request = new AirtableRequest("", Method.Post, InvocationContext.AuthenticationCredentialsProviders);

        var serializedPayload = JsonConvert.SerializeObject(new
        {
            notificationUrl = url,
            specification = new
            {
                options = new
                {
                    filters = new
                    {
                        recordChangeScope = tableId,
                        dataTypes = new[] { "tableData" },
                        changeTypes = new[] { "update" }
                    }
                }
            }
        },
        new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        request.AddStringBody(serializedPayload, DataFormat.Json);

        var createdWebhook = await _client.ExecuteWithErrorHandling<CreatedWebhookDto>(request);

        Console.WriteLine(JsonConvert.SerializeObject(createdWebhook, Formatting.Indented));
    }

    [TestMethod]
    public async Task ProcessIncomingWebhook()
    {
        const string webhookId = "achXbXJO96b4KY7JP";
        var client = new AirtableClient(InvocationContext.AuthenticationCredentialsProviders, new AirtableWebhookUrlBuilder());
        var getDataRequest = new AirtableRequest($"/{webhookId}/payloads?cursor=1", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var getDataResponse = await client.ExecuteWithErrorHandling(getDataRequest);
        Console.WriteLine(getDataResponse.Content);
    }
}
