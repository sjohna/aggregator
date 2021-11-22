using Aggregator;
using AggregatorLib;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AggregatorTest.JsonSerialization
{
    [TestFixture]
    public class TestUnprocessedDocumentContentJsonConverter
    {
        private UnprocessedDocumentContentJsonConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new UnprocessedDocumentContentJsonConverter();
        }

        [Test]
        public void BlogPostContent()
        {
            var content = new AggregatorLib.BlogPostContent
                              (
                                  Title: "Test Title",
                                  Content: "Test Content",
                                  Categories: new List<AtomCategory>(),
                                  AllowsComments: true,
                                  CommentUri: "example.com/comments",
                                  CommentFeedUri: "example.com/feed/comments"
                              );

            using (MemoryStream stream = new MemoryStream())
            using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
            {
                converter.Write(writer, content, null);

                var json = Encoding.UTF8.GetString(stream.ToArray());

                Assert.IsTrue(json.Length > 0);
                Assert.IsTrue(Regex.IsMatch(json, "\"ContentType\":\"BlogPost\""));
            }
        }

        [Test]
        public void FeedSourceDescriptionContent()
        {
            var content = new AggregatorLib.FeedSourceDescriptionContent
                              (
                                  Title: "Test Title",
                                  Description: "Test Description",
                                  IconUri: "example.com/icon"
                              );

            using (MemoryStream stream = new MemoryStream())
            using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
            {
                converter.Write(writer, content, null);

                var json = Encoding.UTF8.GetString(stream.ToArray());

                Assert.IsTrue(json.Length > 0);
                Assert.IsTrue(Regex.IsMatch(json, "\"ContentType\":\"FeedSourceDescription\""));
            }
        }

        [Test]
        public void DeletedAtSourceContent()
        {
            var content = new AggregatorLib.DeletedAtSourceContent();

            using (MemoryStream stream = new MemoryStream())
            using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
            {
                converter.Write(writer, content, null);

                var json = Encoding.UTF8.GetString(stream.ToArray());

                Assert.IsTrue(json.Length > 0);
                Assert.IsTrue(Regex.IsMatch(json, "\"ContentType\":\"DeletedAtSource\""));
            }
        }
    }
}
