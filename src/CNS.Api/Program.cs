using Microsoft.AspNetCore.Authentication;
using CNS.Application.Commands.SendMessage;
using CNS.Api.Authentication;
using CNS.Infrastructure;
using CNS.Infrastructure.Hosting;
using CNS.Infrastructure.Persistence;
using Microsoft.OpenApi;

var environmentName = AppEnvironmentNameResolver.Resolve();
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = environmentName
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CNS API",
        Version = "v1",
        Description = "API سرویس CNS",
        Contact = new OpenApiContact
        {
            Name = "پشتیبانی",
            Email = "support@cns.com"
        }
    });
});


builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<SendMessageCommandHandler>();

builder.Services.AddControllers();

//builder.Services
//    .AddAuthentication("Basic")
//    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", _ => { });

//builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CNS API V1");
        options.RoutePrefix = "swagger";
    });
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();

app.Run();
