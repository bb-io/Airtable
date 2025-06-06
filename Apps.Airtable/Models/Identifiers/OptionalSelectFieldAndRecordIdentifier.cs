﻿using Apps.Airtable.DataSourceHandlers;
using Apps.Airtable.DataSourceHandlers.Record;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Airtable.Models.Identifiers;

public class OptionalSelectFieldAndRecordIdentifier
{
    [Display("Table ID")] 
    [DataSource(typeof(TableDataSourceHandler))]
    public string TableId { get; set; }

    [Display("Field ID")]
    [DataSource(typeof(SelectFieldDataSourceHandler))]
    public string FieldId { get; set; }

    [Display("Record ID")]
    [DataSource(typeof(RecordFieldDataHandler))]
    public string? RecordId { get; set; }   

}