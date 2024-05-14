using Apps.Airtable.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable.DataSourceHandlers.Record
{
    public class PrimeRecordDataHandler : RecordDataSourceHandler
    {
        public PrimeRecordDataHandler(InvocationContext invocationContext, [ActionParameter] RecordIdentifier record) :
            base(invocationContext, record.TableId)
        {
        }
    }
}
