using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AggregatorLib
{
    public class UnprocessedDocumentContentJsonConverter : JsonConverter<UnprocessedDocumentContent>
    {
        public override UnprocessedDocumentContent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, UnprocessedDocumentContent value, JsonSerializerOptions options)
        {
            if (value is BlogPostContent bpc)
            {
                JsonSerializer.Serialize<BlogPostContent>(writer, bpc, options);
            }
        }
    }
}
