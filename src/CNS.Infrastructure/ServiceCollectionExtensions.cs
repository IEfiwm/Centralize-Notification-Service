using CNS.Application.Abstractions.Messaging;
using CNS.Application.Abstractions.Persistence;
using CNS.Application.Abstractions.Security;
using CNS.Infrastructure.Messaging;
using CNS.Infrastructure.Persistence;
using CNS.Infrastructure.Providers.Sms;
using CNS.Infrastructure.Providers.Sms.FaraPayamak;
using CNS.Infrastructure.Providers.Sms.GhasedMehr;
using CNS.Infrastructure.Providers.Sms.Trez;
using CNS.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CNS.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Postgres");
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(cs)
            .EnableDetailedErrors(true);
        });

        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
        services.AddScoped<IBasicAuthService, BasicAuthService>();

        services.Configure<RabbitMqOptions>(o => config.GetSection("RabbitMQ").Bind(o));
        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

        services.Configure<SmsOptions>(o => config.GetSection("Providers:Sms").Bind(o));
        services.AddSingleton<IMessageProvider, SampleSmsProvider>();

        services.Configure<FaraPayamakOptions>(o => config.GetSection("Providers:Sms:FaraPayamk").Bind(o));
        services.AddHttpClient<FaraPayamakProvider>((sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<FaraPayamakOptions>>().Value;
            client.BaseAddress = new Uri(opt.Url);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.Configure<GhasedmehrSmsOptions>(o => config.GetSection("Providers:Sms:Ghasedmehr").Bind(o));
        services.AddHttpClient<GhasedmehrSmsProvider>((sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<GhasedmehrSmsOptions>>().Value;
            client.BaseAddress = new Uri(opt.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.Configure<TrezSmsOptions>(o => config.GetSection("Providers:Sms:Trez").Bind(o));
        services.AddHttpClient<TrezSmsProvider>((sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<TrezSmsOptions>>().Value;
            client.BaseAddress = new Uri(opt.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddSingleton<IMessageProvider, GhasedmehrSmsProvider>();

        services.AddSingleton<IMessageProvider, TrezSmsProvider>();

        services.AddSingleton<IMessageProvider, FaraPayamakProvider>();

        services.AddSingleton<IMessageProviderResolver, MessageProviderResolver>();

        return services;
    }
}

