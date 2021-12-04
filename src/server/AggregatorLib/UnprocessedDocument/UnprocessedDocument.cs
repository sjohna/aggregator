using LiteDB;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AggregatorLib
{
    public enum UnprocessedDocumentType
    {
        Regular = 1,
        SourceDescription = 2,
        AuthorDescription = 3
    }

    /**
     * Represents a document in a form close to that in which it was downloaded.
     * 
     */
    public class UnprocessedDocument
    {
        [BsonId]
        public Guid Id { get; protected set; }
        public string Uri { get; protected set; }
        public string? SourceId { get; protected set; }
        public Instant RetrieveTime { get; protected set; }
        // TODO: test how converter this interacts with null in the nullable field
        public Instant? UpdateTime { get; protected set; }
        public Instant? PublishTime { get; protected set; }
        public UnprocessedDocumentContent Content { get; protected set; }
        public IReadOnlyList<UnprocessedDocumentAuthor> Authors { get; protected set; }
        public Guid? SourceRawContentId { get; protected set; }
        public UnprocessedDocumentType DocumentType { get; protected set; }

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected UnprocessedDocument() { }
#pragma warning restore CS8618

        public UnprocessedDocument
        (
            Guid Id,
            string Uri,
            string? SourceId,
            Instant RetrieveTime,
            Instant? UpdateTime,
            Instant? PublishTime,
            UnprocessedDocumentContent Content,
            IReadOnlyList<UnprocessedDocumentAuthor> Authors,
            Guid? SourceRawContentId,
            UnprocessedDocumentType DocumentType = UnprocessedDocumentType.Regular
        )
        {
            this.Id = Id;
            this.Uri = Uri;
            this.SourceId = SourceId;
            this.RetrieveTime = RetrieveTime;
            this.UpdateTime = UpdateTime;
            this.PublishTime = PublishTime;
            this.Content = Content;
            this.Authors = Authors;
            this.SourceRawContentId = SourceRawContentId;
            this.DocumentType = DocumentType;
        }
    }
}