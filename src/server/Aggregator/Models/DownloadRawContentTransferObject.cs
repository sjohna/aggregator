using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aggregator.Models
{
    public class DownloadRawContentTransferObject
    {
        public string Context { get; set; }
        public string Type { get; set; }
        public string? SourceUri { get; set; }
    }
}
