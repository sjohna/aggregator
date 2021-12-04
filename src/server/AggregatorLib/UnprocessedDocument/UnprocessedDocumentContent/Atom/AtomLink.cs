using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class AtomLink : IEquatable<AtomLink>
    {
        public string Href { get; protected set; }
        public string? Rel { get; protected set; }
        public string? Type { get; protected set; }

        public AtomLink(string Href, string? Rel = null, string? Type = null)
        {
            this.Href = Href;
            this.Rel = Rel;
            this.Type = Type;
        }

        public bool Equals(AtomLink? other)
        {
            return this == other ||
                (other != null &&
                this.Href == other.Href &&
                this.Rel == other.Rel &&
                this.Type == other.Type);
        }
    }
}
