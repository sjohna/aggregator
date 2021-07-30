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
            var instance = new RawDocumentAuthor("Test Author", "Test Context");

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual("Test Author", instanceInDatabase.Name);
            Assert.AreEqual("Test Context", instanceInDatabase.Context);
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
            var instance = new WordpressContent
            (
                Title: "Test Title",
                Content: "Test Content",
                Categories: new List<string>() { "cat1", "cat2", "cat3" }
            );

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
                DocumentUri: "http://example.com/1234",
                SourceId: "1234",
                ParentDocumentUri: "http://example.com/1233",
                RetrieveTime: NodaTime.Instant.FromUnixTimeSeconds(1000000),
                UpdateTime: NodaTime.Instant.FromUnixTimeSeconds(2000000),
                PublishTime: NodaTime.Instant.FromUnixTimeSeconds(3000000),
                Content: new WordpressContent(Title: "Test Title", Content: "Test Content", Categories: new List<string>() { "cat1", "cat2", "cat3" }),
                Author: new RawDocumentAuthor("Test Author", "Test Context")
            );

            var instanceInDatabase = StoreAndRetrieve(instance);

            Assert.AreEqual(instance.Id, instanceInDatabase.Id);
            Assert.AreEqual(instance.DocumentUri, instanceInDatabase.DocumentUri);
            Assert.AreEqual(instance.SourceId, instanceInDatabase.SourceId);
            Assert.AreEqual(instance.ParentDocumentUri, instanceInDatabase.ParentDocumentUri);
            Assert.AreEqual(instance.RetrieveTime, instanceInDatabase.RetrieveTime);
            Assert.AreEqual(instance.UpdateTime, instanceInDatabase.UpdateTime);
            Assert.AreEqual(instance.PublishTime, instanceInDatabase.PublishTime);

            Assert.AreEqual((instance.Content as WordpressContent)!.Title, (instanceInDatabase.Content as WordpressContent)!.Title);
            Assert.AreEqual((instance.Content as WordpressContent)!.Content, (instanceInDatabase.Content as WordpressContent)!.Content);
            Assert.IsTrue(Enumerable.SequenceEqual((instance.Content as WordpressContent)!.Categories, (instanceInDatabase.Content as WordpressContent)!.Categories));

            Assert.AreEqual(instance.Author.Name, instanceInDatabase.Author.Name);
            Assert.AreEqual(instance.Author.Context, instanceInDatabase.Author.Context);
        }
    }
}
