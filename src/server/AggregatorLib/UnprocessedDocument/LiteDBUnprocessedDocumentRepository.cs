using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class LiteDBUnprocessedDocumentRepository : IUnprocessedDocumentRepository
    {
        private LiteDatabase Database;
        private ILiteCollection<UnprocessedDocument> Collection;

        public LiteDBUnprocessedDocumentRepository(LiteDatabase Database)
        {
            this.Database = Database;
            this.Collection = this.Database.GetCollection<UnprocessedDocument>("UnprocessedDocument");

            this.Collection.EnsureIndex(doc => doc.SourceId);
        }

        public void Add(UnprocessedDocument document)
        {
            Collection.Insert(document);
        }

        public IEnumerable<UnprocessedDocument> GetAll()
        {
            return Collection.FindAll();
        }

        public UnprocessedDocument? GetById(Guid Id)
        {
            var doc = Collection.Find(doc => doc.Id == Id).FirstOrDefault();

            return doc;
        }

        public IEnumerable<UnprocessedDocument> GetBySourceId(string sourceId)
        {
            return Collection.Find(doc => doc.SourceId == sourceId);
        }
    }
}
