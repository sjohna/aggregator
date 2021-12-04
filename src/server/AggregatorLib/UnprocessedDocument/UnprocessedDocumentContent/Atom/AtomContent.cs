using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AggregatorLib
{
    public class AtomContent : UnprocessedDocumentContent
    {
        public string Title { get; protected set; }
        public string Content { get; protected set; }
        public IReadOnlyList<AtomCategory> Categories { get; protected set; }
        public IReadOnlyList<AtomLink> Links { get; protected set; }
        public override string ContentType => "Atom";

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected AtomContent() { }
#pragma warning restore CS8618

        public AtomContent(string Title, string Content, IReadOnlyList<AtomCategory> Categories, IReadOnlyList<AtomLink> Links) 
        {
            this.Title = Title;
            this.Content = Content;
            this.Categories = Categories.ToList();
            this.Links = Links.ToList();
        }
    }
}