using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    /**
     * Content that describes a feed source.
     */
    public class FeedSourceDescriptionContent : UnprocessedDocumentContent, IEquatable<FeedSourceDescriptionContent>
    {
        public string Title { get; protected set; }
        public string? Description { get; protected set; }
        public string? IconUri { get; protected set; }

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected FeedSourceDescriptionContent() { }
#pragma warning restore CS8618

        public FeedSourceDescriptionContent(string Title, string? Description, string? IconUri)
        {
            this.Title = Title;
            this.Description = Description;
            this.IconUri = IconUri;
        }

        public override bool Equals(object? other)
        {
            if (other is FeedSourceDescriptionContent rightType)
            {
                return this.Equals(rightType);
            }

            return false;
        }

        public bool Equals(FeedSourceDescriptionContent? other)
        {
            if (other == null) return false;

            return this.Title == other.Title
                   && this.Description == other.Description
                   && this.IconUri == other.IconUri;
        }
    }
}
