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

namespace AggregatorLibTest
{
    [TestFixture]
    class TestLiteDBRawContentRepository
    {
#pragma warning disable CS8618
        private IRawContentRepository repository;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            LiteDBFunctions.DoLiteDBGlobalSetUp();
            repository = new LiteDBRawContentRepository(new LiteDatabase(":memory:"));
        }

        private RawContent TestContent(int index)
        {
            return new RawContent
            (
                Id: TestId(index),
                RetrieveTime: Instant.FromUnixTimeSeconds(12345678),
                Type: $"Type {index}",
                Content: $"Content {index}",
                Context: $"Context {index}",
                SourceUri: $"http://example.com/atom-{index}.xml"
            );
        }

        [Test]
        public void StateAfterCreation()
        {
            Assert.AreEqual(0, repository.GetAllRawContent().Count());
        }

        [Test]
        public void AddOneRawContent()
        {
            var rawContent = TestContent(1);

            repository.AddRawContent(rawContent);
            var rawContentById = repository.GetRawContentById(rawContent.Id);
            Assert.IsNotNull(rawContentById);

            AssertRawContentIsIdentical(rawContent, rawContentById!);

            Assert.AreEqual(1, repository.GetAllRawContent().Count());
            var rawContentInAllRawContent = repository.GetAllRawContent().First();
            AssertRawContentIsIdentical(rawContent, rawContentInAllRawContent);
        }

        [Test]
        public void GetRawContentByIdInEmptyRepository()
        {
            Assert.IsNull(repository.GetRawContentById(Guid.NewGuid()));
        }

        [Test]
        public void GetInvalidRawContentIdInNonEmptyRepository()
        {
            for (int i = 1; i <= 100; ++i)
            {
                repository.AddRawContent(TestContent(i));
            }

            Assert.IsNull(repository.GetRawContentById(TestId(1000)));
        }

        [Test]
        public void AddTwoRawContent()
        {
            repository.AddRawContent(TestContent(1));
            repository.AddRawContent(TestContent(2));

            AssertRawContentIsIdentical(TestContent(1), repository.GetRawContentById(TestId(1))!);
            AssertRawContentIsIdentical(TestContent(2), repository.GetRawContentById(TestId(2))!);

            var contentInRepository = repository.GetAllRawContent().OrderBy(doc => doc.Id).ToList();

            for (int index = 1; index <= contentInRepository.Count; ++index)
            {
                AssertRawContentIsIdentical(TestContent(index), contentInRepository[index - 1]);
            }
        }

        [Test]
        public void StoringRawContentDoesNotTrimWhitespace()
        {
            var content = new RawContent
            (
                Id: TestId(1),
                RetrieveTime: Instant.FromUnixTimeSeconds(12345678),
                Type: "Type 1",
                Content: "Content with trailing newlines\n\n",
                Context: "Context 1",
                SourceUri: "http://example.com/atom-1.xml"
            );

            repository.AddRawContent(content);

            var contentInRepository = repository.GetAllRawContent().First();

            AssertRawContentIsIdentical(content, contentInRepository);
        }
    }
}
