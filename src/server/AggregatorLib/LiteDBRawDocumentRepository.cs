using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class LiteDBRawDocumentRepository : IUnprocessedDocumentRepository
    {
        private LiteDatabase Database;
        private ILiteCollection<UnprocessedDocument> Collection;

        public LiteDBRawDocumentRepository(LiteDatabase Database)
        {
            this.Database = Database;
            this.Collection = this.Database.GetCollection<UnprocessedDocument>("RawDocument");
        }

        public void AddRawDocument(UnprocessedDocument document)
        {
            Collection.Insert(document);
        }

        public IEnumerable<UnprocessedDocument> GetAllRawDocuments()
        {
            return Collection.FindAll();
        }

        public UnprocessedDocument GetRawDocumentById(Guid Id)
        {
            var doc = Collection.Find(doc => doc.Id == Id).FirstOrDefault();

            return doc ?? throw new RepositoryException($"RawDocument with id {Id} not present in repository.");
        }
    }
}
