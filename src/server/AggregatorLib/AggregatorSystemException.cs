using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class AggregatorSystemException : Exception
    {
        public AggregatorSystemException(string message) : base(message) { }

        public AggregatorSystemException(string message, Exception innerException) : base(message, innerException) { }
    }
}
