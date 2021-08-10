using AggregatorLib;
using LiteDB;
using NUnit.Framework;
using System;
using System.IO;
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

        public static void AssertUnprocessedDocumentsAreIdentical(UnprocessedDocument expected, UnprocessedDocument actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Uri, actual.Uri);
            Assert.AreEqual(expected.SourceId, actual.SourceId);
            Assert.AreEqual(expected.ParentDocumentUri, actual.ParentDocumentUri);
            Assert.AreEqual(expected.RetrieveTime, actual.RetrieveTime);
            Assert.AreEqual(expected.UpdateTime, actual.UpdateTime);
            Assert.AreEqual(expected.PublishTime, actual.PublishTime);
            Assert.AreEqual(expected.SourceRawContentId, actual.SourceRawContentId);

            Assert.AreEqual((expected.Content as BlogPostContent)!.Title, (actual.Content as BlogPostContent)!.Title);
            Assert.AreEqual((expected.Content as BlogPostContent)!.Content, (actual.Content as BlogPostContent)!.Content);
            Assert.IsTrue(Enumerable.SequenceEqual((expected.Content as BlogPostContent)!.Categories, (actual.Content as BlogPostContent)!.Categories));
            Assert.AreEqual((expected.Content as BlogPostContent)!.AllowsComments, (actual.Content as BlogPostContent)!.AllowsComments);
            Assert.AreEqual((expected.Content as BlogPostContent)!.CommentUri, (actual.Content as BlogPostContent)!.CommentUri);
            Assert.AreEqual((expected.Content as BlogPostContent)!.CommentFeedUri, (actual.Content as BlogPostContent)!.CommentFeedUri);

            Assert.AreEqual(expected.Authors.Count, actual.Authors.Count);

            for (int i = 0; i < expected.Authors.Count; ++i)
            {
                Assert.AreEqual(expected.Authors[i].Name, actual.Authors[i].Name);
                Assert.AreEqual(expected.Authors[i].Context, actual.Authors[i].Context);
                Assert.AreEqual(expected.Authors[i].Uri, actual.Authors[i].Uri);
            }
        }

        public static void AssertRawContentIsIdentical(RawContent expected, RawContent actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Type, actual.Type);
            Assert.AreEqual(expected.Content, actual.Content);
            Assert.AreEqual(expected.Context, actual.Context);
            Assert.AreEqual(expected.SourceUri, actual.SourceUri);
        }

        public static Guid TestId(int index)
        {
            return Guid.Parse($"12345678-1234-1234-1234-{index:D12}");
        }

        public static Stream GetTestDataResourceStream(string path)
        {
            string resourceName = $"AggregatorLibTest.TestData.{path}";

            var assembly = typeof(AggregatorLibTest.TestFeedExtractor).Assembly;

            return assembly.GetManifestResourceStream(resourceName) ?? throw new Exception("Null resource stream");
        }
    }
}
