using BT.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Middleware;

/// <summary>
/// Adds a <c>CorrelationId</c> response header and optionally logs request bodies at Debug level.
/// </summary>
/// <remarks>
/// Request body logging is gated behind <c>LogLevel.Debug</c> — it is never active in
/// Production where the minimum level is Information. Do not enable Debug in Production.
/// <para>
/// <b>Why not <c>partial</c>?</b> There is no generated counterpart for this class.
/// The <c>partial</c> modifier was removed — it was misleading and served no purpose.
/// </para>
/// </remarks>
internal sealed class LoggingMiddleware(RequestDelegate next,ILogger<LoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Correlation ID is already set by CorrelationIdEnricher via Serilog.
        // We just echo it in the response header for client-side correlation.
        context.Response.Headers.TryAdd("X-Correlation-ID", context.TraceIdentifier);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            // EnableBuffering allows the body to be read multiple times.
            // Without it, reading here would consume the stream and the
            // downstream controller would receive an empty body.
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                leaveOpen: true);              // must leave open — downstream still needs it

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            // Reset position so the controller reads from the beginning
            context.Request.Body.Position = 0;

            HttpClientLogDefinitions.LogRequest(
                logger,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty,
                body);
        }

        await next(context).ConfigureAwait(false);
    }
}