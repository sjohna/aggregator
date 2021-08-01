﻿using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AggregatorLib
{
    public class FeedExtractor
    {
        SyndicationFeed feed;
        Instant retrieveTime;
        
        // TODO: id as source URI, alternate link doesn't exist, only summary (return stub content type), different feed link types, different content types?, source feed for content, parent documents for comments
        public FeedExtractor(Stream feedStream, Instant retrieveTime)
        {
            this.retrieveTime = retrieveTime;

            using (var feedReader = XmlReader.Create(feedStream))
            {
                feed = SyndicationFeed.Load(feedReader);
            }
        }
        public IEnumerable<RawDocument> RawDocuments
        {
            get
            {
                foreach (var item in feed.Items)
                {
                    var alternateLink = item.Links.FirstOrDefault(link => link.RelationshipType == "alternate");
                    string sourceUri = alternateLink?.Uri.ToString() ?? throw new Exception();  // TODO: different exception here

                    var commentLink = item.Links.FirstOrDefault(link => link.RelationshipType == "replies" && link.MediaType != "application/atom+xml");    // TODO: this might not be the only type to check...
                    string? commentUri = commentLink?.Uri.ToString();

                    var commentFeedLink = item.Links.FirstOrDefault(link => link.RelationshipType == "replies" && link.MediaType == "application/atom+xml");    // TODO: this might not be the only type to check...
                    string? commentFeedUri = commentFeedLink?.Uri.ToString();

                    var authors = new List<RawDocumentAuthor>();
                    foreach (var author in item.Authors)
                    {
                        authors.Add(new RawDocumentAuthor(
                            Name: author.Name,
                            Context: "blog",        // TODO: maybe this should be the site? Not sure what I should do with the context...
                            Uri: author.Uri
                        ));
                    }

                    var categories = new List<String>();
                    foreach (var category in item.Categories)
                    {
                        if (!category.Name.Equals("uncategorized", StringComparison.OrdinalIgnoreCase))
                        {
                            categories.Add(category.Name);
                        }
                    }

                    var content = new WordpressContent(
                        Title: WebUtility.HtmlDecode(item.Title.Text),
                        Content: (item?.Content as TextSyndicationContent)?.Text!,  // TODO: handle errors, handle no content present, only summary
                        Categories: categories,
                        AllowsComments: commentUri != null || commentFeedUri != null,
                        CommentUri: commentUri,
                        CommentFeedUri: commentFeedUri
                    ) ;

                    yield return new RawDocument(
                        Id: Guid.NewGuid(),
                        Uri: sourceUri,
                        SourceId: item.Id,
                        ParentDocumentUri: null,
                        RetrieveTime: retrieveTime,
                        UpdateTime: Instant.FromDateTimeOffset(item.LastUpdatedTime),   // TODO: test these times not being present
                        PublishTime: Instant.FromDateTimeOffset(item.PublishDate),
                        Content: content,
                        Authors: authors
                    );
                }
            }
        }
    }
}
