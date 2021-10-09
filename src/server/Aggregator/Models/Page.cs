using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aggregator.Models
{
    public class Page<T>
    {
        public int PageSize { get; }

        public int Offset { get; }

        public int? Total { get; }

        public List<T> Items { get; }

        public Page(int PageSize, int Offset, List<T> Items)
        {
            this.PageSize = PageSize;
            this.Offset = Offset;
            this.Items = Items;
            this.Total = null;
        }

        public Page(int PageSize, int Offset, int Total, List<T> Items)
        {
            this.PageSize = PageSize;
            this.Offset = Offset;
            this.Total = Total;
            this.Items = Items;
        }
    }
}
