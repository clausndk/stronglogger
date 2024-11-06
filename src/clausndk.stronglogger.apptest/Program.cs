// See https://aka.ms/new-console-template for more information

using clausndk.stronglogger;
using clausndk.stronglogger.apptest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var httpClient = new HttpClient();
var customSeqLogAppender = new CustomSeqLogAppender(httpClient);
var strongLogger = new StrongLoggerBuilder()
    .AddConsoleAppender()
    .AddAppender(customSeqLogAppender)
    .Build();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(strongLogger);
builder.Services.AddHostedService<Main>();

var app = builder.Build();

await app.RunAsync();

customSeqLogAppender.Dispose();
httpClient.Dispose();