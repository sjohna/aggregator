using LiteDB;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public UnprocessedDocument GetLatestForSourceId(string sourceId)
        {
            return Collection.Query().Where(doc => doc.SourceId == sourceId).OrderByDescending(doc => doc.UpdateTime).FirstOrDefault();
        }

        // TODO: unit tests
        public IEnumerable<UnprocessedDocument> Query(
            string? Where = null, 
            string? OrderByAsc = null,
            string? OrderByDesc = null,
            int? Offset = null,
            int? Limit = null)
        {
            if (OrderByAsc != null && OrderByDesc != null)
            {
                // TODO: better exception, maybe
                throw new ArgumentException();
            }

            var query = Collection.Query();
            query = Where != null ? query.Where(Where) : query;
            query = OrderByAsc != null ? query.OrderBy(OrderByAsc) : query;
            query = OrderByDesc != null ? query.OrderByDescending(OrderByDesc) : query;

            ILiteQueryableResult<UnprocessedDocument> result = query;
            result = Offset != null ? result.Offset(Offset.Value) : result;
            result = Limit != null ? result.Limit(Limit.Value) : result;

            return result.ToEnumerable();
        }

        // TODO: unit tests
        public int Count(
            string? Where = null)
        {
            if (Where != null)
            {
                return Collection.Count(Where);
            }
            else
            {
                return Collection.Count();
            }
        }
    }
}
