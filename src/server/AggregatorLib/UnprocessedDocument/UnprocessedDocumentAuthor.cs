﻿namespace AggregatorLib
{
    public class UnprocessedDocumentAuthor
    {
        public string Name { get; protected set; }
        public string Context { get; protected set; }  // e.g. Wordpress blogger, youtube channel, reddit username, etc.
        public string? Uri { get; protected set; }

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected UnprocessedDocumentAuthor() { }
#pragma warning restore CS8618

        public UnprocessedDocumentAuthor(string Name, string Context, string? Uri = null)
        {
            this.Name = Name;
            this.Context = Context;
            this.Uri = Uri;
        }
    }
}
