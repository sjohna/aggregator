using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AggregatorLib
{
    public class WordpressContent : RawDocumentContent
    {
        public string Title { get; protected set; }
        public string Content { get; protected set; }
        public IReadOnlyList<string> Categories { get; protected set; }

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected WordpressContent() { }
#pragma warning restore CS8618

        public WordpressContent(string Title, string Content, IReadOnlyList<string> Categories) 
        {
            this.Title = Title;
            this.Content = Content;
            this.Categories = Categories.ToList();
        }
    }
}