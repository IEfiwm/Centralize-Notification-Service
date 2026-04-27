using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CNS.Application.Abstractions.Messaging;
using CNS.Application.Abstractions.Persistence;
using CNS.Application.Abstractions.Security;
using CNS.Infrastructure.Messaging;
using CNS.Infrastructure.Persistence;
using CNS.Infrastructure.Providers.Sms;
using CNS.Infrastructure.Security;

namespace CNS.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("Postgres"));
        });

        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
        services.AddScoped<IBasicAuthService, BasicAuthService>();

        services.Configure<RabbitMqOptions>(o => config.GetSection("RabbitMQ").Bind(o));
        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

        services.Configure<SmsOptions>(o => config.GetSection("Providers:Sms").Bind(o));
        services.AddSingleton<IMessageProvider, SampleSmsProvider>();

        services.Configure<TrezSmsOptions>(o => config.GetSection("Providers:Sms:Trez").Bind(o));
        services.AddHttpClient<TrezSmsProvider>((sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<TrezSmsOptions>>().Value;
            client.BaseAddress = new Uri(opt.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddSingleton<IMessageProvider, TrezSmsProvider>();

        services.AddSingleton<IMessageProviderResolver, MessageProviderResolver>();

        return services;
    }
}

