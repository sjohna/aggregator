using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public interface IRawDocumentRepository
    {
        public void AddRawDocument(RawDocument document);

        public RawDocument GetRawDocumentById(Guid Id);

        public IEnumerable<RawDocument> GetAllRawDocuments();

        // TODO: queries
    }
}
