using Apps.Airtable.Models.Identifiers;
using Apps.Airtable.Models.Requests;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable.Webhooks.Handlers;
public class OnFieldUpdatedHandler : BaseWebhookHandler
{
    public OnFieldUpdatedHandler(InvocationContext invocationContext, [WebhookParameter(true)] OptionalFieldAndRecordIdentifier table) : base(invocationContext, new TableIdentifier { TableId = table.TableId }, new WebhookConfigRequest { ChangeType = "update", DataType = "tableData" })
    {
        
    }
}
