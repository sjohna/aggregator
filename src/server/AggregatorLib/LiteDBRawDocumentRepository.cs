using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class LiteDBRawDocumentRepository : IRawDocumentRepository
    {
        private LiteDatabase Database;
        private ILiteCollection<RawDocument> Collection;

        public LiteDBRawDocumentRepository(LiteDatabase Database)
        {
            this.Database = Database;
            this.Collection = this.Database.GetCollection<RawDocument>("RawDocument");
        }

        public void AddRawDocument(RawDocument document)
        {
            Collection.Insert(document);
        }

        public IEnumerable<RawDocument> GetAllRawDocuments()
        {
            return Collection.FindAll();
        }

        public RawDocument GetRawDocumentById(Guid Id)
        {
            var doc = Collection.Find(doc => doc.Id == Id).FirstOrDefault();

            return doc ?? throw new RepositoryException($"RawDocument with id {Id} not present in repository.");
        }
    }
}
