﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class RawContent
    {
        [BsonId]
        public Guid Id { get; protected set; }
        public string Content { get; protected set; }
        public string Type { get; protected set; }
        public string? SourceUri { get; protected set; }

#pragma warning disable CS8618  // disable null checking, as this constructor is only used by LiteDB, which sets the properties of this object immediately after constructing
        protected RawContent() { }
#pragma warning restore CS8618

        public RawContent(string Type, string Content, string? SourceUri = null)
            : this(Guid.NewGuid(), Type, Content, SourceUri)
        {
        }

        public RawContent(Guid Id, string Type, string Content, string? SourceUri = null)
        {
            this.Id = Id;
            this.Type = Type;
            this.Content = Content;
            this.SourceUri = SourceUri;
        }

    }
}
