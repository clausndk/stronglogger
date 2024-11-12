using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace clausndk.stronglogger.apptest;

internal class CustomSeqLogAppender : IStrongLoggerAppender, IDisposable
{
    private readonly ConcurrentQueue<QueueItem> _queue = new();
    private readonly HttpClient _client;
    private readonly Task _task;
    private readonly CancellationTokenSource _cancellationToken;
    private readonly Uri _url = new("http://localhost:5341/ingest/clef");
    private readonly MediaTypeHeaderValue _mediaTypeHeaderValue = new("application/vnd.serilog.clef");
    protected static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerOptions.Default);

    public CustomSeqLogAppender(HttpClient client)
    {
        _client = client;
        _cancellationToken = new CancellationTokenSource();
        _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        JsonSerializerOptions.Converters.Add(new ExceptionConverter());
        JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private async Task Run()
    {
        var messages = new List<QueueItem>();
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Flush(messages);

                await Task.Delay(TimeSpan.FromSeconds(12));
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception in SeqAppender: {e}");
            }
        }

        await Flush(messages);
    }

    private async Task Flush(List<QueueItem> messages)
    {
        messages.Clear();
        while (_queue.TryDequeue(out var queueItem))
        {
            messages.Add(queueItem);

            if (messages.Count >= 100)
            {
                await SendMessages(messages);
                messages.Clear();
            }
        }

        if (messages.Count > 0)
            await SendMessages(messages);
    }

    private async Task SendMessages(IReadOnlyList<QueueItem> queueItems)
    {
        var messages = queueItems
            .Select(q => q.ToJson())
            .ToList();

        var content = new StringContent(
            string.Join(Environment.NewLine, messages),
            Encoding.UTF8,
            _mediaTypeHeaderValue);
        using var response = await _client.PostAsync(_url, content, _cancellationToken.Token);
        var stringResponse = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(stringResponse);
    }

    public void Write(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception, string logMessage)
    {
        _queue.Enqueue(new QueueItem(timestamp, logLevel, exception, logMessage));
    }

    public void Dispose()
    {
        _cancellationToken.Cancel();
        _task.Wait();

        _client.Dispose();
        _task.Dispose();
    }

    private record QueueItem(DateTimeOffset Timestamp, LogLevel LogLevel, Exception? Exception, string LogMessage)
    {
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions);
        }

        [JsonPropertyName("@t")] public DateTimeOffset Timestamp { get; init; } = Timestamp;

        [JsonPropertyName("@l")] public LogLevel LogLevel { get; init; } = LogLevel;

        [JsonPropertyName("@x")] public string? Ex { get; init; } = Exception?.ToString();

        [JsonPropertyName("@m")] public string LogMessage { get; init; } = LogMessage;
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