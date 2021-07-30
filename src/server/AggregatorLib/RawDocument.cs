using LiteDB;
using NodaTime;
using System;

namespace AggregatorLib
{
    /**
     * Represents a document information in a form close to that in which it was downloaded.
     * 
     */
    public class RawDocument
    {
        [BsonId]
        public Guid Id { get; protected set; }
        public string DocumentUri { get; protected set; }
        public string? SourceId { get; protected set; }
        public string? ParentDocumentUri { get; protected set; }
        public Instant RetrieveTime { get; protected set; }
        public Instant? UpdateTime { get; protected set; }
        public Instant? PublishTime { get; protected set; }

        public RawDocumentContent Content { get; protected set; }

        public RawDocumentAuthor Author { get; protected set; }

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected RawDocument() { }
#pragma warning restore CS8618

        public RawDocument
        (
            Guid Id,
            string DocumentUri,
            string? SourceId,
            string? ParentDocumentUri,
            Instant RetrieveTime,
            Instant? UpdateTime,
            Instant? PublishTime,
            RawDocumentContent Content,
            RawDocumentAuthor Author
        )
        {
            this.Id = Id;
            this.DocumentUri = DocumentUri;
            this.SourceId = SourceId;
            this.ParentDocumentUri = ParentDocumentUri;
            this.RetrieveTime = RetrieveTime;
            this.UpdateTime = UpdateTime;
            this.PublishTime = PublishTime;
            this.Content = Content;
            this.Author = Author;
        }
    }
}