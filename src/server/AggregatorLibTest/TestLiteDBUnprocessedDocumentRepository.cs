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
    class TestLiteDBUnprocessedDocumentRepository
    {
#pragma warning disable CS8618
        private IUnprocessedDocumentRepository repository;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            LiteDBFunctions.DoLiteDBGlobalSetUp();
            repository = new LiteDBUnprocessedDocumentRepository(new LiteDatabase(":memory:"));
        }

        private UnprocessedDocument TestDocument(int index)
        {
            return new UnprocessedDocument
            (
                Id: TestId(index),
                Uri: $"http://example.com/{index}",
                SourceId: $"{index}",
                ParentDocumentUri: $"http://example.com/{index - 1}",
                RetrieveTime: NodaTime.Instant.FromUnixTimeSeconds(1000000 + index),
                UpdateTime: NodaTime.Instant.FromUnixTimeSeconds(2000000 + index),
                PublishTime: NodaTime.Instant.FromUnixTimeSeconds(3000000 + index),
                Content: new BlogPostContent(Title: $"Title {index}", Content: $"Content {index}", Categories: new List<string>() { $"cat1-{index}", $"cat2-{index}" }, AllowsComments: false, CommentUri: null, CommentFeedUri: null),
                Authors: new List<UnprocessedDocumentAuthor> { new UnprocessedDocumentAuthor($"Author {index}", $"Context {index}") },
                SourceRawContentId: Guid.Parse("00000000-0000-0000-0000-000000001234")
            );
        }

        [Test]
        public void StateAfterCreation()
        {
            Assert.AreEqual(0, repository.GetAllUnprocessedDocuments().Count());
        }

        [Test]
        public void AddOneUnprocessedDocument()
        {
            var unprocessedDoc = TestDocument(1);

            repository.AddUnprocessedDocument(unprocessedDoc);
            var docById = repository.GetUnprocessedDocumentById(unprocessedDoc.Id);
            AssertUnprocessedDocumentsAreIdentical(unprocessedDoc, docById);

            Assert.AreEqual(1, repository.GetAllUnprocessedDocuments().Count());
            var unprocessedDocInAllDocuments = repository.GetAllUnprocessedDocuments().First();
            AssertUnprocessedDocumentsAreIdentical(unprocessedDoc, unprocessedDocInAllDocuments);
        }

        [Test]
        public void GetUnprocessedDocumentByIdInEmptyRepository()
        {
            Assert.IsNull(repository.GetUnprocessedDocumentById(Guid.NewGuid()));
        }

        [Test]
        public void GetInvalidUnprocessedDocumentIdInNonEmptyRepository()
        {
            for (int i = 1; i <= 100; ++i)
            {
                repository.AddUnprocessedDocument(TestDocument(i));
            }

            Assert.IsNull(repository.GetUnprocessedDocumentById(TestId(1000)));
        }

        [Test]
        public void AddTwoUnprocessedDocuments()
        {
            repository.AddUnprocessedDocument(TestDocument(1));
            repository.AddUnprocessedDocument(TestDocument(2));

            AssertUnprocessedDocumentsAreIdentical(TestDocument(1), repository.GetUnprocessedDocumentById(TestId(1)));
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), repository.GetUnprocessedDocumentById(TestId(2)));

            var documentsInRepository = repository.GetAllUnprocessedDocuments().OrderBy(doc => doc.Id).ToList();

            for (int index = 1; index <= documentsInRepository.Count; ++index)
            {
                AssertUnprocessedDocumentsAreIdentical(TestDocument(index), documentsInRepository[index-1]);
            }
        }
    }
}
