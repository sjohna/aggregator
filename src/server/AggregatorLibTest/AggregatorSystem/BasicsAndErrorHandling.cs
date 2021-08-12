using AggregatorLib;
using LiteDB;
using NodaTime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AggregatorLibTest.TestHelpers;

namespace AggregatorLibTest.TestAggregatorSystem
{
    [TestFixture]
    public class BasicsAndErrorHandling
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
        public void ProcessingContentWithExistingIdThrowsException()
        {
            var content = RawContentForEmbeddedAtomFeed("singlePost.xml", Instant.FromUnixTimeSeconds(12345678));
            system.ProcessRawContent(content);

            Assert.Throws<AggregatorSystemException>(() => system.ProcessRawContent(content));
        }
    }
}
