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
        }

        public void AddUnprocessedDocument(UnprocessedDocument document)
        {
            Collection.Insert(document);
        }

        public IEnumerable<UnprocessedDocument> GetAllUnprocessedDocuments()
        {
            return Collection.FindAll();
        }

        public UnprocessedDocument? GetUnprocessedDocumentById(Guid Id)
        {
            var doc = Collection.Find(doc => doc.Id == Id).FirstOrDefault();

            return doc;
        }
    }
}
