using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Caching;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.Messaging;
using GlobalStable.Infrastructure.Persistence;
using GlobalStable.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refit;

namespace GlobalStable.Infrastructure.DependencyInjection
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPersistence(configuration);
            services.AddCache(configuration);
            services.AddMessaging(configuration);
            services.AddClients(configuration);

            return services;
        }

        public static void ApplyDatabaseMigrations(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying migrations: {ex.Message}");
                throw;
            }
        }

        private static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("Postgres")
                                       ?? throw new NullReferenceException(nameof(connectionString));

            services.AddDbContext<ServiceDbContext>(options =>
                options.UseNpgsql(connectionString, e =>
                    e.MigrationsAssembly(typeof(ServiceDbContext).Assembly.FullName)));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
            services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
            services.AddScoped<IWithdrawalOrderRepository, WithdrawalOrderRepository>();
            services.AddScoped<IDepositOrderRepository, DepositOrderRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();

            services.AddMemoryCache();

            return services;
        }

        private static IServiceCollection AddCache(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var redisConnection = configuration["Redis:ConnectionString"];
            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                Console.WriteLine("⚠ Redis connection string is not set. Cache will not be used.");
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "BGX_Cache_";
                });
                services.AddSingleton<RedisCacheService>();
            }

            return services;
        }

        private static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var rabbitConfig = configuration.GetSection("RabbitMQ");
            var host = rabbitConfig["Host"];
            var username = rabbitConfig["Username"];
            var password = rabbitConfig["Password"];

            if (string.IsNullOrWhiteSpace(host)
                || string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("RabbitMQ configuration is missing required fields.");
            }

            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMqConnection>>();
                return new RabbitMqConnection(host, username, password, logger);
            });

            services.AddSingleton<IEventPublisher>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMqPublisher>>();
                var connection = sp.GetRequiredService<RabbitMqConnection>();
                return new RabbitMqPublisher(logger, connection);
            });

            services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();
            services.AddScoped<ITransactionEventPublisher, TransactionEventPublisher>();
            services.AddScoped<INotificationPublisher, NotificationPublisher>();
            services.AddScoped<IAuditEventPublisher, AuditEventPublisher>();

            return services;
        }

        private static IServiceCollection AddClients(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var transactionService = configuration["TransactionService:BaseAddress"]
                                     ?? throw new InvalidOperationException("Missing configuration: TransactionService:BaseAddress");

            var bgpConnectorClient = configuration["BgpConnector:BaseAddress"]
                                     ?? throw new InvalidOperationException("Missing configuration: BgpConnector:BaseAddress");

            var customerService = configuration["CustomerService:BaseAddress"]
                                     ?? throw new InvalidOperationException("Missing configuration: CustomerService:BaseAddress");

            if (!Uri.TryCreate(transactionService, UriKind.Absolute, out var transactionServiceBaseAddress))
            {
                throw new InvalidOperationException($"Invalid URI format for TransactionService: {transactionService}");
            }

            if (!Uri.TryCreate(bgpConnectorClient, UriKind.Absolute, out var bgpConnectorBaseAddress))
            {
                throw new InvalidOperationException($"Invalid URI format for BgpConnector: {bgpConnectorClient}");
            }

            if (!Uri.TryCreate(customerService, UriKind.Absolute, out var customerServiceBaseAddress))
            {
                throw new InvalidOperationException($"Invalid URI format for CustomerService: {customerService}");
            }

            services
                .AddRefitClient<ITransactionServiceClient>()
                .ConfigureHttpClient(c => c.BaseAddress = transactionServiceBaseAddress);

            services
                .AddRefitClient<IBgpConnectorClient>()
                .ConfigureHttpClient(c => c.BaseAddress = bgpConnectorBaseAddress);

            services
                    .AddRefitClient<ICustomerServiceClient>()
                    .ConfigureHttpClient(c => c.BaseAddress = customerServiceBaseAddress);

            return services;
        }
    }
}
