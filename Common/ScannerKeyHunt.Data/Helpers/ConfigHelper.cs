using Microsoft.Extensions.Configuration;

namespace ScannerKeyHunt.Data.Helpers
{
    public static class ConfigHelper
    {
        private static readonly IConfiguration _configuration;
        private static readonly string _environment;

        static ConfigHelper()
        {
            _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_environment}.json", optional: true, reloadOnChange: true)
                .Build();
        }

        public static bool UseCache => GetBoolValue("AppSettings:UseCache", false);

        public static long CacheDurationMinutes => GetLongValue("AppSettings:CacheDurationMinutes", 30);

        public static string MongoDBConnectionString => GetStringValue("MongoDBSettings:ConnectionString");

        public static string MongoDBDatabaseName => GetStringValue("MongoDBSettings:DatabaseName");

        public static string RedisConnectionString => GetStringValue("RedisCacheSettings:ConnectionString");

        public static string RedisInstanceName => GetStringValue("RedisCacheSettings:InstanceName");

        public static Enums.CacheType CacheType => GetEnumValue<Enums.CacheType>("AppSettings:CacheType", Enums.CacheType.MemoryCache);

        public static double TokenConfigurationExpireHours => GetDoubleValue("TokenConfiguration:ExpireHours", 1);

        public static string TokenConfigurationIssuer => GetStringValue("TokenConfiguration:Issuer");

        public static string TokenConfigurationAudience => GetStringValue("TokenConfiguration:Audience");

        public static string JWTKey => GetStringValue("JWT:Key");

        public static string GoogleFirestoreCredentialsPath => GetStringValue("GoogleFirestore:CredentialsPath");

        public static string GoogleFirestoreProjectId => GetStringValue("GoogleFirestore:ProjectId");

        public static string EnvironmentProvider => GetStringValue("Enviroment:Provider");

        public static string ZapiURL => GetStringValue("ZapiAPI:ZapiURL");

        public static string PostgresConnection => GetConnectionString("PostgresConnection");

        public static string SqliteConnection => GetConnectionString("SqliteConnection");

        public static string DefaultConnection => GetConnectionString("DefaultConnection");

        public static string LocalConnection => GetConnectionString("LocalhostConnection");

        public static string DefaultConfigsPathFileDefault => GetStringValue("DefaultConfigs:PathFileDefault");

        public static string MongoDBHost => Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_HOST");

        public static string MongoDBPort => Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PORT");

        public static string MongoDBUsername => Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_USERNAME");

        public static string MongoDBPassword => Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD");

        public static string MongoDBDatabase => Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_DATABASE") ?? GetStringValue("MongoDBSettings:DatabaseName");

        public static string MSSQLDBHost => Environment.GetEnvironmentVariable("MSSQL_HOST");

        public static string MSSQLDBPort => Environment.GetEnvironmentVariable("MSSQL_PORT");

        public static string MSSQLDBDatabase => Environment.GetEnvironmentVariable("MSSQL_DATABASE");

        public static string MSSQLDBUsername => Environment.GetEnvironmentVariable("MSSQL_USERNAME");

        public static string MSSQLDBPassword => Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");

        public static string POSTGRESDBHost => Environment.GetEnvironmentVariable("POSTGRES_HOST");

        public static string POSTGRESDBPort => Environment.GetEnvironmentVariable("POSTGRES_PORT");

        public static string POSTGRESDBDatabase => Environment.GetEnvironmentVariable("POSTGRES_DATABASE");

        public static string POSTGRESDBUsername => Environment.GetEnvironmentVariable("POSTGRES_USERNAME");

        public static string POSTGRESDBPassword => Environment.GetEnvironmentVariable("POSTGRES_SA_PASSWORD");

        public static string ProviderDBEnv => Environment.GetEnvironmentVariable("PROVIDER_DB_ENV");

        public static string MinioEndpoint => Environment.GetEnvironmentVariable("MINIO_ENDPOINT") ?? GetStringValue("Minio:Endpoint");

        public static string MinioAccessKey => Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY") ?? GetStringValue("Minio:AccessKey");

        public static string MinioSecretKey => Environment.GetEnvironmentVariable("MINIO_SECRET_KEY") ?? GetStringValue("Minio:SecretKey");

        public static bool MinioIsSSL => Convert.ToBoolean(Environment.GetEnvironmentVariable("MINIO_IS_SSL") ?? GetStringValue("Minio:IsSSL"));

        public static bool LettuceActive
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(Environment.GetEnvironmentVariable("LETSENCRYPT_ACTIVE") ?? GetStringValue("LetsEncrypt:IsActive"));
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool LettuceEncryptAcceptTermsOfService => Convert.ToBoolean(Environment.GetEnvironmentVariable("LETSENCRYPT_ACCEPT_TERMS_OF_SERVICE") ?? GetStringValue("LetsEncrypt:AcceptTermsOfService"));

        public static string LettuceEncryptEmail => Environment.GetEnvironmentVariable("LETSENCRYPT_EMAIL") ?? GetStringValue("LetsEncrypt:EmailAddress");

        public static string LettuceEncryptDomains => Environment.GetEnvironmentVariable("LETSENCRYPT_DOMAINS") ?? GetStringValue("LetsEncrypt:Domains");

        public static string LettuceEncryptChallengePath => Environment.GetEnvironmentVariable("LETSENCRYPT_CHALLENGE_PATH") ?? GetStringValue("LetsEncrypt:ChallengePath");

        public static string LettuceEncryptStaging => Environment.GetEnvironmentVariable("LETSENCRYPT_STAGING") ?? GetStringValue("LetsEncrypt:Staging");

        public static string LettuceEncryptUseStaging => Environment.GetEnvironmentVariable("LETSENCRYPT_USE_STAGING") ?? GetStringValue("LetsEncrypt:UseStaging");

        public static string CredencialsUserUserAdmin => GetStringValue("CredencialsUser:UserAdmin");

        public static string CredencialsUserUserMember => GetStringValue("CredencialsUser:UserMember");

        public static string CredencialsUserUserUser => GetStringValue("CredencialsUser:UserUser");

        public static string CredencialsUserUserService => GetStringValue("CredencialsUser:UserService");

        private static string GetStringValue(string key, string defaultValue = "")
        {
            try
            {
                return _configuration[key] ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static bool GetBoolValue(string key, bool defaultValue = false)
        {
            try
            {
                return bool.Parse(_configuration[key] ?? defaultValue.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }

        private static long GetLongValue(string key, long defaultValue = 0)
        {
            try
            {
                return long.Parse(_configuration[key] ?? defaultValue.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }

        private static double GetDoubleValue(string key, double defaultValue = 0)
        {
            try
            {
                return double.Parse(_configuration[key] ?? defaultValue.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }

        private static TEnum GetEnumValue<TEnum>(string key, TEnum defaultValue) where TEnum : struct
        {
            try
            {
                return Enum.TryParse(_configuration[key], out TEnum value) ? value : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static string GetConnectionString(string name)
        {
            try
            {
                return _configuration.GetConnectionString(name) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsDevelopment() => _environment == "Development";
        public static bool IsProduction() => _environment == "Production";
    }
}
