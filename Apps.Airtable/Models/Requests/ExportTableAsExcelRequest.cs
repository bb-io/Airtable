using Apps.Airtable.DataSourceHandlers;
using Apps.Airtable.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Airtable.Models.Requests;

public class ExportTableAsExcelRequest : TableIdentifier
{
    [Display("View")]
    [DataSource(typeof(ViewDataSourceHandler))]
    public string? View { get; set; }

    [Display("Fields")]
    [DataSource(typeof(ExportFieldDataSourceHandler))]
    public IEnumerable<string>? Fields { get; set; }
}
