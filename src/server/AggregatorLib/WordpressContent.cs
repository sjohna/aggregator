using System.Collections.Generic;

namespace AggregatorLib
{
    public class WordpressContent
    {
        public string Content { get; }
        public IReadOnlyCollection<string> Categories { get; }
    }
}
