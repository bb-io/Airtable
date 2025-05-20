using Apps.Airtable.Actions;
using Apps.Airtable.Connections;
using Apps.Airtable.DataSourceHandlers;
using Apps.Airtable.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Airtable.Base;

namespace Tests.Airtable;

[TestClass]
public class DataSourceTests : TestBase
{
    [TestMethod]
    public async Task Field_Ids()
    {
        var handler = new FieldDataSourceHandler(InvocationContext, new FieldAndRecordIdentifier { TableId = "tblcoiOOt2k67kTHF" });
        var result = await handler.GetDataAsync(new DataSourceContext { }, CancellationToken.None);
        foreach(var record in result)
        {
            Console.WriteLine($"{record.Key}: {record.Value}");
        }
    }
}
