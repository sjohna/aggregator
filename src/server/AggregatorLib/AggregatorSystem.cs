using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class AggregatorSystem
    {
        public class ProcessedContentAdditions
        {
            public List<RawContent> RawContentAdditions { get; }

            public List<UnprocessedDocument> UnprocessedDocumentAdditions { get; }

            public ProcessedContentAdditions()
            {
                RawContentAdditions = new List<RawContent>();
                UnprocessedDocumentAdditions = new List<UnprocessedDocument>();
            }
        }

        // TODO: consider how to handle reads form the system
        public IRawContentRepository RawContentRepository { get; }
        public IUnprocessedDocumentRepository UnprocessedDocumentRepository { get; }

        public AggregatorSystem
        (
            IRawContentRepository RawContentRepository,
            IUnprocessedDocumentRepository UnprocessedDocumentRepository
        )
        {
            this.RawContentRepository = RawContentRepository;
            this.UnprocessedDocumentRepository = UnprocessedDocumentRepository;
        }

        public ProcessedContentAdditions ProcessRawContent(RawContent content) 
        {
            if (content.Type == "atom/xml")
            {
                return ProcessAtomContent(content);
            }
            else
            {
                throw new AggregatorSystemException($"Invalid content type: {content.Type}");
            }
        }

        // TODO: processing sequence ID...
        // TODO: settings to override title document processing, maybe others?
        // TODO: associate said settings with document in database
        // TODO: logging
        // TODO: add checks for if rawcontent is too big
        private ProcessedContentAdditions ProcessAtomContent(RawContent content)
        {
            // TODO SOON: give this function a refactor, or at least look at the variable names...
            var additions = new ProcessedContentAdditions();

            // validate content
            // TODO: handle race condition here with some sort of lock on content processing
            var contentInRepository = RawContentRepository.GetRawContentById(content.Id);
            if (contentInRepository != null) throw new AggregatorSystemException($"Already processed RawContent with Id {content.Id}");

            var extractor = new FeedExtractor(content.Content, content.RetrieveTime, content.Id);

            bool anyUnprocessedDocumentUpdates = false;

            var titleDocument = extractor.TitleDocument;
            if (titleDocument != null)
            {
                // check for title updates
                var existingTitleDocument = UnprocessedDocumentRepository.Query(
                    Where: $"SourceId = '{titleDocument.SourceId}' AND DocumentType = '{titleDocument.DocumentType}'",
                    OrderByDesc: "RetrieveTime",
                    Limit: 1).FirstOrDefault();

                if (existingTitleDocument == null)
                {
                    UnprocessedDocumentRepository.Add(titleDocument);
                    additions.UnprocessedDocumentAdditions.Add(titleDocument);
                    anyUnprocessedDocumentUpdates = true;
                }
                else
                {
                    var existingContent = existingTitleDocument.Content as FeedSourceDescriptionContent;
                    var currentContent = titleDocument.Content as FeedSourceDescriptionContent;

                    if (existingContent != null && currentContent != null && !existingContent.Equals(currentContent))
                    {
                        UnprocessedDocumentRepository.Add(titleDocument);
                        additions.UnprocessedDocumentAdditions.Add(titleDocument);
                        anyUnprocessedDocumentUpdates = true;
                    }
                }
            }

            foreach (var newDocument in extractor.RawDocuments)
            {
                var existingDocument = UnprocessedDocumentRepository.GetLatestForSourceId(newDocument.SourceId!);    // SourceId is definitely not null: FeedExtractor throws an exception if it can't populate

                // TODO: option for deep compare?
                if (existingDocument == null || existingDocument.UpdateTime < newDocument.UpdateTime)
                {
                    UnprocessedDocumentRepository.Add(newDocument);
                    additions.UnprocessedDocumentAdditions.Add(newDocument);
                    anyUnprocessedDocumentUpdates = true;
                }
            }

            if (anyUnprocessedDocumentUpdates)
            {
                RawContentRepository.AddRawContent(content);
                additions.RawContentAdditions.Add(content);
            }

            return additions;
        }
    }
}
