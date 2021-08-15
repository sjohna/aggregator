using AggregatorLib;
using NodaTime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AggregatorLibTest.TestHelpers;

namespace AggregatorLibTest
{
    [TestFixture(ExtractorInitMode.Stream)]
    [TestFixture(ExtractorInitMode.String)]
    class TestFeedExtractor
    {
        public enum ExtractorInitMode
        {
            Stream,
            String
        }

        private ExtractorInitMode InitMode;
        private Guid SourceRawContentId = Guid.NewGuid();

        public TestFeedExtractor(ExtractorInitMode mode)
        {
            this.InitMode = mode;
        }

        private FeedExtractor ExtractorForEmbeddedFile(string filename)
        {
            if (this.InitMode == ExtractorInitMode.Stream)
            {
                return ExtractorForEmbeddedFileFromStream(filename);
            }
            else
            {
                return ExtractorForEmbeddedFileFromString(filename);
            }
        }

        private FeedExtractor ExtractorForEmbeddedFileFromStream(string filename)
        {
            Stream resourceStream = GetTestDataResourceStream($"FeedExtractor.atom.{filename}");

            return new FeedExtractor(resourceStream, Instant.FromUnixTimeSeconds(12345678), SourceRawContentId);
        }

        private FeedExtractor ExtractorForEmbeddedFileFromString(string filename)
        {
            Stream resourceStream = GetTestDataResourceStream($"FeedExtractor.atom.{filename}");

            using (var streamReader = new StreamReader(resourceStream))
            {
                return new FeedExtractor(streamReader.ReadToEnd(), Instant.FromUnixTimeSeconds(12345678), SourceRawContentId);
            }
        }

        // assert properties for single post tests
        private void AssertSinglePostTestDocumentProperties(
            UnprocessedDocument doc,
            string Uri = "https://example.com/testblog/test-blog-entry-title",
            string SourceId = "12345",
            string? ParentDocumentUri = null,
            Instant? UpdateTime = null,
            Instant? PublishTime = null,
            List<UnprocessedDocumentAuthor>? Authors = null
        )
        {
            if (Authors == null)
            {
                Authors = new List<UnprocessedDocumentAuthor>()
                {
                    new UnprocessedDocumentAuthor(
                        Name: "Test Blogger",
                        Context: "blog",
                        Uri: "https://example.com/testblog"
                    )
                };
            }

            Assert.AreEqual(Uri, doc.Uri);
            Assert.AreEqual(SourceId, doc.SourceId);
            Assert.AreEqual(ParentDocumentUri, doc.ParentDocumentUri);
            Assert.AreEqual(UpdateTime, doc.UpdateTime);
            Assert.AreEqual(PublishTime, doc.PublishTime);
            Assert.AreEqual(SourceRawContentId, doc.SourceRawContentId);
            Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);

            Assert.AreEqual(Authors.Count, doc.Authors.Count);
            
            for (int i = 0; i < Authors.Count; ++i)
            {
                Assert.AreEqual(Authors[i].Name, doc.Authors[i].Name);
                Assert.AreEqual(Authors[i].Context, doc.Authors[i].Context);
                Assert.AreEqual(Authors[i].Uri, doc.Authors[i].Uri);
            }
        }

        private void AssertSinglePostTestDocumentContentProperties(
           UnprocessedDocumentContent unprocessedDocument,
           string Title = "Test Blog Entry Title",
           string Content = "<p>This is the test blog entry content.</p>",
           List<string>? Categories = null,
           bool AllowsComments = true,
           string? CommentUri = "https://example.com/testblog/test-blog-entry-title/#comments",
           string? CommentFeedUri = "https://example.com/testblog/test-blog-entry-title/feed/atom/"
        )
        {
            if (Categories == null) Categories = new List<string>();

            Assert.IsTrue(unprocessedDocument is BlogPostContent);
            var content = (unprocessedDocument as BlogPostContent)!;

            Assert.AreEqual(Title, content.Title);
            Assert.AreEqual(Content, content.Content);
            Assert.IsTrue(Enumerable.SequenceEqual(Categories!, content.Categories));
            Assert.AreEqual(AllowsComments, content.AllowsComments);
            Assert.AreEqual(CommentUri, content.CommentUri);
            Assert.AreEqual(CommentFeedUri, content.CommentFeedUri);
        }

