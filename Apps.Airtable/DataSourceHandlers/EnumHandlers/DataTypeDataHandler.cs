using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Airtable.DataSourceHandlers.EnumHandlers
{
    public class DataTypeDataHandler : IStaticDataSourceHandler
    {
        public Dictionary<string, string> GetData()
        {
            return new()
            {
                { "tableData", "Table data" },
                { "tableFields", "Table fields" },
                { "tableMetadata", "Table metadata" }
            };
        }
    }
}
