using Apps.Airtable.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Airtable.Models.Responses
{
    public class ListRecordsResponse
    {
        public IEnumerable<RecordDto> Records { get; set; }
    }
}
