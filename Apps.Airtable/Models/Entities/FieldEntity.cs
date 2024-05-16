using Blackbird.Applications.Sdk.Common;

namespace Apps.Airtable.Models.Entities;

public class FieldEntity
{
    [Display("Field ID")]
    public string Id { get; set; }
    
    public string Value { get; set; }
}