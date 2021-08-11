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
        public void UnprocessedDocumentAuthor()
        {
            var instance = new UnprocessedDocumentAuthor("Test Author", "Test Context", "http://example.com/test");

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
        public void UnprocessedBlogPostContent()
        {
            var instance = TestWordpressContent();

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual("Test Title", instanceInDatabase.Title);
            Assert.AreEqual("Test Content", instanceInDatabase.Content);
            Assert.IsTrue(Enumerable.SequenceEqual(instance.Categories, instanceInDatabase.Categories));
        }

        [Test]
        public void UnprocessedDocument()
        {
            var instance = new UnprocessedDocument
            (
                Id: Guid.NewGuid(),
                Uri: "http://example.com/1234",
                SourceId: "1234",
                ParentDocumentUri: "http://example.com/1233",
                RetrieveTime: NodaTime.Instant.FromUnixTimeSeconds(1000000),
                UpdateTime: NodaTime.Instant.FromUnixTimeSeconds(2000000),
                PublishTime: NodaTime.Instant.FromUnixTimeSeconds(3000000),
                Content: TestWordpressContent(),
                Authors: new List<UnprocessedDocumentAuthor>() { new UnprocessedDocumentAuthor("Test Author", "Test Context") },
                SourceRawContentId: Guid.NewGuid()
            );

            var instanceInDatabase = StoreAndRetrieve(instance);

            AssertUnprocessedDocumentsAreIdentical(instance, instanceInDatabase);
        }

        [Test]
        public void UnprocessedDocumentWithNullPublishTime()
        {
            var instance = new UnprocessedDocument
            (
                Id: Guid.NewGuid(),
                Uri: "http://example.com/1234",
                SourceId: "1234",
                ParentDocumentUri: "http://example.com/1233",
                RetrieveTime: NodaTime.Instant.FromUnixTimeSeconds(1000000),
                UpdateTime: NodaTime.Instant.FromUnixTimeSeconds(2000000),
                PublishTime: null,
                Content: TestWordpressContent(),
                Authors: new List<UnprocessedDocumentAuthor>() { new UnprocessedDocumentAuthor("Test Author", "Test Context") },
                SourceRawContentId: Guid.NewGuid()
            );

            var instanceInDatabase = StoreAndRetrieve(instance);

            AssertUnprocessedDocumentsAreIdentical(instance, instanceInDatabase);
        }

        [Test]
        public void RawContentWithSpecificGuidPassedIn()
        {
            var id = Guid.NewGuid();

            var instance = new RawContent
            (
                Id: id,
                RetrieveTime: NodaTime.Instant.FromUnixTimeMilliseconds(123454321),
                Type: "testtype",
                Content: "Test Content",
                Context: "Test Context",
                SourceUri: "http://example.com"
            );

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual(id, instanceInDatabase.Id);
            Assert.AreEqual("testtype", instanceInDatabase.Type);
            Assert.AreEqual("Test Content", instanceInDatabase.Content);
            Assert.AreEqual("Test Context", instanceInDatabase.Context);
            Assert.AreEqual("http://example.com", instanceInDatabase.SourceUri);
        }

        [Test]
        public void RawContentWithGuidSetAutomatically()
        {
            var instance = new RawContent
            (
                RetrieveTime: NodaTime.Instant.FromUnixTimeMilliseconds(123454321),
                Type: "testtype",
                Content: "Test Content",
                Context: "Test Context",
                SourceUri: "http://example.com"
            );

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual(instance.Id, instanceInDatabase.Id);
            Assert.AreEqual("testtype", instanceInDatabase.Type);
            Assert.AreEqual("Test Content", instanceInDatabase.Content);
            Assert.AreEqual("Test Context", instanceInDatabase.Context);
            Assert.AreEqual("http://example.com", instanceInDatabase.SourceUri);
        }

        [Test]
        public void FeedSourceDescriptionContent()
        {
            var instance = new FeedSourceDescriptionContent
            (
                Title: "Test Title",
                Description: "Test Description",
                IconUri: "http://example.com/icon.jpg"
            );

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual("Test Title", instanceInDatabase.Title);
            Assert.AreEqual("Test Description", instanceInDatabase.Description);
            Assert.AreEqual("http://example.com/icon.jpg", instanceInDatabase.IconUri);
        }
    }
}
