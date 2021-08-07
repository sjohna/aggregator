using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AggregatorLib
{
    public class BlogPostContent : RawDocumentContent
    {
        public string Title { get; protected set; }
        public string Content { get; protected set; }
        public IReadOnlyList<string> Categories { get; protected set; }
        public bool AllowsComments { get; protected set; }
        public string? CommentUri { get; protected set; }
        public string? CommentFeedUri { get; protected set; }

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected BlogPostContent() { }
#pragma warning restore CS8618

        public BlogPostContent(string Title, string Content, IReadOnlyList<string> Categories, bool AllowsComments, string? CommentUri, string? CommentFeedUri) 
        {
            this.Title = Title;
            this.Content = Content;
            this.Categories = Categories.ToList();
            this.AllowsComments = AllowsComments;
            this.CommentUri = CommentUri;
            this.CommentFeedUri = CommentFeedUri;
        }
    }
}