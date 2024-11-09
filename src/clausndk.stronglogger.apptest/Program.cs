// See https://aka.ms/new-console-template for more information

using clausndk.stronglogger;
using clausndk.stronglogger.apptest;
using clausndk.stronglogger.json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var httpClient = new HttpClient();
var jsonLogAppender = new JsonFileStrongLoggerAppender(@"D:\StrongLogs", "yyyy-MM-dd");
var xmlLogAppender = new XmlFileStrongLoggerAppender(@"D:\StrongLogs", "yyyy-MM-dd");
var customSeqLogAppender = new CustomSeqLogAppender(httpClient);
var strongLogger = new StrongLoggerBuilder()
    .AddConsoleAppender()
    .AddAppender(jsonLogAppender)
    .AddAppender(xmlLogAppender)
    .AddAppender(customSeqLogAppender)
    .Build();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(strongLogger);
builder.Services.AddHostedService<Main>();

var app = builder.Build();

await app.RunAsync();

customSeqLogAppender.Dispose();
httpClient.Dispose();