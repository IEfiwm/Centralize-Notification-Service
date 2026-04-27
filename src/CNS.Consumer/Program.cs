using NotificationCenter.Consumer;
using NotificationCenter.Infrastructure;
using NotificationCenter.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await DatabaseInitializer.InitializeAsync(host.Services);
host.Run();
