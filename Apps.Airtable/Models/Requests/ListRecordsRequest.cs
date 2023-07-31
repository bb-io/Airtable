using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable.Models.Requests
{
    public class ListRecordsRequest
    {
        [Display("Base ID")]
        public string BaseId { get; set; }

        [Display("Table ID (or name)")]
        public string TableId { get; set; }
    }
}
