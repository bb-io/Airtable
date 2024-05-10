using Apps.Airtable.DataSourceHandlers;
using Apps.Airtable.DataSourceHandlers.Record;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Airtable.Models.Identifiers;

public class RecordIdentifier
{
    [Display("Table ID")] 
    [DataSource(typeof(TableDataSourceHandler))]
    public string TableId { get; set; }
    
    [Display("Record ID")]
    [DataSource(typeof(PrimeRecordDataHandler))]
    public string RecordId { get; set; }
}