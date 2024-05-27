using Apps.Airtable.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Airtable.DataSourceHandlers.Record;

public class RecordFieldDataHandler : RecordDataSourceHandler
{
    public RecordFieldDataHandler(InvocationContext invocationContext, [ActionParameter] FieldAndRecordIdentifier field) : base(
        invocationContext, field.TableId)
    {
    }
}