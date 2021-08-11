using AggregatorLib;
using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using static AggregatorLibTest.TestHelpers;
using System.IO;

namespace AggregatorLibTest
{
    [TestFixture]
    class TestAggregatorSystem
    {
#pragma warning disable CS8618
        private AggregatorSystem system;
        private IRawContentRepository rawContentRepository;
        private IUnprocessedDocumentRepository unprocessedDocumentRepository;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            LiteDBFunctions.DoLiteDBGlobalSetUp();

            rawContentRepository = new LiteDBRawContentRepository(new LiteDatabase(":memory:"));
            unprocessedDocumentRepository = new LiteDBUnprocessedDocumentRepository(new LiteDatabase(":memory:"));

            system = new AggregatorSystem
            (
                RawContentRepository: rawContentRepository,
                UnprocessedDocumentRepository: unprocessedDocumentRepository
            );
        }

        private RawContent RawContentForEmbeddedAtomFeed(string filename)
        {
            Stream resourceStream = GetTestDataResourceStream($"atom.{filename}");

            using (var reader = new StreamReader(resourceStream))
            {
                return new RawContent
                (
                    RetrieveTime: Instant.FromUnixTimeSeconds(12345678),
                    Type: "atom/xml",
                    Content: reader.ReadToEnd(),
                    Context: "blog",
                    SourceUri: "http://example.com/atom"
                );
            }
        }

        [Test]
        public void StateAfterCreation()
        {
            Assert.AreEqual(0, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(0, system.UnprocessedDocumentRepository.GetAllUnprocessedDocuments().Count());
        }

        [Test]
        public void ProcessRawContentWithInvalidType()
        {
            var content = new RawContent
            (
                    RetrieveTime: Instant.FromUnixTimeSeconds(12345678),
                    Type: "Invalid Type",
                    Content: "Test Content",
                    Context: "Test Context",
                    SourceUri: "http://example.com/test"
            );

            Assert.Throws<AggregatorSystemException>(() => system.ProcessRawContent(content));
        }

        [Test]
        public void ProcessAtomContentWithSingleItem()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost_WithUpdateAndPublishTime.xml");

            system.ProcessRawContent(content);

            Assert.AreEqual(1, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(2, system.UnprocessedDocumentRepository.GetAllUnprocessedDocuments().Count());

            // post
            {
                var doc = system.UnprocessedDocumentRepository.GetAllUnprocessedDocuments().First(doc => doc.DocumentType == UnprocessedDocumentType.Regular);

                Assert.AreEqual(null, doc.ParentDocumentUri);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as BlogPostContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual("Test Blog Entry Title", unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title/feed/atom/", unprocessedContent.CommentFeedUri);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title/#comments", unprocessedContent.CommentUri);
                Assert.AreEqual(true, unprocessedContent.AllowsComments);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // title doc
            {
                var doc = system.UnprocessedDocumentRepository.GetAllUnprocessedDocuments().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.ParentDocumentUri);
                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual("Test Blog", unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        public void ProcessingContentWithgExistingIdThrowsException()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost_WithUpdateAndPublishTime.xml");
            system.ProcessRawContent(content);

            Assert.Throws<AggregatorSystemException>(() => system.ProcessRawContent(content));
        }

        [Test]
        public void ProcessingSameFeedTwiceInARowIsIdempotent()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost_WithUpdateAndPublishTime.xml");
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("singlePost_WithUpdateAndPublishTime.xml");
            system.ProcessRawContent(content2);

            Assert.AreEqual(1, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(2, system.UnprocessedDocumentRepository.GetAllUnprocessedDocuments().Count());

            // post
            {
                var doc = system.UnprocessedDocumentRepository.GetAllUnprocessedDocuments().First(doc => doc.DocumentType == UnprocessedDocumentType.Regular);

                Assert.AreEqual(null, doc.ParentDocumentUri);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as BlogPostContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual("Test Blog Entry Title", unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title/feed/atom/", unprocessedContent.CommentFeedUri);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title/#comments", unprocessedContent.CommentUri);
                Assert.AreEqual(true, unprocessedContent.AllowsComments);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // title doc
            {
                var doc = system.UnprocessedDocumentRepository.GetAllUnprocessedDocuments().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.ParentDocumentUri);
                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual("Test Blog", unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }
    }
}
