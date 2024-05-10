using Apps.Airtable.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Airtable.DataSourceHandlers.Record;

public class PrimeRecordDataHandler : RecordDataSourceHandler
{
    
    public PrimeRecordDataHandler(InvocationContext invocationContext, [ActionParameter] RecordIdentifier record) :
        base(invocationContext, record.TableId)
    {
    }
}