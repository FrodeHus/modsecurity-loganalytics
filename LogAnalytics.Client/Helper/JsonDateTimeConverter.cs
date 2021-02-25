using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogAnalytics.Client.Helper
{
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options) =>
          reader.GetString().FromModSecDateTime();

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(dateTimeValue.ToISO8601());
    }
}