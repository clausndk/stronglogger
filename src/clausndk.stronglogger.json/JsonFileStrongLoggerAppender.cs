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

        var output = CreateJsonLogEntry(timestamp, logLevel, exception, logMessage);
        using var fileStream = new StreamWriter(
            outputFile,
            Encoding.UTF8,
            new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Append });
        fileStream.WriteLine(output);
    }

    private StringBuilder CreateJsonLogEntry(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception,
        string logMessage)
    {
        _sb.Clear();
        _sb.Append("{\"@t\":\"");
        _sb.Append($"{timestamp:s}Z\",\"@l\":\"{logLevel}\",\"@m\":\"{logMessage}\"");
        LogException(_sb, exception);
        _sb.Append('}');

        return _sb;
    }

    private void LogException(StringBuilder sb, Exception? exception)
    {
        if (exception == null)
            return;

        var ex = System.Text.Json.JsonSerializer.Serialize(exception, JsonSerializerOptions);
        sb.Append($",\"@x\":\"{ex}\",\"Stacktrace\":\"{exception.StackTrace}\"");
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