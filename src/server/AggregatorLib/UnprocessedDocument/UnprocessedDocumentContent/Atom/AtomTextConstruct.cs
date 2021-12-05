using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    /**
     * Represents content within an Atom entry/feed, such as entry titles or content.
     */
    public class AtomTextConstruct : IEquatable<AtomTextConstruct>
    {
        public string Type { get; protected set; }
        public string Content { get; protected set; }

        public AtomTextConstruct(string Type, string Content)
        {
            this.Type = Type;
            this.Content = Content;
        }

        public bool Equals(AtomTextConstruct? other)
        {
            return this == other ||
                (other != null &&
                this.Type == other.Type &&
                this.Content == other.Content);
        }
    }
}
