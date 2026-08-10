using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Features.ControlPlane.Tenants.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace BT.Infrastructure.Features.ControlPlane.Provisioning;

public class GitHubActionsStampProvisioner(HttpClient httpClient, IConfiguration configuration, ILogger<GitHubActionsStampProvisioner> logger) : IStampProvisioner
{
    public async Task ProvisionIsolatedStampAsync(string tenantId, string stampId, string resourceGroup, string databaseProvider, CancellationToken cancellationToken = default)
    {
        var githubToken = configuration["ControlPlane:GitHub:Pat"];
        var repoOwner = configuration["ControlPlane:GitHub:RepoOwner"];
        var repoName = configuration["ControlPlane:GitHub:RepoName"];

        if (string.IsNullOrEmpty(githubToken) || string.IsNullOrEmpty(repoOwner) || string.IsNullOrEmpty(repoName))
        {
            logger.LogWarning("GitHub Action credentials are not configured. Provisioning skipped.");
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.github.com/repos/{repoOwner}/{repoName}/actions/workflows/provision-isolated-stamp.yml/dispatches");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("BaseTemplate", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

        var payload = new
        {
            @ref = "main",
            inputs = new
            {
                stamp_id = stampId,
                resource_group = resourceGroup,
                database_provider = databaseProvider
            }
        };

        request.Content = JsonContent.Create(payload);

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            logger.LogError(
                "Failed to trigger GitHub Actions workflow for stamp provisioning. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, error);
            throw new InvalidOperationException(
                $"Stamp provisioning workflow dispatch failed with HTTP {(int)response.StatusCode}. " +
                "Check GitHub Actions credentials and workflow configuration.");
        }

        logger.LogInformation("Successfully triggered GitHub Actions workflow for stamp {StampId}", stampId);
    }
}
