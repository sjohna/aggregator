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

        public void ProcessRawContent(RawContent content) 
        {
            if (content.Type == "atom/xml")
            {
                ProcessAtomContent(content);
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
        private void ProcessAtomContent(RawContent content)
        {
            // TODO SOON: give this function a refactor, or at least look at the variable names...

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
                // TODO: some sort of query on DB here, instead of doing this in LINQ
                var existingTitleDocument = UnprocessedDocumentRepository.GetAllUnprocessedDocuments()
                    .FirstOrDefault(doc => doc.SourceId == titleDocument.SourceId && doc.DocumentType == titleDocument.DocumentType);

                if (existingTitleDocument == null)
                {
                    UnprocessedDocumentRepository.AddUnprocessedDocument(titleDocument);
                    anyUnprocessedDocumentUpdates = true;
                }
                else
                {
                    // TODO: a less hacky way of doing this...
                    var existingContent = existingTitleDocument.Content as FeedSourceDescriptionContent;
                    var currentContent = titleDocument.Content as FeedSourceDescriptionContent;

                    if (existingContent != null && currentContent != null && !existingContent.Equals(currentContent))
                    {
                        UnprocessedDocumentRepository.AddUnprocessedDocument(titleDocument);
                        anyUnprocessedDocumentUpdates = true;
                    }
                }
            }

            foreach (var newDocument in extractor.RawDocuments)
            {
                var existingDocument = UnprocessedDocumentRepository.GetAllUnprocessedDocuments()
                    .OrderBy(doc => doc.UpdateTime)
                    .LastOrDefault(doc => doc.SourceId == newDocument.SourceId && doc.DocumentType == newDocument.DocumentType);

                // TODO: option for deep compare?
                if (existingDocument == null || existingDocument.UpdateTime < newDocument.UpdateTime)
                {
                    UnprocessedDocumentRepository.AddUnprocessedDocument(newDocument);
                    anyUnprocessedDocumentUpdates = true;
                }
            }

            if (anyUnprocessedDocumentUpdates)
            {
                RawContentRepository.AddRawContent(content);
            }
        }
    }
}
