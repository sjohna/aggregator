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
        public Guid Id { get; }
        public string? ParentDocumentUri { get; }
        public string DocumentUri { get; }
        public string Title { get; }
        public Instant RetrieveTime { get; }
        public Instant? UpdateTime { get; }
        public Instant? PublishTime { get; }

        public RawDocumentContent Content { get; }

        public RawDocumentAuthor Author { get; }
    }
}