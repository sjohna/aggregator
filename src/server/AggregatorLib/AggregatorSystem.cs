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

        private void ProcessAtomContent(RawContent content)
        {
            var extractor = new FeedExtractor(content.Content, content.RetrieveTime, content.Id);

            // TODO: check whether there are any updates before adding to repository
            RawContentRepository.AddRawContent(content);

            foreach (var document in extractor.RawDocuments)
            {
                // TODO: handle duplicates
                UnprocessedDocumentRepository.AddUnprocessedDocument(document);
            }
        }
    }
}
