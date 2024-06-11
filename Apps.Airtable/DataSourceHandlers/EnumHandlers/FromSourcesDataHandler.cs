using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Airtable.DataSourceHandlers.EnumHandlers
{
    public class FromSourcesDataHandler : IStaticDataSourceHandler
    {
        public Dictionary<string, string> GetData()
        {
            return new()
            {
                { "client", "Client" },
                { "publicApi", "Public api" },
                { "formSubmission", "Form submission" },
                { "formPageSubmission", "Form page submission" },
                { "automation", "Automation" },
                { "system", "System" },
                { "sync", "Sync" },
                { "anonymousUser", "Anonymous user" },
            };
        }
    }
}
