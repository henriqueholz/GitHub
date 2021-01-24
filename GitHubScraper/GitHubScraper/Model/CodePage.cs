using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubScraper.Model
{
    public class CodePage
    {
        public string FileExtension { get; set; }
        public int Quantity { get; set; }
        public int TotalLines { get; set; }
        public Double TotalBytes { get; set; }
    }
}
