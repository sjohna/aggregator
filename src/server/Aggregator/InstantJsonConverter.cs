using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aggregator
{
    public class InstantJsonConverter : JsonConverter<Instant>
    {
        public override Instant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return NodaTime.Text.InstantPattern.ExtendedIso.Parse(reader.GetString()).Value;
        }

        public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(NodaTime.Text.InstantPattern.ExtendedIso.Format(value));
        }
    }
}
