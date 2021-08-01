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
            Assert.AreEqual(expected.Uri, actual.Uri);
            Assert.AreEqual(expected.SourceId, actual.SourceId);
            Assert.AreEqual(expected.ParentDocumentUri, actual.ParentDocumentUri);
            Assert.AreEqual(expected.RetrieveTime, actual.RetrieveTime);
            Assert.AreEqual(expected.UpdateTime, actual.UpdateTime);
            Assert.AreEqual(expected.PublishTime, actual.PublishTime);

            Assert.AreEqual((expected.Content as WordpressContent)!.Title, (actual.Content as WordpressContent)!.Title);
            Assert.AreEqual((expected.Content as WordpressContent)!.Content, (actual.Content as WordpressContent)!.Content);
            Assert.IsTrue(Enumerable.SequenceEqual((expected.Content as WordpressContent)!.Categories, (actual.Content as WordpressContent)!.Categories));
            Assert.AreEqual((expected.Content as WordpressContent)!.AllowsComments, (actual.Content as WordpressContent)!.AllowsComments);
            Assert.AreEqual((expected.Content as WordpressContent)!.CommentUri, (actual.Content as WordpressContent)!.CommentUri);
            Assert.AreEqual((expected.Content as WordpressContent)!.CommentFeedUri, (actual.Content as WordpressContent)!.CommentFeedUri);

            Assert.AreEqual(expected.Authors.Count, actual.Authors.Count);

            for (int i = 0; i < expected.Authors.Count; ++i)
            {
                Assert.AreEqual(expected.Authors[i].Name, actual.Authors[i].Name);
                Assert.AreEqual(expected.Authors[i].Context, actual.Authors[i].Context);
                Assert.AreEqual(expected.Authors[i].Uri, actual.Authors[i].Uri);
            }
        }
    }
}
