﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public abstract class UnprocessedDocumentContent
    {
        protected UnprocessedDocumentContent() { }

        public abstract string ContentType { get; }
    }
}