        [Test]
        public void SinglePost()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost.xml");

            var doc = extractor.RawDocuments.First();

            AssertSinglePostTestDocumentProperties(doc);
            AssertSinglePostTestDocumentContentProperties(doc.Content);
        }

        [Test]
        public void SinglePost_WithCategories()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_WithCategories.xml");

            var doc = extractor.RawDocuments.First();

            AssertSinglePostTestDocumentProperties(doc);
            AssertSinglePostTestDocumentContentProperties(doc.Content, Categories: new List<string>() { "Category One", "Category Two", "Category Three" });
        }

        [Test]
        public void SinglePost_NoCommentsLink()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_NoCommentsLink.xml");

            var doc = extractor.RawDocuments.First();

            AssertSinglePostTestDocumentProperties(doc);
            AssertSinglePostTestDocumentContentProperties(doc.Content, CommentUri: null);
        }

        [Test]
        public void SinglePost_NoCommentsFeed()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_NoCommentsFeed.xml");

            var doc = extractor.RawDocuments.First();

            AssertSinglePostTestDocumentProperties(doc);
            AssertSinglePostTestDocumentContentProperties(doc.Content, CommentFeedUri: null);
        }

        [Test]
        public void SinglePost_NoCommentsLinkOrFeed()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_NoCommentsLinkOrFeed.xml");

            var doc = extractor.RawDocuments.First();

            AssertSinglePostTestDocumentProperties(doc);
            AssertSinglePostTestDocumentContentProperties(doc.Content, AllowsComments: false, CommentUri: null, CommentFeedUri: null);
        }

        [Test]
        public void SinglePost_NoAuthorUri()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_NoAuthorUri.xml");

            var doc = extractor.RawDocuments.First();

            AssertSinglePostTestDocumentProperties(doc, Authors: new List<UnprocessedDocumentAuthor>() { new UnprocessedDocumentAuthor("Test Blogger", "blog", null) } );
            AssertSinglePostTestDocumentContentProperties(doc.Content);
        }

        [Test]
        public void SinglePost_WithPublishedTime()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_WithPublishedTime.xml");

            var doc = extractor.RawDocuments.First();

            AssertSinglePostTestDocumentProperties(doc, PublishTime: Instant.FromUtc(2021, 2, 27, 0, 0));
            AssertSinglePostTestDocumentContentProperties(doc.Content);
        }

        [Test]
        public void SinglePost_WithUpdatedTime()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_WithUpdatedTime.xml");

            var doc = extractor.RawDocuments.First();

            AssertSinglePostTestDocumentProperties(doc, UpdateTime: Instant.FromUtc(2021, 2, 27, 0, 0));
            AssertSinglePostTestDocumentContentProperties(doc.Content);
        }

        [Test]
        public void TitleDocumentForSinglePost()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost.xml");

            var doc = extractor.TitleDocument!;

            Assert.IsNotNull(doc);

            Assert.AreEqual("https://example.com/testblog", doc.Uri);
            Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);
            Assert.AreEqual(null, doc.ParentDocumentUri);
            Assert.AreEqual(null, doc.UpdateTime);
            Assert.AreEqual(null, doc.PublishTime);
            Assert.AreEqual(SourceRawContentId, doc.SourceRawContentId);
            Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
            Assert.AreEqual(UnprocessedDocumentType.SourceDescription, doc.DocumentType);

            Assert.AreEqual(0, doc.Authors.Count);

            var content = (doc.Content as FeedSourceDescriptionContent)!;

            Assert.IsNotNull(content);

            Assert.AreEqual("Test Blog", content.Title);
            Assert.AreEqual("This is the subtitle for the test blog.", content.Description);
            Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", content.IconUri);
        }

        [Test]
        public void TitleDocumentForSinglePost_NoSubtitle()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_NoSubtitle.xml");

            var doc = extractor.TitleDocument!;

            Assert.IsNotNull(doc);

            Assert.AreEqual("https://example.com/testblog", doc.Uri);
            Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);
            Assert.AreEqual(null, doc.ParentDocumentUri);
            Assert.AreEqual(null, doc.UpdateTime);
            Assert.AreEqual(null, doc.PublishTime);
            Assert.AreEqual(SourceRawContentId, doc.SourceRawContentId);
            Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
            Assert.AreEqual(UnprocessedDocumentType.SourceDescription, doc.DocumentType);

            Assert.AreEqual(0, doc.Authors.Count);

            var content = (doc.Content as FeedSourceDescriptionContent)!;

            Assert.IsNotNull(content);

            Assert.AreEqual("Test Blog", content.Title);
            Assert.AreEqual(null, content.Description);
            Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", content.IconUri);
        }

        [Test]
        public void TitleDocumentForSinglePost_NoIconUri()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_NoIconUri.xml");

            var doc = extractor.TitleDocument!;

            Assert.IsNotNull(doc);

            Assert.AreEqual("https://example.com/testblog", doc.Uri);
            Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);
            Assert.AreEqual(null, doc.ParentDocumentUri);
            Assert.AreEqual(null, doc.UpdateTime);
            Assert.AreEqual(null, doc.PublishTime);
            Assert.AreEqual(SourceRawContentId, doc.SourceRawContentId);
            Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
            Assert.AreEqual(UnprocessedDocumentType.SourceDescription, doc.DocumentType);

            Assert.AreEqual(0, doc.Authors.Count);

            var content = (doc.Content as FeedSourceDescriptionContent)!;

            Assert.IsNotNull(content);

            Assert.AreEqual("Test Blog", content.Title);
            Assert.AreEqual("This is the subtitle for the test blog.", content.Description);
            Assert.AreEqual(null, content.IconUri);
        }

        [Test]
        public void TitleDocumentForSinglePost_NoFeedTitle()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_NoFeedTitle.xml");

            var doc = extractor.TitleDocument!;

            Assert.IsNull(doc);
        }

        [Test]
        public void InvalidAtomXMLThrowsException()
        {
            Assert.Throws<AggregatorSystemException>(() => ExtractorForEmbeddedFile("invalidXml.xml"));
        }

        [Test]
        public void IncompleteAtomXMLThrowsException()
        {
            Assert.Throws<AggregatorSystemException>(() => ExtractorForEmbeddedFile("incompleteXml.xml"));
        }

        [Test]
        public void TitleDocumentForSinglePostWithFeedAuthor()
        {
            var extractor = ExtractorForEmbeddedFile("singlePost_WithFeedAuthor.xml");

            var doc = extractor.TitleDocument!;

            Assert.IsNotNull(doc);

            Assert.AreEqual("https://example.com/testblog", doc.Uri);
            Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);
            Assert.AreEqual(null, doc.ParentDocumentUri);
            Assert.AreEqual(null, doc.UpdateTime);
            Assert.AreEqual(null, doc.PublishTime);
            Assert.AreEqual(SourceRawContentId, doc.SourceRawContentId);
            Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
            Assert.AreEqual(UnprocessedDocumentType.SourceDescription, doc.DocumentType);

            Assert.AreEqual(1, doc.Authors.Count);

            Assert.AreEqual("Test Blogger", doc.Authors[0].Name);
            Assert.AreEqual("blog", doc.Authors[0].Context);
            Assert.AreEqual("https://example.com/testblog", doc.Authors[0].Uri);

            var content = (doc.Content as FeedSourceDescriptionContent)!;

            Assert.IsNotNull(content);

            Assert.AreEqual("Test Blog", content.Title);
            Assert.AreEqual("This is the subtitle for the test blog.", content.Description);
            Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", content.IconUri);
        }

        // TODO: test author on feed but not on items: item author(s) should inherit from feed
        // TODO: (DONE) test feed author on TitleDocument
        // TODO: test comment feeds
        // TODO: test when things go wrong: essential fields not available, (DONE) can't parse input
    }
}