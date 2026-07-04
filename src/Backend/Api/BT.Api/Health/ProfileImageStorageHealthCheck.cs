using Azure.Identity;
using Azure.Storage.Blobs;

using BT.Infrastructure.Configuration;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace BT.Api.Health;

internal sealed class ProfileImageStorageHealthCheck(
    IWebHostEnvironment environment,
    IOptions<ProfileImageStorageSettings> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var provider = settings.Provider?.Trim() ?? "Local";

        if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            return CheckLocalStorage(settings);
        }

        if (provider.Equals("Azurite", StringComparison.OrdinalIgnoreCase))
        {
            return await CheckBlobStorageAsync(settings.Azurite, "Azurite profile image storage", cancellationToken)
                .ConfigureAwait(false);
        }

        if (provider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            return await CheckBlobStorageAsync(settings.AzureBlob, "Azure Blob profile image storage", cancellationToken)
                .ConfigureAwait(false);
        }

        return HealthCheckResult.Unhealthy($"ProfileImageStorage:Provider '{settings.Provider}' is not supported.");
    }

    private HealthCheckResult CheckLocalStorage(ProfileImageStorageSettings settings)
    {
        try
        {
            var rootPath = Path.IsPathRooted(settings.LocalRootPath)
                ? settings.LocalRootPath
                : Path.Combine(environment.ContentRootPath, settings.LocalRootPath);

            Directory.CreateDirectory(rootPath);
            return HealthCheckResult.Healthy("Local profile image storage is writable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Local profile image storage probe failed.", ex);
        }
    }

    private static async Task<HealthCheckResult> CheckBlobStorageAsync(
        AzureBlobProfileImageStorageSettings settings,
        string description,
        CancellationToken cancellationToken)
    {
        try
        {
            var containerClient = CreateContainerClient(settings);
            var exists = await containerClient.ExistsAsync(cancellationToken).ConfigureAwait(false);

            return exists.Value
                ? HealthCheckResult.Healthy($"{description} container is reachable.")
                : HealthCheckResult.Unhealthy($"{description} container does not exist or cannot be accessed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{description} probe failed.", ex);
        }
    }

    private static BlobContainerClient CreateContainerClient(AzureBlobProfileImageStorageSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.ContainerUri))
        {
            return new BlobContainerClient(new Uri(settings.ContainerUri), new DefaultAzureCredential());
        }

        if (!string.IsNullOrWhiteSpace(settings.ConnectionString) &&
            !string.IsNullOrWhiteSpace(settings.ContainerName))
        {
            return new BlobContainerClient(settings.ConnectionString, settings.ContainerName);
        }

        throw new InvalidOperationException("Blob profile image storage requires ContainerUri or ConnectionString plus ContainerName.");
    }
}
