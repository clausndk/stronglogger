using System.Xml.Linq;

namespace clausndk.stronglogger.json;

public class XmlFileStrongLoggerAppender(string outputPath, string filenameFormat) : IStrongLoggerAppender
{
    public void Write(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception, string logMessage)
    {
        var outputFile = Path.Join(outputPath, $"{timestamp.ToString(filenameFormat)}.xml");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        XDocument doc;
        if (!File.Exists(outputFile))
            doc = new XDocument(new XElement("log"));
        else
            doc = XDocument.Load(outputFile, LoadOptions.PreserveWhitespace);

        var entry = CreateXmlLogEntry(timestamp, logLevel, exception, logMessage);
        doc.Root!.Add(entry);
        doc.Save(outputFile);
    }

    private XElement CreateXmlLogEntry(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception,
        string logMessage)
    {
        var elem = new XElement("entry",
            new XElement("timestamp", timestamp),
            new XElement("level", logLevel),
            new XElement("message", logMessage),
            new XElement("exception", exception),
            new XElement("stacktrace", exception?.StackTrace));

        return elem;
    }
}