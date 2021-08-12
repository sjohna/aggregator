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
            Assert.AreEqual(0, repository.GetAll().Count());
        }

        [Test]
        public void AddOneUnprocessedDocument()
        {
            var unprocessedDoc = TestDocument(1);

            repository.Add(unprocessedDoc);
            var docById = repository.GetById(unprocessedDoc.Id)!;

            Assert.IsNotNull(docById);
            AssertUnprocessedDocumentsAreIdentical(unprocessedDoc, docById);

            Assert.AreEqual(1, repository.GetAll().Count());
            var unprocessedDocInAllDocuments = repository.GetAll().First();
            AssertUnprocessedDocumentsAreIdentical(unprocessedDoc, unprocessedDocInAllDocuments);

            AssertUnprocessedDocumentsAreIdentical(TestDocument(1), repository.GetBySourceId("1").First());
        }

        [Test]
        public void GetByIdInEmptyRepository()
        {
            Assert.IsNull(repository.GetById(Guid.NewGuid()));
        }

        [Test]
        public void GetBySourceIdInEmptyRepository()
        {
            Assert.AreEqual(0, repository.GetBySourceId("1").Count());
        }

        [Test]
        public void GetInvalidUnprocessedDocumentIdInNonEmptyRepository()
        {
            for (int i = 1; i <= 100; ++i)
            {
                repository.Add(TestDocument(i));
            }

            Assert.IsNull(repository.GetById(TestId(1000)));
        }

        [Test]
        public void GetNonPresentSourceIdInNonEmptyRepository()
        {
            for (int i = 1; i <= 100; ++i)
            {
                repository.Add(TestDocument(i));
            }

            Assert.AreEqual(0, repository.GetBySourceId("1000").Count());
        }

        [Test]
        public void AddTwoUnprocessedDocuments()
        {
            repository.Add(TestDocument(1));
            repository.Add(TestDocument(2));

            AssertUnprocessedDocumentsAreIdentical(TestDocument(1), repository.GetById(TestId(1)));
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), repository.GetById(TestId(2)));

            var documentsInRepository = repository.GetAll().OrderBy(doc => doc.Id).ToList();

            for (int index = 1; index <= documentsInRepository.Count; ++index)
            {
                AssertUnprocessedDocumentsAreIdentical(TestDocument(index), documentsInRepository[index-1]);
            }

            AssertUnprocessedDocumentsAreIdentical(TestDocument(1), repository.GetBySourceId("1").First());
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), repository.GetBySourceId("2").First());
        }
    }
}
