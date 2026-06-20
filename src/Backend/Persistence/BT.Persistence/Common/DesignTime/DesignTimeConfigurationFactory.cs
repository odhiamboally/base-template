using Microsoft.Extensions.Configuration;

namespace BT.Persistence.Common.DesignTime;

internal static class DesignTimeConfigurationFactory
{
    private const string ApiUserSecretsId = "09cd72b4-b751-42c7-93cf-3a068da7958e";

    public static IConfigurationRoot Create()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(@"src\Backend\Api\BT.Api\appsettings.json", optional: true)
            .AddJsonFile(@"src\Backend\Api\BT.Api\appsettings.Development.json", optional: true)
            .AddJsonFile(GetUserSecretsPath(), optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static string GetConnectionString(IConfiguration configuration, string contextConnectionName)
    {
        var connectionString = configuration.GetConnectionString(contextConnectionName)
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.StartsWith("SET_VIA_", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("REPLACE_VIA_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"A valid connection string was not found for '{contextConnectionName}' or 'DefaultConnection'. " +
                "Set it via user-secrets or environment variables before running EF Core tooling.");
        }

        return connectionString;
    }

    public static void ConfigureSqlServer(Microsoft.EntityFrameworkCore.Infrastructure.SqlServerDbContextOptionsBuilder sqlOptions)
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    }

    private static string GetUserSecretsPath()
    {
        var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        return Path.Combine(applicationData, "Microsoft", "UserSecrets", ApiUserSecretsId, "secrets.json");
    }
}
