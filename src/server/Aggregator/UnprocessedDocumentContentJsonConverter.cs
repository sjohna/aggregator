using AggregatorLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aggregator
{
    public class UnprocessedDocumentContentJsonConverter : JsonConverter<UnprocessedDocumentContent>
    {
        public override UnprocessedDocumentContent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, UnprocessedDocumentContent value, JsonSerializerOptions options)
        {
            if (value is UnprocessedAtomContent bpc)
            {
                JsonSerializer.Serialize<UnprocessedAtomContent>(writer, bpc, options);
            }
            else if (value is FeedSourceDescriptionContent fsdc)
            {
                JsonSerializer.Serialize<FeedSourceDescriptionContent>(writer, fsdc, options);
            }
            else if (value is DeletedAtSourceContent dasc)
            {
                JsonSerializer.Serialize<DeletedAtSourceContent>(writer, dasc, options);
            }
        }
    }
}
