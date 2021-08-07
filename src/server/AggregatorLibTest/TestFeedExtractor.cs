using AggregatorLib;
using NodaTime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregatorLibTest
{
    [TestFixture]
    class TestFeedExtractor
    {
        private FeedExtractor ExtractorForEmbeddedFile(string filename)
        {
            string resourceName = $"AggregatorLibTest.TestData.atom.{filename}";

            var assembly = typeof(AggregatorLibTest.TestFeedExtractor).Assembly;

            Stream resourceStream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception("Null resource stream");

            return new FeedExtractor(resourceStream, Instant.FromUnixTimeSeconds(12345678));
        }

        [SetUp]
        public void SetUp()
        {

        }

        // assert properties for single post tests
        private void AssertSinglePostTestDocumentProperties(
            RawDocument doc,
            string Uri = "https://example.com/testblog/test-blog-entry-title",
            string SourceId = "12345",
            string? ParentDocumentUri = null,
            Instant? UpdateTime = null,
            Instant? PublishTime = null,
            List<RawDocumentAuthor>? Authors = null
        )
        {
            if (Authors == null)
            {
                Authors = new List<RawDocumentAuthor>()
                {
                    new RawDocumentAuthor(
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

            Assert.AreEqual(Authors.Count, doc.Authors.Count);
            
            for (int i = 0; i < Authors.Count; ++i)
            {
                Assert.AreEqual(Authors[i].Name, doc.Authors[i].Name);
                Assert.AreEqual(Authors[i].Context, doc.Authors[i].Context);
                Assert.AreEqual(Authors[i].Uri, doc.Authors[i].Uri);
            }
        }

        private void AssertSinglePostTestDocumentContentProperties(
           RawDocumentContent rawContent,
           string Title = "Test Blog Entry Title",
           string Content = "<p>This is the test blog entry content.</p>",
           List<string>? Categories = null,
           bool AllowsComments = true,
           string? CommentUri = "https://example.com/testblog/test-blog-entry-title/#comments",
           string? CommentFeedUri = "https://example.com/testblog/test-blog-entry-title/feed/atom/"
        )
        {
            if (Categories == null) Categories = new List<string>();

            Assert.IsTrue(rawContent is BlogPostContent);
            var content = (rawContent as BlogPostContent)!;

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

            AssertSinglePostTestDocumentProperties(doc, Authors: new List<RawDocumentAuthor>() { new RawDocumentAuthor("Test Blogger", "blog", null) } );
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
    }
}
