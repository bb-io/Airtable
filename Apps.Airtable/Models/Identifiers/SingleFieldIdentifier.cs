using Apps.Airtable.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable.Models.Identifiers
{
    public class SingleFieldIdentifier
    {
        [Display("Table ID")]
        [DataSource(typeof(TableDataSourceHandler))]
        public string TableId { get; set; }

        [Display("Field ID")]
        [DataSource(typeof(SingleFieldDataSourceHandler))]
        public string FieldId { get; set; }
    }
}
