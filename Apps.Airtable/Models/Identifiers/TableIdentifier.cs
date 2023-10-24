using Apps.Airtable.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Airtable.Models.Identifiers;

public class TableIdentifier
{
    [Display("Table")] 
    [DataSource(typeof(TableDataSourceHandler))]
    public string TableId { get; set; }
}