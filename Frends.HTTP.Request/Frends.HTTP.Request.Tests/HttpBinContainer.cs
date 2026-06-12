using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Frends.HTTP.Request.Tests;

internal static class HttpBinContainer
{
    private static readonly SemaphoreSlim StartStopLock = new(1, 1);
    private static IContainer container;
    private static string baseUrl;

    public static string BaseUrl => baseUrl ?? throw new InvalidOperationException("HTTP test container has not been started.");

    public static async Task StartAsync()
    {
        if (container != null)
            return;

        await StartStopLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (container != null)
                return;

            container = new ContainerBuilder("kennethreitz/httpbin:latest")
                .WithPortBinding(80, true)
                .WithCleanUp(true)
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            var mappedPort = container.GetMappedPublicPort(80);
            baseUrl = $"http://localhost:{mappedPort}";

            await WaitUntilReadyAsync(baseUrl).ConfigureAwait(false);
        }
        finally
        {
            StartStopLock.Release();
        }
    }

    public static async Task StopAsync()
    {
        await StartStopLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (container == null)
                return;

            await container.DisposeAsync().ConfigureAwait(false);
            container = null;
            baseUrl = null;
        }
        finally
        {
            StartStopLock.Release();
        }
    }

    private static async Task WaitUntilReadyAsync(string baseUrl)
    {
        using var client = new HttpClient();

        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                var response = await client.GetAsync($"{baseUrl}/status/200").ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // Container may still be starting up.
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        }

        throw new InvalidOperationException("The HTTP test container did not become ready in time.");
    }
}


