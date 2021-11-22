using AggregatorLib;
using LiteDB;
using NodaTime;
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

        private IReadOnlyList<AtomCategory> Categories(int index)
        {
            return new List<AtomCategory>()
            {
                new AtomCategory($"cat1-{index}"),
                new AtomCategory($"cat2-{index}", $"cat2-{index}-schema"),
                new AtomCategory($"cat3-{index}", $"cat3-{index}-schema", $"cat3-{index}-label")
            };
        }

        private UnprocessedDocument TestDocument(int index)
        {
            return new UnprocessedDocument
            (
                Id: TestId(index),
                Uri: $"http://example.com/{index}",
                SourceId: $"{index}",
                ParentDocumentUri: $"http://example.com/{index - 1}",
                RetrieveTime: Instant.FromUnixTimeSeconds(1000000 + index),
                UpdateTime: Instant.FromUnixTimeSeconds(2000000 + index),
                PublishTime: Instant.FromUnixTimeSeconds(3000000 + index),
                Content: new BlogPostContent(Title: $"Title {index}", Content: $"Content {index}", Categories: Categories(index), AllowsComments: false, CommentUri: null, CommentFeedUri: null),
                Authors: new List<UnprocessedDocumentAuthor> { new UnprocessedDocumentAuthor($"Author {index}", $"Context {index}") },
                SourceRawContentId: Guid.Parse("00000000-0000-0000-0000-000000001234")
            );
        }

        private UnprocessedDocument TestDocumentWithUpdateTime(int index, Instant? UpdateTime, Guid? Id = null)
        {
            return new UnprocessedDocument
            (
                Id: Id != null ? Id.Value : TestId(index),
                Uri: $"http://example.com/{index}",
                SourceId: $"{index}",
                ParentDocumentUri: $"http://example.com/{index - 1}",
                RetrieveTime: Instant.FromUnixTimeSeconds(1000000 + index),
                UpdateTime: UpdateTime,
                PublishTime: Instant.FromUnixTimeSeconds(3000000 + index),
                Content: new BlogPostContent(Title: $"Title {index}", Content: $"Content {index}", Categories: Categories(index), AllowsComments: false, CommentUri: null, CommentFeedUri: null),
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
        public void GetLatestForSourceIdInEmptyRepository()
        {
            Assert.IsNull(repository.GetLatestForSourceId("1"));
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
        public void GetLatestForNonPresentSourceIdInNonEmptyRepository()
        {
            for (int i = 1; i <= 100; ++i)
            {
                repository.Add(TestDocument(i));
            }

            Assert.IsNull(repository.GetLatestForSourceId("1000"));
        }

        [Test]
        public void GetLatestForSourceIdWithOneDocumentWithNullUpdateTime()
        {
            repository.Add(TestDocumentWithUpdateTime(1, null));

            AssertUnprocessedDocumentsAreIdentical(TestDocumentWithUpdateTime(1, null), repository.GetLatestForSourceId("1"));
        }

        [Test]
        public void GetLatestForSourceIdWhenOneDocumentHasNullUpdateTime()
        {
            var id = Guid.NewGuid();
            repository.Add(TestDocumentWithUpdateTime(1, null));
            repository.Add(TestDocumentWithUpdateTime(1, Instant.FromUnixTimeSeconds(3000000), id));

            AssertUnprocessedDocumentsAreIdentical(TestDocumentWithUpdateTime(1, Instant.FromUnixTimeSeconds(3000000), id), repository.GetLatestForSourceId("1"));
        }

        [Test]
        public void AddTwoUnprocessedDocuments()
        {
            repository.Add(TestDocument(1));
            repository.Add(TestDocument(2));

            AssertUnprocessedDocumentsAreIdentical(TestDocument(1), repository.GetById(TestId(1))!);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), repository.GetById(TestId(2))!);

            var documentsInRepository = repository.GetAll().OrderBy(doc => doc.Id).ToList();

            for (int index = 1; index <= documentsInRepository.Count; ++index)
            {
                AssertUnprocessedDocumentsAreIdentical(TestDocument(index), documentsInRepository[index-1]);
            }

            AssertUnprocessedDocumentsAreIdentical(TestDocument(1), repository.GetBySourceId("1").First());
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), repository.GetBySourceId("2").First());

            AssertUnprocessedDocumentsAreIdentical(TestDocument(1), repository.GetLatestForSourceId("1"));
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), repository.GetLatestForSourceId("2"));
        }

        [Test]
        public void QuerySingleDocument()
        {
            repository.Add(TestDocument(1));
            var queryResult = repository.Query(Where: "SourceID = '1'").ToList();

            Assert.AreEqual(1, queryResult.Count);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(1), queryResult[0]);
        }

        [Test]
        public void QueryReturnsNoDocuments()
        {
            repository.Add(TestDocument(1));
            var queryResult = repository.Query(Where: "SourceID = '2'").ToList();

            Assert.AreEqual(0, queryResult.Count);
        }

        [Test]
        public void QueryForAuthor()
        {
            repository.Add(TestDocument(1));
            repository.Add(TestDocument(2));
            repository.Add(TestDocument(3));
            var queryResult = repository.Query(Where: "Authors[0].Name = 'Author 2'").ToList();

            Assert.AreEqual(1, queryResult.Count);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), queryResult[0]);
        }

        //[Test]
        //public void QueryGuidField()
        //{
        //    repository.Add(TestDocument(1));
        //    repository.Add(TestDocument(2));
        //    repository.Add(TestDocument(3));
        //    var queryResult = repository.Query(Where: $"Id = '{TestDocument(3).Id}'").ToList();

        //    Assert.AreEqual(1, queryResult.Count);
        //    AssertUnprocessedDocumentsAreIdentical(TestDocument(3), queryResult[0]);
        //}

        [Test]
        public void QueryUpdateTimeField()
        {
            repository.Add(TestDocument(1));
            repository.Add(TestDocument(2));
            repository.Add(TestDocument(3));
            var queryResult = repository.Query(Where: $"UpdateTime = '{NodaTime.Text.InstantPattern.ExtendedIso.Format(TestDocument(3).UpdateTime!.Value)}'").ToList();

            Assert.AreEqual(1, queryResult.Count);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(3), queryResult[0]);
        }

        [Test]
        public void QueryAfterSpecificTime()
        {
            repository.Add(TestDocument(1));
            repository.Add(TestDocument(2));
            repository.Add(TestDocument(3));
            var queryResult = repository.Query(
                    Where: $"UpdateTime >= '{NodaTime.Text.InstantPattern.ExtendedIso.Format(TestDocument(2).UpdateTime!.Value)}'",
                    OrderByAsc: "UpdateTime"
                ).ToList();

            Assert.AreEqual(2, queryResult.Count);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), queryResult[0]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(3), queryResult[1]);
        }

        [Test]
        public void QueryAfterSpecificTimeInDescendingOrder()
        {
            repository.Add(TestDocument(1));
            repository.Add(TestDocument(2));
            repository.Add(TestDocument(3));
            var queryResult = repository.Query(
                    Where: $"UpdateTime >= '{NodaTime.Text.InstantPattern.ExtendedIso.Format(TestDocument(2).UpdateTime!.Value)}'",
                    OrderByDesc: "UpdateTime"
                ).ToList();

            Assert.AreEqual(2, queryResult.Count);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(3), queryResult[0]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(2), queryResult[1]);
        }

        [Test]
        public void QueryAfterSpecificTimeAndLimit()
        {
            for (int i = 1; i <= 10; ++i)
            {
                repository.Add(TestDocument(i));
            }

            var queryResult = repository.Query(
                    Where: $"UpdateTime >= '{NodaTime.Text.InstantPattern.ExtendedIso.Format(TestDocument(4).UpdateTime!.Value)}'",
                    OrderByAsc: "UpdateTime",
                    Limit: 3
                ).ToList();

            Assert.AreEqual(3, queryResult.Count);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(4), queryResult[0]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(5), queryResult[1]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(6), queryResult[2]);
        }

        [Test]
        public void QueryAfterSpecificTimeAndOffset()
        {
            for (int i = 1; i <= 10; ++i)
            {
                repository.Add(TestDocument(i));
            }

            var queryResult = repository.Query(
                    Where: $"UpdateTime >= '{NodaTime.Text.InstantPattern.ExtendedIso.Format(TestDocument(4).UpdateTime!.Value)}'",
                    OrderByAsc: "UpdateTime",
                    Offset: 2
                ).ToList();

            Assert.AreEqual(5, queryResult.Count);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(6), queryResult[0]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(7), queryResult[1]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(8), queryResult[2]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(9), queryResult[3]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(10), queryResult[4]);
        }

        [Test]
        public void QueryAfterSpecificTimeAndOffsetAndLimit()
        {
            for (int i = 1; i <= 10; ++i)
            {
                repository.Add(TestDocument(i));
            }

            var queryResult = repository.Query(
                    Where: $"UpdateTime >= '{NodaTime.Text.InstantPattern.ExtendedIso.Format(TestDocument(4).UpdateTime!.Value)}'",
                    OrderByAsc: "UpdateTime",
                    Offset: 2,
                    Limit: 4
                ).ToList();

            Assert.AreEqual(4, queryResult.Count);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(6), queryResult[0]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(7), queryResult[1]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(8), queryResult[2]);
            AssertUnprocessedDocumentsAreIdentical(TestDocument(9), queryResult[3]);
        }
    }
}
