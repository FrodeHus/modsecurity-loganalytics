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
          DateTime.ParseExact(reader.GetString(),
              "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(dateTimeValue.ToString(
                    "YYYY-MM-DDThh:mm:ssZ", CultureInfo.InvariantCulture));
    }
}