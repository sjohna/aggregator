using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public interface IUnprocessedDocumentRepository
    {
        public void AddUnprocessedDocument(UnprocessedDocument document);

        public UnprocessedDocument GetUnprocessedDocumentById(Guid Id);

        public IEnumerable<UnprocessedDocument> GetAllUnprocessedDocuments();

        // TODO: queries
    }
}
