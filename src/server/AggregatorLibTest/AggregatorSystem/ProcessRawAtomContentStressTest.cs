using AggregatorLib;
using LiteDB;
using NodaTime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace AggregatorLibTest.TestAggregatorSystem
{
    [TestFixture]
    class ProcessRawAtomContentStressTest
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

        private AtomTextConstruct ItemTitle(int item, int version) => new AtomTextConstruct("text", $"Item {item} Title {version}");

        private string ItemContent(int item, int version) => $"Item {item} content {version}";

        private string ItemUri(int item) => $"http://dynamic-test-feed.com/{item}";

        private string ItemId(int item) => $"{item}";

        private Instant ItemUpdateTime(int item, int version) => Instant.FromUnixTimeSeconds(1000000 * item + version);

        private void AssertDocumentIsCorrect(UnprocessedDocument doc, int item, int version, Instant retrieveTime)
        {
            Assert.AreEqual(null, doc.PublishTime);
            Assert.AreEqual(ItemUpdateTime(item, version), doc.UpdateTime);
            Assert.AreEqual(ItemUri(item), doc.Uri);
            Assert.AreEqual(retrieveTime, doc.RetrieveTime);
            Assert.AreEqual(ItemId(item), doc.SourceId);

            var unprocessedContent = (doc.Content as UnprocessedAtomContent)!;
            Assert.IsNotNull(unprocessedContent);

            Assert.AreEqual(ItemTitle(item, version), unprocessedContent.Title);
            Assert.AreEqual(ItemContent(item, version), unprocessedContent.Content);
            Assert.AreEqual(0, unprocessedContent.Categories.Count());

            Assert.AreEqual(0, doc.Authors.Count());
        }

        private SyndicationItem Item(int itemIndex, int version)
        {
            var item = new SyndicationItem();
            item.Id = ItemId(itemIndex);
            item.Links.Add(new SyndicationLink(new Uri(ItemUri(itemIndex)), "alternate", "link", null, 0));
            item.Title = SyndicationContent.CreatePlaintextContent(ItemTitle(itemIndex, version).Content);
            item.Content = SyndicationContent.CreatePlaintextContent(ItemContent(itemIndex, version));
            item.LastUpdatedTime = ItemUpdateTime(itemIndex, version).ToDateTimeOffset();

            return item;
        }

        private SyndicationFeed Feed()
        {
            var feed = new SyndicationFeed();
            feed.Title = SyndicationContent.CreatePlaintextContent("Dynamic Test Feed");
            feed.Description = SyndicationContent.CreatePlaintextContent("Dynamic Test Feed description.");
            feed.Id = "dynamic-test-feed";
            feed.Links.Add(new SyndicationLink(new Uri("http://dynamic-test-feed.com/atom"), "alternate", "link", null, 0));

            feed.Items = new List<SyndicationItem>();

            return feed;
        }

        private void AddItem(SyndicationFeed feed, int item, int version) 
        {
            (feed.Items as List<SyndicationItem>)!.Add(Item(item, version));
        }

        private void AddItemRange(SyndicationFeed feed, int startIndex, int count, int version)
        {
            var list = (feed.Items as List<SyndicationItem>)!;

            for (int i = startIndex; i < startIndex + count; ++i )
            {
                list.Add(Item(i, version));
            }
        }

        private RawContent ToContent(SyndicationFeed feed, Instant retrieveTime)
        {
            var builder = new StringBuilder();
            var atomWriter = XmlWriter.Create(builder);
            var atomFormatter = new Atom10FeedFormatter(feed);
            atomFormatter.WriteTo(atomWriter);
            atomWriter.Close();

            var rawContent = new RawContent
            (
                RetrieveTime: retrieveTime,
                Content: builder.ToString(),
                Context: "blog",
                Type: "atom/xml",
                SourceUri: null
            );

            return rawContent;
        }

        [Test]
        public void SmallDynamicallyGeneratedFeed()
        {
            var feed = Feed();
            AddItem(feed, 1, 1);

            system.ProcessRawContent(ToContent(feed, Instant.FromUnixTimeSeconds(12345678)));

            // title doc
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.DocumentType == UnprocessedDocumentType.SourceDescription);

                Assert.AreEqual(null, doc.PublishTime);
                Assert.AreEqual(null, doc.UpdateTime);
                Assert.AreEqual("http://dynamic-test-feed.com/atom", doc.Uri);
                Assert.AreEqual(Instant.FromUnixTimeSeconds(12345678), doc.RetrieveTime);
                Assert.AreEqual("dynamic-test-feed", doc.SourceId);

                var unprocessedContent = (doc.Content as FeedSourceDescriptionContent)!;
                Assert.IsNotNull(unprocessedContent);

                Assert.AreEqual(new AtomTextConstruct("text", "Dynamic Test Feed"), unprocessedContent.Title);
                Assert.AreEqual("Dynamic Test Feed description.", unprocessedContent.Description);
                Assert.AreEqual(null, unprocessedContent.IconUri);

                Assert.AreEqual(0, doc.Authors.Count());
            }

            // item
            {
                var doc = system.UnprocessedDocumentRepository.GetAll().First(doc => doc.SourceId == ItemId(1));

                AssertDocumentIsCorrect(doc, 1, 1, Instant.FromUnixTimeSeconds(12345678));
            }
        }

        [TestCase(10)]
        //[TestCase(20)]
        //[TestCase(30)]
        //[TestCase(40)]
        //[TestCase(50)]
        //[TestCase(60)]
        //[TestCase(70)]
        //[TestCase(80)]
        //[TestCase(90)]
        [TestCase(100)]
        //[TestCase(200)]
        //[TestCase(300)]
        //[TestCase(400)]
        //[TestCase(500)]
        //[TestCase(600)]
        //[TestCase(700)]
        //[TestCase(800)]
        //[TestCase(900)]
        [TestCase(1000)]
        public void NItemFeed(int N)
        {
            var feed = Feed();
            AddItemRange(feed, 1, N, 1);

            Instant retrieveTime = Instant.FromUnixTimeSeconds(12345678);
            system.ProcessRawContent(ToContent(feed, retrieveTime));

            for (int i = 1; i <= N; ++i)
            {
                var doc = system.UnprocessedDocumentRepository.GetBySourceId(ItemId(i)).First();
                AssertDocumentIsCorrect(doc, i, 1, retrieveTime);
            }
        }

        [TestCase(10)]
        [TestCase(100)]
        //[TestCase(1000)]
        public void OverlappingTenItemFeedsUpToItemN(int N)
        {
            Instant retrieveTime = Instant.FromUnixTimeSeconds(12345678);

            for (int max = 10; max <= N; ++max)
            {
                var feed = Feed();
                AddItemRange(feed, max - 10 + 1, max, 1);

                system.ProcessRawContent(ToContent(feed, retrieveTime));
            }

            for (int i = 1; i <= N; ++i)
            {
                var doc = system.UnprocessedDocumentRepository.GetBySourceId(ItemId(i)).First();
                AssertDocumentIsCorrect(doc, i, 1, retrieveTime);
            }
        }

        [TestCase(10)]
        [TestCase(100)]
        public void NewVersionsOfFeedUpToVersionN(int N)
        {
            Instant retrieveTime = Instant.FromUnixTimeSeconds(12345678);

            for (int version = 1; version <= N; ++version)
            {
                var feed = Feed();
                AddItemRange(feed, 1, 10, version);

                system.ProcessRawContent(ToContent(feed, retrieveTime));
            }

            for (int i = 1; i <= 10; ++i)
            {
                var docs = system.UnprocessedDocumentRepository.GetBySourceId(ItemId(i)).OrderBy(doc => doc.UpdateTime).ToList();

                for (int version = 1; version <= N; ++version)
                {
                    AssertDocumentIsCorrect(docs[version-1], i, version, retrieveTime);
                }
            }
        }
    }
}
