using CNS.Consumer;
using CNS.Infrastructure;
using CNS.Infrastructure.Hosting;
using CNS.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;

var environmentName = AppEnvironmentNameResolver.Resolve();
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    EnvironmentName = environmentName
});

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await DatabaseInitializer.InitializeAsync(host.Services);
host.Run();
