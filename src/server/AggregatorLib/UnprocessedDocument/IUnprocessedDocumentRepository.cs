using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public interface IUnprocessedDocumentRepository
    {
        public void AddRawDocument(UnprocessedDocument document);

        public UnprocessedDocument GetRawDocumentById(Guid Id);

        public IEnumerable<UnprocessedDocument> GetAllRawDocuments();

        // TODO: queries
    }
}
