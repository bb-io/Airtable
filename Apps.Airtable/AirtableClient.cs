using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable
{
    public class AirtableClient : RestClient
    {
        public AirtableClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : 
            base(new RestClientOptions() { ThrowOnAnyError = true, BaseUrl = new Uri("https://api.airtable.com") }) { }

    }
}
