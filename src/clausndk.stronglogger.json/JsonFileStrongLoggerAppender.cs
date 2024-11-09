using System.Text;

namespace clausndk.stronglogger.json;

public class JsonFileStrongLoggerAppender(string outputPath, string filenameFormat) : IStrongLoggerAppender
{
    private readonly StringBuilder _sb = new();

    public void Write(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception, string logMessage)
    {
        var outputFile = Path.Join(outputPath, $"{timestamp.ToString(filenameFormat)}.json");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var output = CreateJsonLogEntry(timestamp, logLevel, exception, logMessage);
        using var fileStream = new StreamWriter(
            outputFile, 
            Encoding.UTF8,
            new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Append });
        fileStream.WriteLine(output);
        
    }

    private string CreateJsonLogEntry(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception, string logMessage)
    {
        _sb.Clear();
        _sb.Append("{\"@t\":\"");
        _sb.Append($"{timestamp:s}Z\",\"@l\":\"{logLevel}\",\"@m\":\"{logMessage}\"");
        LogException(_sb, exception);
        _sb.Append('}');

        return _sb.ToString();
    }

    private void LogException(StringBuilder sb, Exception? exception)
    {
        if (exception == null)
            return;

        sb.Append($",\"@x\":\"{exception}\",\"Stacktrace\":\"{exception.StackTrace}\"");
    }
}
