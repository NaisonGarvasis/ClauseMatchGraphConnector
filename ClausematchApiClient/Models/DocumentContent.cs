using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Models
{
    public class DocumentContent
    {
        public HeaderFooterSection? Header { get; set; }
        public HeaderFooterSection? Footer { get; set; }
        public List<BodySection>? Body { get; set; }
    }
}
