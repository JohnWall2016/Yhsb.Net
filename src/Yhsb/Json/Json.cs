using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Yhsb.Json
{
    public class JsonField
    {
        public object Value { get; set; } = default;

        public virtual string Name => $"{Value}";

        public override string ToString() => Name;
    }

    public class JsonFieldConverter : JsonConverter
    {
        public override void WriteJson(
            JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((value as JsonField).Value);
        }

        public override object ReadJson(
            JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            var constructor = objectType.GetConstructor(new Type[0]);
            if (constructor != null)
            {
                var field = constructor.Invoke(null, null) as JsonField;
                field.Value = reader.Value;
                return field;
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(JsonField));
        }
    }

    public static class JsonExtension
    {
        static readonly JsonSerializer _serializer = null;
        static JsonSerializer _serializerUnderlyingName = null;

        static JsonExtension()
        {
            _serializer = new JsonSerializer();
            _serializer.Converters.Add(new JsonFieldConverter());
        }

        class Resolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(
                Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> list = base.CreateProperties(
                    type, memberSerialization);
                foreach (JsonProperty prop in list)
                {
                    prop.PropertyName = prop.UnderlyingName;
                }
                return list;
            }
        }

        public static string Serialize(object obj, bool underlyingName = false)
        {
            using var writer = new StringWriter();
            if (!underlyingName)
            {
                _serializer.Serialize(writer, obj);
            }
            else
            {
                if (_serializerUnderlyingName == null)
                {
                    _serializerUnderlyingName = JsonSerializer.CreateDefault(
                        new JsonSerializerSettings
                        {
                            ContractResolver = new Resolver()
                        }
                    );
                    _serializerUnderlyingName.Converters.Add(new JsonFieldConverter());
                }
                _serializerUnderlyingName.Serialize(writer, obj);
            }
            return writer.ToString();
        }

        public static T Deserialize<T>(string json)
        {
            using var reader = new JsonTextReader(new StringReader(json));
            return _serializer.Deserialize<T>(reader);
        }
    }

}