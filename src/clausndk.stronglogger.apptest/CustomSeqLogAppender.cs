using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace clausndk.stronglogger.apptest;

internal class CustomSeqLogAppender : IStrongLoggerAppender, IDisposable
{
    private readonly ConcurrentQueue<QueueItem> _queue = new();
    private readonly HttpClient _client;
    private readonly Task _task;
    private readonly CancellationTokenSource _cancellationToken;

    public CustomSeqLogAppender(HttpClient client)
    {
        _client = client;
        _cancellationToken = new CancellationTokenSource();
        _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
    }

    private async Task Run()
    {
        var messages = new List<QueueItem>();
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                messages.Clear();
                while (_queue.TryDequeue(out var queueItem))
                    messages.Add(queueItem);

                if (messages.Count > 0)
                    await SendMessages(messages);

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            catch (Exception)
            {
            }
        }
    }

    private async Task SendMessages(IReadOnlyList<QueueItem> queueItems)
    {
        var messages = queueItems
            .Select(q => q.ToJson())
            .ToList();

        var content = new StringContent(
            string.Join(Environment.NewLine, messages),
            Encoding.UTF8,
            new MediaTypeHeaderValue("application/vnd.serilog.clef"));
        var url = "http://localhost:5341/ingest/clef";
        using var response = await _client.PostAsync(url, content);
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
            var sb = new StringBuilder("{\"@t\":\"");
            sb.Append($"{Timestamp:s}Z\",\"@l\":\"{LogLevel}\",\"@m\":\"{LogMessage}\"");
            //LogException(sb, Exception);
            sb.Append('}');

            return sb.ToString();
        }
    }
}