using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Home.Web.Domain.Automation.Condition;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using JsonTokenType = System.Text.Json.JsonTokenType;

namespace Home.Web.Serialization
{
    public class ConditionItemSerializer : IBsonSerializer<IConditionItem>
    {
        public Type ValueType => typeof(IConditionItem);

        public IConditionItem Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var t = default(IConditionItem);
            context.Reader.ReadStartDocument();
            var obj = new JObject();
            var child = new JObject();
            while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                if(context.Reader.State == BsonReaderState.Type) continue;
                switch (context.Reader.CurrentBsonType)
                {
                    case BsonType.String:
                        obj.Add(context.Reader.ReadName(), context.Reader.ReadString());
                        break;
                    case BsonType.Int32:
                        obj.Add(context.Reader.ReadName(), context.Reader.ReadInt32());
                        break;
                    case BsonType.Document:
                        break;
                }
            }
            context.Reader.ReadEndDocument();
            switch (obj.GetValue(nameof(IConditionItem.Type)).ToObject<ConditionTypeEnum>())
            {
                case ConditionTypeEnum.DeviceCmd:
                    t = new DeviceCmdCondition();
                    break;
                case ConditionTypeEnum.DeviceState:
                    t = new DeviceStateCondition();
                    break;
            }
            JsonConvert.PopulateObject(obj.ToString(), t);
            return t;
        }

        void WriteObject(IBsonWriter writer, object value)
        {
            writer.WriteStartDocument();
            foreach (var propInfo in value?.GetType().GetProperties() ?? new PropertyInfo[]{ })
            {
                writer.WriteName(propInfo.Name);
                if (propInfo.PropertyType == typeof(int) || propInfo.PropertyType == typeof(int?))
                {
                    var val = (int?)propInfo.GetValue(value);
                    if (val.HasValue) writer.WriteInt32(val.Value);
                    else writer.WriteNull();
                }
                else if (typeof(Enum).IsAssignableFrom(propInfo.PropertyType))
                {
                    writer.WriteInt32((int)propInfo.GetValue(value));
                }
                else if (propInfo.PropertyType == typeof(string) || propInfo.PropertyType == typeof(Guid))
                {
                    writer.WriteString(propInfo.GetValue(value).ToString());
                }
                else if (typeof(object).IsAssignableFrom(propInfo.PropertyType))
                {
                    WriteObject(writer, propInfo.GetValue(value));
                }
            }
            writer.WriteEndDocument();
        }
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IConditionItem value)
        {
            WriteObject(context.Writer, value);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            context.Writer.WriteStartDocument();
            foreach (var propInfo in value.GetType().GetProperties())
            {
                context.Writer.WriteName(propInfo.Name);
                if (propInfo.PropertyType == typeof(int))
                {
                    
                    context.Writer.WriteInt32((int)propInfo.GetValue(value));
                }
                else if(propInfo.PropertyType == typeof(string))
                {
                    context.Writer.WriteString((string)propInfo.GetValue(value));
                }
            }
            context.Writer.WriteEndDocument();
            context.Writer.Flush();
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
