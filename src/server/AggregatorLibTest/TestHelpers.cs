using AggregatorLib;
using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            Assert.IsNotNull(value);
            return value!;
        }

        public static Container<T> InContainer<T>(this T value)
        {
            return new Container<T>(value);
        }

        public static void AssertUnprocessedDocumentsAreIdentical(UnprocessedDocument expected, UnprocessedDocument actual)
        {
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Uri, actual.Uri);
            Assert.AreEqual(expected.SourceId, actual.SourceId);
            Assert.AreEqual(expected.RetrieveTime, actual.RetrieveTime);
            Assert.AreEqual(expected.UpdateTime, actual.UpdateTime);
            Assert.AreEqual(expected.PublishTime, actual.PublishTime);
            Assert.AreEqual(expected.SourceRawContentId, actual.SourceRawContentId);
            Assert.AreEqual(expected.DocumentType, actual.DocumentType);

            // TODO: handle different content types
            Assert.AreEqual((expected.Content as AtomContent)!.Title, (actual.Content as AtomContent)!.Title);
            Assert.AreEqual((expected.Content as AtomContent)!.Content, (actual.Content as AtomContent)!.Content);
            Assert.IsTrue(Enumerable.SequenceEqual((expected.Content as AtomContent)!.Categories, (actual.Content as AtomContent)!.Categories));
            Assert.IsTrue(Enumerable.SequenceEqual((expected.Content as AtomContent)!.Links, (actual.Content as AtomContent)!.Links));

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
            Assert.IsNotNull(actual);

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
