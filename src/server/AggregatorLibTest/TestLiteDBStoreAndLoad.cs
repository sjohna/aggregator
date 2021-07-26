using AggregatorLib;
using LiteDB;
using NUnit.Framework;
using System.Linq;
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
    }
}
