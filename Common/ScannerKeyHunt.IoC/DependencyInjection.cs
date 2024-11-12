using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Data.Enums;
using ScannerKeyHunt.Data.Helpers;
using ScannerKeyHunt.Domain.Interfaces;
using ScannerKeyHunt.Domain.Services;
using ScannerKeyHunt.Repository;
using ScannerKeyHunt.Repository.Interfaces;
using ScannerKeyHunt.Repository.Interfaces.Repository;
using ScannerKeyHunt.Repository.Repository;
using ScannerKeyHunt.RepositoryCache.Cache;
using ScannerKeyHunt.RepositoryCache.Cache.Factories;
using System.Reflection;
using System.Text;


namespace ScannerKeyHunt.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection CreateSeeds(this IServiceCollection services)
        {
            ISeedUserRoleInitial seedUserRoleInitial = services.BuildServiceProvider().GetService<ISeedUserRoleInitial>();

            GetSeedUserRoleInitial(seedUserRoleInitial);

            return services;
        }

        public static void GetSeedUserRoleInitial(ISeedUserRoleInitial seedUserRoleInitial)
        {
            seedUserRoleInitial.SeedRoles();

            seedUserRoleInitial.SeedUsers();
        }

        public static IUnitOfWork GetUnitOfWork()
        {
            return GetService<IUnitOfWork>();
        }

        public static TService GetService<TService>() where TService : class
        {
            return CreateService().BuildServiceProvider().GetService<TService>();
        }

        public static ServiceCollection CreateService()
        {
            ServiceCollection services = new ServiceCollection();

            ConfigureDBContext<BaseContext>(services);
            ConfigureIdentityServices(services);
            ConfigureAdditionalServices(services);

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            ConfigureDBContext<BaseContext>(services);

            ConfigureMongoDB(services);

            if (ConfigHelper.UseCache && ConfigHelper.CacheType == CacheType.Redis)
            {
                //var configurationOptions = new ConfigurationOptions
                //{
                //    EndPoints = { "oregon-redis.render.com:6379" }, // Adjust endpoint if necessary
                //    Password = "jskMjs8aYYxoZOndLABCXpymT8S0uz6l",  // Replace with your actual Redis password
                //    Ssl = true,  // Enable SSL
                //    AbortOnConnectFail = false  // Optional: prevent connection from being aborted on failure
                //};

                //var connection = ConnectionMultiplexer.Connect(configurationOptions);
                //rediss://red-clvng16d3nmc738etnug:jskMjs8aYYxoZOndLABCXpymT8S0uz6l@oregon-redis.render.com:6379
                //services.AddStackExchangeRedisCache(options =>
                //{
                //    options.Configuration = ConfigHelper.RedisConnectionString;
                //    options.InstanceName = "0";
                //    //options.ConfigurationOptions = new ConfigurationOptions()
                //    //{
                //    //    AbortOnConnectFail = true,
                //    //    //EndPoints = { ConfigHelper.RedisConnectionString }
                //    //};
                //});
                //options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                //{
                //    AbortOnConnectFail = true,
                //    EndPoints = { options.Configuration }
                //};
            }

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services, bool isApi = true)
        {
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = long.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
                options.MemoryBufferThreshold = int.MaxValue;
            });

            if (ConfigHelper.LettuceActive && ConfigHelper.IsProduction())
            {
                services.AddLettuceEncrypt();
                services.AddLettuceEncrypt(options =>
                {
                    options.DomainNames = ConfigHelper.LettuceEncryptDomains?.Split(",") ?? new string[] { };
                    options.AcceptTermsOfService = ConfigHelper.LettuceEncryptAcceptTermsOfService;
                    options.EmailAddress = ConfigHelper.LettuceEncryptEmail;
                });
            }

            ConfigureIdentityServices(services, isApi);
            ConfigureAdditionalServices(services);

            AddBackgroundServices(services);

            return services;
        }

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHostedService<WebhookProcessorService>();

            return services;
        }

        private static void ConfigureDBContext<TContext>(IServiceCollection services)
            where TContext : DbContext
        {
            string defaultConnection = string.Empty;

            ProvidersDB provider = GetDatabaseProvider();

            switch (provider)
            {
                case ProvidersDB.POSTGRES:
                    defaultConnection = ConfigHelper.PostgresConnection;

                    if (string.IsNullOrEmpty(defaultConnection))
                        break;

                    if (ConfigHelper.IsProduction())
                    {
                        string username = ConfigHelper.POSTGRESDBUsername;
                        string password = ConfigHelper.POSTGRESDBPassword;
                        string host = ConfigHelper.POSTGRESDBHost;
                        string port = ConfigHelper.POSTGRESDBPort;
                        string database = ConfigHelper.POSTGRESDBDatabase;

                        defaultConnection = string.Format(defaultConnection, host, port, database, username, password);
                    }

                    services.AddDbContext<TContext>(options =>
                        options.UseNpgsql(defaultConnection)
                    );
                    break;
                case ProvidersDB.SQLITE:
                    defaultConnection = ConfigHelper.SqliteConnection;

                    if (string.IsNullOrEmpty(defaultConnection))
                        break;

                    services.AddDbContext<TContext>(options =>
                        options.UseSqlite(defaultConnection));
                    break;
                case ProvidersDB.MEMORY:
                    services.AddDbContext<TContext>(options =>
                        options.UseInMemoryDatabase("ScannerKeyHunt"));
                    break;
                case ProvidersDB.DEFAULT:
                    defaultConnection = ConfigHelper.DefaultConnection;

                    if (string.IsNullOrEmpty(defaultConnection))
                        break;

                    if (ConfigHelper.IsProduction())
                    {
                        string username = ConfigHelper.MSSQLDBUsername;
                        string password = ConfigHelper.MSSQLDBPassword;
                        string host = ConfigHelper.MSSQLDBHost;
                        string port = ConfigHelper.MSSQLDBPort;
                        string database = ConfigHelper.MSSQLDBDatabase;

                        defaultConnection = string.Format(defaultConnection, host, port, database, username, password);
                    }

                    services.AddDbContext<TContext>(options =>
                        options.UseSqlServer(defaultConnection), ServiceLifetime.Scoped);
                    break;
                case ProvidersDB.LOCALHOST:
                    services.AddDbContext<TContext>(options =>
                        options.UseSqlServer(ConfigHelper.LocalConnection), ServiceLifetime.Scoped);
                    break;
                default:
                    break;
            }

            List<ProvidersDB> providersDBs = new List<ProvidersDB> { ProvidersDB.POSTGRES, ProvidersDB.DEFAULT, ProvidersDB.SQLITE };

            if (providersDBs.Contains(provider) && (string.IsNullOrEmpty(defaultConnection)))
            {
                services.AddDbContext<TContext>(options =>
                        options.UseInMemoryDatabase("ScannerKeyHunt"));
            }
        }

        private static void ConfigureMongoDB(IServiceCollection services)
        {
            string connectionString = ConfigHelper.MongoDBConnectionString;

            if (ConfigHelper.IsProduction())
            {
                string username = ConfigHelper.MongoDBUsername;
                string password = ConfigHelper.MongoDBPassword;
                string host = ConfigHelper.MongoDBHost;
                string port = ConfigHelper.MongoDBPort;

                connectionString = string.Format(connectionString, username, password, host, port);
            }

            services.AddScoped<IMongoClient, MongoClient>(sp =>
            {
                return new MongoClient(connectionString);
            });

            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(ConfigHelper.MongoDBDatabase);
            });
        }

        private static void ConfigureIdentityServices(IServiceCollection services, bool isApi = true)
        {
            if (isApi)
            {
                services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<BaseContext>()
                    .AddDefaultTokenProviders();
            }

            services.Configure<IdentityOptions>(
                opts => opts.SignIn.RequireConfirmedEmail = true
            );

            services.AddScoped<IUserIdentityRepository, UserIdentityRepository>();
            services.AddScoped<IUserStoreRepository, UserStoreRepository>();

            // Configuração do MemoryCache
            services.AddMemoryCache();

            // Registro dos serviços de cache
            //services.AddScoped<RedisCacheService>();
            services.AddScoped<MemoryCacheService>();
            services.AddScoped<MongoDBCacheService>();
            services.AddScoped<FirestoreCacheService>();

            // Registro da factory
            services.AddScoped<CacheServiceFactory>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void ConfigureAdditionalServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(GetIConfiguration());

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ScannerKeyHunt.Domain.Interfaces.IEmailSender, EmailSender>();
            services.AddScoped<ISeedUserRoleInitial, SeedUserRoleInitial>();

            services.AddLogging();
            //services.AddMemoryCache();
        }

        public static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static IConfiguration GetIConfiguration()
        {
            string _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_environment}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static ProvidersDB GetDatabaseProvider()
        {
            try
            {
                string provider = ConfigHelper.ProviderDBEnv;

                if (string.IsNullOrEmpty(provider))
                    throw new Exception("PROVIDER_DB_ENV Not Found");

                return Enum.Parse<ProvidersDB>(provider);
            }
            catch (Exception)
            {
                return Enum.Parse<ProvidersDB>(ConfigHelper.EnvironmentProvider);
            }
        }

        public static IServiceCollection DefineDB<TContext>(this IServiceCollection services)
        where TContext : DbContext
        {
            ConfigureDBContext<TContext>(services);
            services.AddMemoryCache();
            services.AddLogging();

            return services;
        }

        public static IServiceCollection CreateDatabase(this IServiceCollection services, WebApplication app)
        {
            ILogger<SeedUserRoleInitial> _logger = GetService<ILogger<SeedUserRoleInitial>>();

            try
            {
                using (var scope = app.Services.CreateAsyncScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<BaseContext>();
                    dbContext.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to creating database");
            }

            return services;
        }

        public static IServiceCollection DropDatabase(this IServiceCollection services, WebApplication application)
        {
            using (var scope = application.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<BaseContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.Migrate();
            }

            return services;
        }

        public static IServiceCollection AddApplications(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddWorker(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = ConfigHelper.TokenConfigurationAudience,
                    ValidIssuer = ConfigHelper.TokenConfigurationIssuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.JWTKey))
                };
            });

            return services;
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            IHealthChecksBuilder health = services.AddHealthChecks();

            services.AddHealthChecksUI().AddInMemoryStorage();

            string defaultConnection = string.Empty;

            ProvidersDB provider = GetDatabaseProvider();

            switch (provider)
            {
                case ProvidersDB.POSTGRES:
                    defaultConnection = ConfigHelper.PostgresConnection;

                    if (string.IsNullOrEmpty(defaultConnection))
                        break;

                    if (ConfigHelper.IsProduction())
                    {
                        string username = ConfigHelper.POSTGRESDBUsername;
                        string password = ConfigHelper.POSTGRESDBPassword;
                        string host = ConfigHelper.POSTGRESDBHost;
                        string port = ConfigHelper.POSTGRESDBPort;
                        string database = ConfigHelper.POSTGRESDBDatabase;

                        defaultConnection = string.Format(defaultConnection, host, port, database, username, password);
                    }

                    health.AddNpgSql(defaultConnection);
                    break;
                case ProvidersDB.SQLITE:
                    break;
                case ProvidersDB.MEMORY:
                    break;
                case ProvidersDB.DEFAULT:
                    defaultConnection = ConfigHelper.DefaultConnection;

                    if (string.IsNullOrEmpty(defaultConnection))
                        break;

                    if (ConfigHelper.IsProduction())
                    {
                        string username = ConfigHelper.MSSQLDBUsername;
                        string password = ConfigHelper.MSSQLDBPassword;
                        string host = ConfigHelper.MSSQLDBHost;
                        string port = ConfigHelper.MSSQLDBPort;
                        string database = ConfigHelper.MSSQLDBDatabase;

                        defaultConnection = string.Format(defaultConnection, host, port, database, username, password);
                    }

                    health.AddSqlServer(defaultConnection);
                    break;
                case ProvidersDB.LOCALHOST:
                    health.AddSqlServer(ConfigHelper.LocalConnection);
                    break;
            }

            return services;
        }
    }
}