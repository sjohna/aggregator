using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;

namespace AggregatorLib
{
    public class FeedExtractor
    {
        SyndicationFeed feed;
        Instant retrieveTime;
        Guid sourceRawContentId;
        
        // TODO: id as source URI, alternate link doesn't exist, only summary (return stub content type), different feed link types, different content types?, source feed for content, parent documents for comments
        public FeedExtractor(Stream feedStream, Instant retrieveTime, Guid sourceRawContentId)
        {
            this.retrieveTime = retrieveTime;
            this.sourceRawContentId = sourceRawContentId;

            using (var feedReader = XmlReader.Create(feedStream))
            {
                feed = SyndicationFeed.Load(feedReader);
            }
        }

        public FeedExtractor(string feedString, Instant retrieveTime, Guid sourceRawContentId)
        {
            this.retrieveTime = retrieveTime;
            this.sourceRawContentId = sourceRawContentId;

            using (var stringReader = new StringReader(feedString))
            using (var feedReader = XmlReader.Create(stringReader))
            {
                feed = SyndicationFeed.Load(feedReader);
            }
        }

        public UnprocessedDocument? TitleDocument
        {
            get
            {
                var title = feed.Title?.Text;
                if (title == null) return null;

                var iconElement = feed.ElementExtensions.FirstOrDefault(element => element.GetReader().Name == "icon");
                var iconUri = iconElement?.GetObject<string>(); // TODO: catch XmlException if content is not in fact a string

                var alternateLink = feed.Links.FirstOrDefault(link => link.RelationshipType == "alternate");
                string sourceUri = alternateLink?.Uri.ToString() ?? throw new Exception();  // TODO: different exception here

                var content = new FeedSourceDescriptionContent
                (
                    Title: title,
                    Description: feed.Description?.Text,
                    IconUri: iconUri
                );

                var authors = new List<UnprocessedDocumentAuthor>();
                foreach (var author in feed.Authors)
                {
                    authors.Add(new UnprocessedDocumentAuthor(
                        Name: author.Name,
                        Context: "blog",
                        Uri: author.Uri
                    ));
                }

                return new UnprocessedDocument(
                        Id: Guid.NewGuid(),
                        Uri: sourceUri,
                        SourceId: feed.Id,
                        ParentDocumentUri: null,
                        RetrieveTime: retrieveTime,
                        UpdateTime: null,
                        PublishTime: null,
                        Content: content,
                        Authors: authors,
                        SourceRawContentId: sourceRawContentId,
                        DocumentType: UnprocessedDocumentType.SourceDescription
                    );
            }
        }

        public IEnumerable<UnprocessedDocument> RawDocuments
        {
            get
            {
                foreach (var item in feed.Items)
                {
                    var alternateLink = item.Links.FirstOrDefault(link => link.RelationshipType == "alternate");
                    string sourceUri = alternateLink?.Uri.ToString() ?? throw new Exception();  // TODO: different exception here

                    // TODO: rethink how to detect whether comments are allowed
                    var commentLink = item.Links.FirstOrDefault(link => link.RelationshipType == "replies" && link.MediaType != "application/atom+xml");    // TODO: this might not be the only type to check...
                    string? commentUri = commentLink?.Uri.ToString();

                    var commentFeedLink = item.Links.FirstOrDefault(link => link.RelationshipType == "replies" && link.MediaType == "application/atom+xml");    // TODO: this might not be the only type to check...
                    string? commentFeedUri = commentFeedLink?.Uri.ToString();

                    var authors = new List<UnprocessedDocumentAuthor>();
                    foreach (var author in item.Authors)
                    {
                        authors.Add(new UnprocessedDocumentAuthor(
                            Name: author.Name,
                            Context: "blog",
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

                    var content = new BlogPostContent(
                        Title: WebUtility.HtmlDecode(item.Title.Text),
                        Content: (item.Content as TextSyndicationContent)?.Text!,  // TODO: handle errors, handle no content present, only summary
                        Categories: categories,
                        AllowsComments: commentUri != null || commentFeedUri != null,
                        CommentUri: commentUri,
                        CommentFeedUri: commentFeedUri
                    ) ;

                    Instant? UpdateTime = item.LastUpdatedTime.Year != 1 ? Instant.FromDateTimeOffset(item.LastUpdatedTime) : null;
                    Instant? PublishTime = item.PublishDate.Year != 1 ? Instant.FromDateTimeOffset(item.PublishDate) : null;

                    yield return new UnprocessedDocument(
                        Id: Guid.NewGuid(),
                        Uri: sourceUri,
                        SourceId: item.Id,
                        ParentDocumentUri: null,
                        RetrieveTime: retrieveTime,
                        UpdateTime: UpdateTime,
                        PublishTime: PublishTime,
                        Content: content,
                        Authors: authors,
                        SourceRawContentId: sourceRawContentId
                    );
                }
            }
        }
    }
}
