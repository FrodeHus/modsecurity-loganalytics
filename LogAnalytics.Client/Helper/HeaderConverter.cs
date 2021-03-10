using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogAnalytics.Client.Helper
{
    public class HeaderConverter : JsonConverter<Dictionary<string, string>>
    {
        public override Dictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, expected StartObject");
            }

            var dictionary = new Dictionary<string, string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }

                var propertyName = reader.GetString();
                if (string.IsNullOrEmpty(propertyName))
                {
                    throw new JsonException("Property name is missing");
                }

                reader.Read();
                dictionary.Add(propertyName, reader.GetString());
            }

            return dictionary;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var property in value.Keys)
            {
                writer.WriteStartObject(property.ToLowerInvariant());
                writer.WriteStringValue(value[property]);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}