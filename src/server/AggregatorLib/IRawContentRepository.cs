using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public interface IRawContentRepository
    {
        public void AddRawContent(RawContent content);

        public RawContent GetRawContentById(Guid Id);

        public IEnumerable<RawContent> GetAllRawContent();

        // TODO: queries
    }
}
