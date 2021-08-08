using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class LiteDBRawContentRepository : IRawContentRepository
    {
        private LiteDatabase Database;
        private ILiteCollection<RawContent> Collection;

        public LiteDBRawContentRepository(LiteDatabase Database)
        {
            this.Database = Database;
            this.Collection = this.Database.GetCollection<RawContent>("RawContent");
        }

        public void AddRawContent(RawContent content)
        {
            Collection.Insert(content);
        }

        public IEnumerable<RawContent> GetAllRawContent()
        {
            return Collection.FindAll();
        }

        public RawContent GetRawContentById(Guid Id)
        {
            var content = Collection.Find(content => content.Id == Id).FirstOrDefault();

            return content ?? throw new RepositoryException($"RawContent with id {Id} not present in repository.");
        }
    }
}
