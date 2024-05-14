using Apps.Airtable.UrlBuilders;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Airtable.Invocables;

public class AirtableInvocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected AirtableClient ContentClient { get; }
    protected AirtableClient MetaClient { get; }
    
    public AirtableInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        ContentClient = new(Creds, new AirtableContentUrlBuilder());
        MetaClient = new (Creds, new AirtableMetaUrlBuilder());
    }
}