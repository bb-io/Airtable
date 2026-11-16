using Apps.Airtable.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Airtable.DataSourceHandlers.EnumHandlers;

public class ExportFormatDataHandler : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData()
    {
        return new()
        {
            { ExportFormats.Excel, "Excel" },
            { ExportFormats.Json, "JSON" }
        };
    }
}
