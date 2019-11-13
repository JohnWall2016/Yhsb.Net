using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yhsb.Json
{
    public abstract class Field<T>
    {
        public T Value { get; set; } = default;

        public abstract string Name { get; }

        public override string ToString() => Name;
    }

    public class FieldConverter<T, TField>
        : JsonConverter<TField> where TField : Field<T>, new()
    {
        public override void WriteJson(
            JsonWriter writer, TField value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override TField ReadJson(
            JsonReader reader, Type objectType, TField existingValue, 
            bool hasExistingValue, JsonSerializer serializer)
        {
            var prop = new TField()
            {
                Value = (T)reader.Value
            };
            return prop;
        }
    }
}