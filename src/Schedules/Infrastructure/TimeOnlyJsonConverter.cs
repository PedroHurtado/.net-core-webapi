namespace Schedules.Infrastructure;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string FormatWithSeconds = "HH:mm:ss";
    private const string FormatWithoutSeconds = "HH:mm";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()!;
        return TimeOnly.TryParseExact(value, FormatWithoutSeconds, out var result)
            ? result
            : TimeOnly.ParseExact(value, FormatWithSeconds);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(FormatWithoutSeconds));
}