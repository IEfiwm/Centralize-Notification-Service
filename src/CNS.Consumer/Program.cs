using CNS.Consumer;
using CNS.Infrastructure;
using CNS.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await DatabaseInitializer.InitializeAsync(host.Services);
host.Run();
