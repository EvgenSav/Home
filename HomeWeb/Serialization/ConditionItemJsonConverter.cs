using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Home.Web.Domain.Automation.Condition;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;

namespace Home.Web.Serialization
{
    public class ConditionItemJsonConverter : JsonConverter<IConditionItem>
    {
        public override IConditionItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IConditionItem value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
