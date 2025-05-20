using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable.Webhooks.Responses;
public class UpdatedFieldResponse
{
    [Display("Changed records")]
    public List<ChangedRecord> ChangedRecords { get; set; }
}

public class ChangedRecord
{
    [Display("Field ID")]
    public string FieldId { get; set; }

    [Display("Record ID")]
    public string RecordId { get; set; }

    [Display("Previous value")]
    public string PreviousValue { get; set; }

    [Display("New value")]
    public string NewValue { get; set; }
}
