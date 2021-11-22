using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class AtomCategory : IEquatable<AtomCategory>
    {
        public string? Name { get; protected set; }

        public string? Scheme { get; protected set; }

        public string? Label { get; protected set; }

        public AtomCategory(string? Name, string? Scheme = null, string? Label = null)
        {
            this.Name = Name;
            this.Scheme = Scheme;
            this.Label = Label;
        }

        public bool Equals(AtomCategory? other)
        {
            return this == other ||
                (other != null &&
                this.Name == other.Name &&
                this.Scheme == other.Scheme &&
                this.Label == other.Label);
        }
    }
}
