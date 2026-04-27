using Microsoft.AspNetCore.Authentication;
using CNS.Application.Commands.SendMessage;
using CNS.Api.Authentication;
using CNS.Infrastructure;
using CNS.Infrastructure.Hosting;
using CNS.Infrastructure.Persistence;

var environmentName = AppEnvironmentNameResolver.Resolve();
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = environmentName
});

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<SendMessageCommandHandler>();

builder.Services.AddControllers();

builder.Services
    .AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", _ => { });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
