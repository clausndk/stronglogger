using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace clausndk.stronglogger.json;

public class JsonFileStrongLoggerAppender : IStrongLoggerAppender
{
    private readonly StringBuilder _sb = new();
    private readonly string _outputPath;
    private readonly string _filenameFormat;
    protected static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerOptions.Default);

    public JsonFileStrongLoggerAppender(string outputPath, string filenameFormat)
    {
        _outputPath = outputPath;
        _filenameFormat = filenameFormat;
        JsonSerializerOptions.Converters.Add(new ExceptionConverter());
    }

    public void Write(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception, string logMessage)
    {
        var outputFile = Path.Join(_outputPath, $"{timestamp.ToString(_filenameFormat)}.json");
        if (!Directory.Exists(_outputPath))
            Directory.CreateDirectory(_outputPath);
        
        using var fileStream = new StreamWriter(
            outputFile,
            Encoding.UTF8,
            new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Append });
        CreateJsonLogEntry(fileStream, timestamp, logLevel, exception, logMessage);
        fileStream.WriteLine();
    }

    private void CreateJsonLogEntry(StreamWriter fileStream, DateTimeOffset timestamp, LogLevel logLevel,
        Exception? exception,
        string logMessage)
    {
        new JsonLogEntry(timestamp, logLevel, exception, logMessage)
            .ToJson(fileStream);
    }

    private record JsonLogEntry(DateTimeOffset Timestamp, LogLevel LogLevel, Exception? Exception, string LogMessage)
    {
        public void ToJson(StreamWriter fileStream)
        {
            JsonSerializer.Serialize(fileStream.BaseStream, this, JsonSerializerOptions);
        }

        [JsonPropertyName("@t")]
        public DateTimeOffset Timestamp { get; init; } = Timestamp;

        [JsonPropertyName("@l")]
        public LogLevel LogLevel { get; init; } = LogLevel;

        [JsonPropertyName("@x")]
        public Exception? Exception { get; init; } = Exception;

        [JsonPropertyName("@m")]
        public string LogMessage { get; init; } = LogMessage;
    }

    private class ExceptionConverter : JsonConverter<Exception>
    {
        public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            var exceptionType = value.GetType();
            writer.WriteString("ClassName", exceptionType.FullName);
            var properties = exceptionType.GetProperties()
                .Where(e => e.PropertyType != typeof(Type))
                .Where(e => e.PropertyType.Namespace != typeof(MemberInfo).Namespace)
                .ToList();
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(value, null);
                if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull && propertyValue == null)
                    continue;
                writer.WritePropertyName(property.Name);
                JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
            }

            writer.WriteEndObject();
        }
    }
}