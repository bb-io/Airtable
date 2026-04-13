using Apps.Airtable.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Airtable.Models.Identifiers;

public class ViewIdentifier
{
    [Display("View")] 
    [DataSource(typeof(ViewDataSourceHandler))]
    public string? View { get; set; }
}