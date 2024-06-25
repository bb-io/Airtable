using Apps.Airtable.DataSourceHandlers.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Airtable.Models.Requests
{
    public class WebhookConfigRequest
    {
        [Display("Data type")]
        [StaticDataSource(typeof(DataTypeDataHandler))]
        public string DataType { get; set; }

        [Display("Change type")]
        [StaticDataSource(typeof(ChangeTypeDataHandler))]
        public string ChangeType { get; set; }

        [Display("From sources")]
        [StaticDataSource(typeof(FromSourcesDataHandler))]
        public List<string>? FromSources { get; set; }
    }
}
