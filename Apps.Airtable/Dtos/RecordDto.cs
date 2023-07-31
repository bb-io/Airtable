using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable.Dtos
{
    public class RecordDto
    {
        [Display("Created time")]
        public DateTime CreatedTime { get; set; }

        [Display("Record ID")]
        public string Id { get; set; }
    }
}
