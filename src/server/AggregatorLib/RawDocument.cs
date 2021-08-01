﻿using LiteDB;
using NodaTime;
using System;
using System.Collections.Generic;

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
        public string Uri { get; protected set; }
        public string? SourceId { get; protected set; }
        public string? ParentDocumentUri { get; protected set; }
        public Instant RetrieveTime { get; protected set; }
        public Instant? UpdateTime { get; protected set; }
        public Instant? PublishTime { get; protected set; }

        public RawDocumentContent Content { get; protected set; }

        public IReadOnlyList<RawDocumentAuthor> Authors { get; protected set; }

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected RawDocument() { }
#pragma warning restore CS8618

        public RawDocument
        (
            Guid Id,
            string Uri,
            string? SourceId,
            string? ParentDocumentUri,
            Instant RetrieveTime,
            Instant? UpdateTime,
            Instant? PublishTime,
            RawDocumentContent Content,
            IReadOnlyList<RawDocumentAuthor> Authors
        )
        {
            this.Id = Id;
            this.Uri = Uri;
            this.SourceId = SourceId;
            this.ParentDocumentUri = ParentDocumentUri;
            this.RetrieveTime = RetrieveTime;
            this.UpdateTime = UpdateTime;
            this.PublishTime = PublishTime;
            this.Content = Content;
            this.Authors = Authors;
        }
    }
}