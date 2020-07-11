using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace Home.Web.Serialization
{
    public class ObjectIdConverter : JsonConverter<ObjectId>
    {
        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.TryGetBytesFromBase64(out var bytes);
            var str = Convert.ToBase64String(bytes);
            return ObjectId.Parse(str);
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            var stringVal = value.ToString();
            var bytes = Convert.FromBase64String(stringVal);
            writer.WriteBase64StringValue(bytes);
        }
    }
}
