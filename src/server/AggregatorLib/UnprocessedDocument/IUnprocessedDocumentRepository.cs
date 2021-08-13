using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public interface IUnprocessedDocumentRepository
    {
        public void Add(UnprocessedDocument document);

        public UnprocessedDocument? GetById(Guid Id);

        public IEnumerable<UnprocessedDocument> GetAll();

        public IEnumerable<UnprocessedDocument> GetBySourceId(string sourceId);

        public UnprocessedDocument GetLatestForSourceId(string sourceId);
    }
}
