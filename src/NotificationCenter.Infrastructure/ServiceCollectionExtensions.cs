using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationCenter.Application.Abstractions.Messaging;
using NotificationCenter.Application.Abstractions.Persistence;
using NotificationCenter.Application.Abstractions.Security;
using NotificationCenter.Infrastructure.Messaging;
using NotificationCenter.Infrastructure.Persistence;
using NotificationCenter.Infrastructure.Providers.Sms;
using NotificationCenter.Infrastructure.Security;

namespace NotificationCenter.Infrastructure;

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

