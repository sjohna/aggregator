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

namespace AggregatorLibTest.TestAggregatorSystem
{
    [TestFixture]
    class ProcessAtomContent
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

        private RawContent RawContentForEmbeddedAtomFeed(string filename, Instant retrieveTime)
        {
            Stream resourceStream = GetTestDataResourceStream($"AggregatorSystem.atom.{filename}");

            using (var reader = new StreamReader(resourceStream))
            {
                return new RawContent
                (
                    RetrieveTime: retrieveTime,
                    Type: "atom/xml",
                    Content: reader.ReadToEnd(),
                    Context: "blog",
                    SourceUri: "http://example.com/atom"
                );
            }
        }

        [Test]
        public void ProcessAtomContentWithSingleItem()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));

            system.ProcessRawContent(content);

            Assert.AreEqual(1, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(2, system.UnprocessedDocumentRepository.GetAll().Count());

            // post
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.Regular);

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
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
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        public void ProcessingSameFeedTwiceInARowIsIdempotent()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content2);

            Assert.AreEqual(1, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(2, system.UnprocessedDocumentRepository.GetAll().Count());

            // post
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.Regular);

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
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
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        public void ProcessAtomContentWithUpdatedVersionOfExistingUnprocessedDocument()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("singlePostUpdated.xml", Instant.FromUnixTimeSeconds(23456789));
            system.ProcessRawContent(content2);

            Assert.AreEqual(2, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(3, system.UnprocessedDocumentRepository.GetAll().Count());

            // post - first version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // post - second version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .Skip(1)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 1, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(23456789), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the updated test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content2.Id);
            }

            // title doc
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        public void ProcessAtomContentWithUpdatedTitleDocument()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("singlePostWithUpdatedSubtitle.xml", Instant.FromUnixTimeSeconds(23456789));
            system.ProcessRawContent(content2);

            Assert.AreEqual(2, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(3, system.UnprocessedDocumentRepository.GetAll().Count());

            // post - first version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // title doc - first version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "http://example.com/testblog/feed/atom/" && doc.DocumentType == UnprocessedDocumentType.SourceDescription)
                    .OrderBy(doc => doc.RetrieveTime)
                    .First();
                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // title doc - second version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "http://example.com/testblog/feed/atom/" && doc.DocumentType == UnprocessedDocumentType.SourceDescription)
                    .OrderBy(doc => doc.RetrieveTime)
                    .Skip(1)
                    .First();

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(23456789), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the new subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content2.Id);
            }
        }

        [Test]
        [Repeat(10)]
        public void ReprocessingSameSubtitleDoesNotCreateRedundantDocuments()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("singlePostWithUpdatedSubtitle.xml", Instant.FromUnixTimeSeconds(23456789));
            system.ProcessRawContent(content2);

            Assert.AreEqual(2, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(3, system.UnprocessedDocumentRepository.GetAll().Count());

            var content3 = RawContentForEmbeddedAtomFeed("singlePostWithUpdatedSubtitle.xml", Instant.FromUnixTimeSeconds(34567890));
            system.ProcessRawContent(content3);

            Assert.AreEqual(2, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(3, system.UnprocessedDocumentRepository.GetAll().Count());

            // post - first version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // title doc - first version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "http://example.com/testblog/feed/atom/" && doc.DocumentType == UnprocessedDocumentType.SourceDescription)
                    .OrderBy(doc => doc.RetrieveTime)
                    .First();

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // title doc - second version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "http://example.com/testblog/feed/atom/" && doc.DocumentType == UnprocessedDocumentType.SourceDescription)
                    .OrderBy(doc => doc.RetrieveTime)
                    .Skip(1)
                    .First();

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(23456789), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the new subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content2.Id);
            }
        }

        [Test]
        public void ProcessAtomContentWithTwoPosts()
        {
            var content = RawContentForEmbeddedAtomFeed("twoPosts.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            Assert.AreEqual(1, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(3, system.UnprocessedDocumentRepository.GetAll().Count());

            // first post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // second post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12346")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title-2", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12346", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title 2"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the second test blog entry content.</p>", unprocessedContent.Content);
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
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        public void ProcessAtomContentWithSecondPost()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("secondPost.xml", Instant.FromUnixTimeSeconds(23456789));
            system.ProcessRawContent(content2);

            Assert.AreEqual(2, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(3, system.UnprocessedDocumentRepository.GetAll().Count());

            // first post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // second post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12346")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title-2", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(23456789), doc.RetrieveTime);
                Assert.AreEqual("12346", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title 2"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the second test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content2.Id);
            }

            // title doc
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        public void ProcessAtomContentWithOneNewAndOneExistingPost()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("twoPosts.xml", Instant.FromUnixTimeSeconds(23456789));
            system.ProcessRawContent(content2);

            Assert.AreEqual(2, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(3, system.UnprocessedDocumentRepository.GetAll().Count());

            // first post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // second post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12346")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title-2", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(23456789), doc.RetrieveTime);
                Assert.AreEqual("12346", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title 2"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the second test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content2.Id);
            }

            // title doc
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        public void ProcessAtomContentWhereOneOfTwoDocumentsIsUpdated()
        {
            var content = RawContentForEmbeddedAtomFeed("twoPosts.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("twoPostsOneUpdated.xml", Instant.FromUnixTimeSeconds(23456789));
            system.ProcessRawContent(content2);

            Assert.AreEqual(2, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(4, system.UnprocessedDocumentRepository.GetAll().Count());

            // first post - first version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // first post - second version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .Skip(1)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 1, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(23456789), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the updated test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content2.Id);
            }

            // second post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12346")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title-2", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12346", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title 2"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the second test blog entry content.</p>", unprocessedContent.Content);
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
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        public void ProcessAtomContentWithBothNewAndUpdatedPosts()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            var content2 = RawContentForEmbeddedAtomFeed("twoPosts.xml", Instant.FromUnixTimeSeconds(23456789));
            system.ProcessRawContent(content2);

            var content3 = RawContentForEmbeddedAtomFeed("threePostsOneUpdated.xml", Instant.FromUnixTimeSeconds(34567890));
            system.ProcessRawContent(content3);

            Assert.AreEqual(3, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(5, system.UnprocessedDocumentRepository.GetAll().Count());

            // first post - first version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // first post - second version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .Skip(1)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 1, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(34567890), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the updated test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content3.Id);
            }

            // second post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12346")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title-2", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(23456789), doc.RetrieveTime);
                Assert.AreEqual("12346", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title 2"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the second test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content2.Id);
            }

            // third post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12347")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 2, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 2, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title-3", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(34567890), doc.RetrieveTime);
                Assert.AreEqual("12347", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title 3"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the third test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content3.Id);
            }

            // title doc
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }

        [Test]
        [Repeat(10)]
        public void IdempotencyScenario()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUtc(2021, 8, 11, 1, 0, 0)));

            var content2 = RawContentForEmbeddedAtomFeed("twoPosts.xml", Instant.FromUnixTimeSeconds(23456789));
            system.ProcessRawContent(content2);

            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUtc(2021, 8, 11, 2, 0, 0)));
            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("secondPost.xml", Instant.FromUtc(2021, 8, 11, 3, 0, 0)));
            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("twoPosts.xml", Instant.FromUtc(2021, 8, 11, 4, 0, 0)));

            var content3 = RawContentForEmbeddedAtomFeed("threePostsOneUpdated.xml", Instant.FromUnixTimeSeconds(34567890));
            system.ProcessRawContent(content3);

            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUtc(2021, 8, 11, 5, 0, 0)));
            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("secondPost.xml", Instant.FromUtc(2021, 8, 11, 6, 0, 0)));
            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("twoPosts.xml", Instant.FromUtc(2021, 8, 11, 7, 0, 0)));
            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("singlePostUpdated.xml", Instant.FromUtc(2021, 8, 11, 8, 0, 0)));
            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("twoPostsOneUpdated.xml", Instant.FromUtc(2021, 8, 11, 9, 0, 0)));
            system.ProcessRawContent(RawContentForEmbeddedAtomFeed("threePostsOneUpdated.xml", Instant.FromUtc(2021, 8, 11, 10, 0, 0)));

            if (system.RawContentRepository.GetAllRawContent().Count() != 3)
            {
                var contentList = system.RawContentRepository.GetAllRawContent().ToList();
                Console.WriteLine("help");
            }

            Assert.AreEqual(3, system.RawContentRepository.GetAllRawContent().Count());
            Assert.AreEqual(5, system.UnprocessedDocumentRepository.GetAll().Count());

            // first post - first version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }

            // first post - second version
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12345")
                    .OrderBy(doc => doc.UpdateTime)
                    .Skip(1)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 1, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(34567890), doc.RetrieveTime);
                Assert.AreEqual("12345", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the updated test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content3.Id);
            }

            // second post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12346")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 0, 30, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title-2", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(23456789), doc.RetrieveTime);
                Assert.AreEqual("12346", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title 2"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the second test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content2.Id);
            }

            // third post
            {
                var doc = system.UnprocessedDocumentRepository
                    .GetAll()
                    .Where(doc => doc.SourceId == "12347")
                    .OrderBy(doc => doc.UpdateTime)
                    .First();

                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 2, 0, 0), doc.PublishTime);
                Assert.AreEqual(Instant.FromUtc(2021, 2, 27, 2, 0, 0), doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog/test-blog-entry-title-3", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(34567890), doc.RetrieveTime);
                Assert.AreEqual("12347", doc.SourceId);

                var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("html", "Test Blog Entry Title 3"), unprocessedContent.Title);
                Assert.AreEqual("<p>This is the third test blog entry content.</p>", unprocessedContent.Content);
                Assert.AreEqual(0, unprocessedContent.Categories.Count());

                Assert.AreEqual(1, doc.Authors.Count());

                var author = doc.Authors.First();

                Assert.AreEqual("Test Blogger", author.Name);
                Assert.AreEqual("https://example.com/testblog", author.Uri);
                Assert.AreEqual("blog", author.Context);

                Assert.AreEqual(doc.SourceRawContentId, content3.Id);
            }

            // title doc
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("https://example.com/testblog", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("http://example.com/testblog/feed/atom/", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Test Blog"), unprocessedContent.Title);
                Assert.AreEqual("This is the subtitle for the test blog.", unprocessedContent.Description);
                Assert.AreEqual("https://example.files.wordpress.com/testblogicon.jpg?w=32", unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());

                Assert.AreEqual(doc.SourceRawContentId, content.Id);
            }
        }
    }
}
