using Apps.Airtable.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Airtable.Models.Identifiers;

public class UniqueFieldIdentifier
{
    [Display("Table ID")]
    [DataSource(typeof(TableDataSourceHandler))]
    public string TableId { get; set; }

    [Display("Unique key field")]
    [DataSource(typeof(UniqueFieldDataSourceHandler))]
    public string FieldId { get; set; }
}
