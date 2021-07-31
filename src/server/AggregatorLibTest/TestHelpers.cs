using AggregatorLib;
using LiteDB;
using NUnit.Framework;
using System.Linq;

namespace AggregatorLibTest
{
    public static class TestHelpers
    {
        public class Container<T>
        {
            [BsonId]
            public int Id = 1;

            public T? Value { get; protected set; }

            public Container() { }

            public Container(T value)
            {
                this.Value = value;
            }
        }

        public static T AssertNotNull<T>(T? value)
        {
            if (value == null)
            {
                Assert.IsNotNull(value);
                throw new System.Exception();   // shouldn't be reached, but will silence the compiler
            }
            else
            {
                return value;
            }
        }

        public static Container<T> InContainer<T>(this T value)
        {
            return new Container<T>(value);
        }

        public static void AssertRawDocumentsAreIdentical(RawDocument expected, RawDocument actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.DocumentUri, actual.DocumentUri);
            Assert.AreEqual(expected.SourceId, actual.SourceId);
            Assert.AreEqual(expected.ParentDocumentUri, actual.ParentDocumentUri);
            Assert.AreEqual(expected.RetrieveTime, actual.RetrieveTime);
            Assert.AreEqual(expected.UpdateTime, actual.UpdateTime);
            Assert.AreEqual(expected.PublishTime, actual.PublishTime);

            Assert.AreEqual((expected.Content as WordpressContent)!.Title, (actual.Content as WordpressContent)!.Title);
            Assert.AreEqual((expected.Content as WordpressContent)!.Content, (actual.Content as WordpressContent)!.Content);
            Assert.IsTrue(Enumerable.SequenceEqual((expected.Content as WordpressContent)!.Categories, (actual.Content as WordpressContent)!.Categories));

            Assert.AreEqual(expected.Author.Name, actual.Author.Name);
            Assert.AreEqual(expected.Author.Context, actual.Author.Context);
        }
    }
}
