using Apps.Airtable.DataSourceHandlers.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Airtable.Models.Requests
{
    public class WebhookConfigRequest
    {
        [Display("Data types")]
        [StaticDataSource(typeof(DataTypeDataHandler))]
        public List<string> DataTypes { get; set; }

        [Display("Change types")]
        [StaticDataSource(typeof(ChangeTypeDataHandler))]
        public List<string> ChangeTypes { get; set; }

        [Display("From sources")]
        [StaticDataSource(typeof(FromSourcesDataHandler))]
        public List<string>? FromSources { get; set; }

        public string? WebhookSite { get; set; }
    }
}
