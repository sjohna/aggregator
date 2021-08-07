using AggregatorLib;
using LiteDB;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using static AggregatorLibTest.TestHelpers;

namespace AggregatorLibTest
{
    [TestFixture]
    class TestLiteDBStoreAndRetrieve
    {
        private LiteDatabase? database;

        [SetUp]
        public void SetUp()
        {
            LiteDBFunctions.DoLiteDBGlobalSetUp();
            database = new LiteDatabase(":memory:");
        }

        private T StoreAndRetrieve<T>(T value)
        {
            var container = value.InContainer();
            var collection = database!.GetCollection<Container<T>>("test");
            collection.Insert(container);
            return AssertNotNull(collection.FindAll().First().Value);
        }

        private BlogPostContent TestWordpressContent()
        {
            return new BlogPostContent
            (
                Title: "Test Title",
                Content: "Test Content",
                Categories: new List<string>() { "cat1", "cat2", "cat3" },
                AllowsComments: true,
                CommentUri: "http://example.com/1234/comments",
                CommentFeedUri: "http://example.com/1234/comments/feed"
            );
        }

        [Test]
        public void Instant()
        {
            var instance = NodaTime.Instant.FromUnixTimeSeconds(123456789L);

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual(instance, instanceInDatabase);
        }

        [Test]
        public void RawDocumentAuthor()
        {
            var instance = new RawDocumentAuthor("Test Author", "Test Context", "http://example.com/test");

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual("Test Author", instanceInDatabase.Name);
            Assert.AreEqual("Test Context", instanceInDatabase.Context);
            Assert.AreEqual("http://example.com/test", instanceInDatabase.Uri);
        }

        [Test]
        public void DeletedAtSourceContent()
        {
            var instance = new DeletedAtSourceContent();

            Assert.DoesNotThrow(() => StoreAndRetrieve(instance));
        }

        [Test]
        public void RawWordpressContent()
        {
            var instance = TestWordpressContent();

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual("Test Title", instanceInDatabase.Title);
            Assert.AreEqual("Test Content", instanceInDatabase.Content);
            Assert.IsTrue(Enumerable.SequenceEqual(instance.Categories, instanceInDatabase.Categories));
        }

        [Test]
        public void RawDocument()
        {
            var instance = new RawDocument
            (
                Id: Guid.NewGuid(),
                Uri: "http://example.com/1234",
                SourceId: "1234",
                ParentDocumentUri: "http://example.com/1233",
                RetrieveTime: NodaTime.Instant.FromUnixTimeSeconds(1000000),
                UpdateTime: NodaTime.Instant.FromUnixTimeSeconds(2000000),
                PublishTime: NodaTime.Instant.FromUnixTimeSeconds(3000000),
                Content: TestWordpressContent(),
                Authors: new List<RawDocumentAuthor>() { new RawDocumentAuthor("Test Author", "Test Context") }
            );

            var instanceInDatabase = StoreAndRetrieve(instance);

            AssertRawDocumentsAreIdentical(instance, instanceInDatabase);
        }

        [Test]
        public void RawDocumentWithNullPublishTime()
        {
            var instance = new RawDocument
            (
                Id: Guid.NewGuid(),
                Uri: "http://example.com/1234",
                SourceId: "1234",
                ParentDocumentUri: "http://example.com/1233",
                RetrieveTime: NodaTime.Instant.FromUnixTimeSeconds(1000000),
                UpdateTime: NodaTime.Instant.FromUnixTimeSeconds(2000000),
                PublishTime: null,
                Content: TestWordpressContent(),
                Authors: new List<RawDocumentAuthor>() { new RawDocumentAuthor("Test Author", "Test Context") }
            );

            var instanceInDatabase = StoreAndRetrieve(instance);

            AssertRawDocumentsAreIdentical(instance, instanceInDatabase);
        }
    }
}
