using AggregatorLib;
using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static AggregatorLibTest.TestHelpers;

namespace AggregatorLibTest
{
    [TestFixture]
    class TestLiteDBRawDocumentRepository
    {
#pragma warning disable CS8618
        private IRawDocumentRepository repository;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            LiteDBFunctions.DoLiteDBGlobalSetUp();
            repository = new LiteDBRawDocumentRepository(new LiteDatabase(":memory:"));
        }

        private RawDocument TestDocument(int index)
        {
            return new RawDocument
            (
                Id: TestDocumentId(index),
                Uri: $"http://example.com/{index}",
                SourceId: $"{index}",
                ParentDocumentUri: $"http://example.com/{index - 1}",
                RetrieveTime: NodaTime.Instant.FromUnixTimeSeconds(1000000 + index),
                UpdateTime: NodaTime.Instant.FromUnixTimeSeconds(2000000 + index),
                PublishTime: NodaTime.Instant.FromUnixTimeSeconds(3000000 + index),
                Content: new WordpressContent(Title: $"Title {index}", Content: $"Content {index}", Categories: new List<string>() { $"cat1-{index}", $"cat2-{index}" }, AllowsComments: false, CommentUri: null, CommentFeedUri: null),
                Authors: new List<RawDocumentAuthor> { new RawDocumentAuthor($"Author {index}", $"Context {index}") }
            );
        }

        private Guid TestDocumentId(int index)
        {
            return Guid.Parse($"12345678-1234-1234-1234-{index:D12}");
        }

        private IEnumerable<RawDocument> DocumentSequence(IEnumerable<int> sequence)
        {
            foreach (var index in sequence)
            {
                yield return TestDocument(index);
            }
        }

        private IEnumerable<RawDocument> DocumentSequence(params int[] indices)
        {
            return DocumentSequence(indices as IEnumerable<int>);
        }

        [Test]
        public void StateAfterCreation()
        {
            Assert.AreEqual(0, repository.GetAllRawDocuments().Count());
        }

        [Test]
        public void AddOneRawDocument()
        {
            var rawDoc = TestDocument(1);

            repository.AddRawDocument(rawDoc);
            var docById = repository.GetRawDocumentById(rawDoc.Id);
            AssertRawDocumentsAreIdentical(rawDoc, docById);

            Assert.AreEqual(1, repository.GetAllRawDocuments().Count());
            var rawDocInAllRawDocuments = repository.GetAllRawDocuments().First();
            AssertRawDocumentsAreIdentical(rawDocInAllRawDocuments, docById);
        }

        [Test]
        public void GetRawDocumentByIdInEmptyRepository()
        {
            Assert.Throws<RepositoryException>(() => repository.GetRawDocumentById(Guid.NewGuid()));
        }

        [Test]
        public void GetInvalidDocumentIdInNonEmptyRepository()
        {
            for (int i = 1; i <= 100; ++i)
            {
                repository.AddRawDocument(TestDocument(i));
            }

            Assert.Throws<RepositoryException>(() => repository.GetRawDocumentById(TestDocumentId(1000)));
        }

        [Test]
        public void AddTwoRawDocuments()
        {
            repository.AddRawDocument(TestDocument(1));
            repository.AddRawDocument(TestDocument(2));

            AssertRawDocumentsAreIdentical(TestDocument(1), repository.GetRawDocumentById(TestDocumentId(1)));
            AssertRawDocumentsAreIdentical(TestDocument(2), repository.GetRawDocumentById(TestDocumentId(2)));

            var documentsInRepository = repository.GetAllRawDocuments().OrderBy(doc => doc.Id).ToList();

            for (int index = 1; index <= documentsInRepository.Count; ++index)
            {
                AssertRawDocumentsAreIdentical(TestDocument(index), documentsInRepository[index-1]);
            }
        }

        [Test]
        public void AddOneThousandRawDocuments()
        {
            for (int index = 1; index <= 1000; ++index)
            {
                var doc = TestDocument(index);
                repository.AddRawDocument(doc);
                AssertRawDocumentsAreIdentical(doc, repository.GetRawDocumentById(TestDocumentId(index)));
            }

            for (int index = 1; index <= 1000; ++index)
            {
                AssertRawDocumentsAreIdentical(TestDocument(index), repository.GetRawDocumentById(TestDocumentId(index)));
            }

            var documentsInRepository = repository.GetAllRawDocuments().OrderBy(doc => doc.Id).ToList();

            for (int index = 1; index <= documentsInRepository.Count; ++index)
            {
                AssertRawDocumentsAreIdentical(TestDocument(index), documentsInRepository[index - 1]);
            }
        }
    }
}
