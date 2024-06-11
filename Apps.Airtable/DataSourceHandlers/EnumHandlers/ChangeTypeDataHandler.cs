using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Airtable.DataSourceHandlers.EnumHandlers
{
    public class ChangeTypeDataHandler : IStaticDataSourceHandler
    {
        public Dictionary<string, string> GetData()
        {
            return new() 
            {
                { "add", "On add" },
                { "update", "On update" },
                { "remove", "On remove" }
            };
        }
    }
}
