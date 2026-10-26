using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.SendAndReceiveBytes.Definitions;
using Frends.HTTP.SendAndReceiveBytes.Helpers;

[assembly: InternalsVisibleTo("Frends.HTTP.SendAndReceiveBytes.Tests")]

namespace Frends.HTTP.SendAndReceiveBytes;

/// <summary>
/// Task class.
/// </summary>
public static class HTTP
{
    internal static readonly ObjectCache ClientCache = MemoryCache.Default;
    private static readonly CacheItemPolicy CachePolicy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) };

    internal static IHttpClientFactory ClientFactory { get; set; } = new HttpClientFactory();

    /// <summary>
    /// HTTP request for sending and receiving content in byte array.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.HTTP.SendAndReceiveBytes)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, byte[] BodyBytes, double BodySizeInMegaBytes, string ContentType, Dictionary(string, string) Headers, int StatusCode, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> SendAndReceiveBytes([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(input.Url)) throw new ArgumentNullException("Url can not be empty.");

            var httpClient = GetHttpClientForOptions(options);
            var headers = GetHeaderDictionary(input.Headers, options);

            using var content = new ByteArrayContent(input.ContentBytes);
            using var responseMessage = await GetHttpRequestResponseAsync(
                    httpClient,
                    input.Method.ToString(),
                    input.Url,
                    content,
                    headers,
                    options,
                    cancellationToken)
                .ConfigureAwait(false);

            var response = new Result
            {
                Success = true,
                BodyBytes = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false),
                ContentType = responseMessage.Content.Headers.ContentType?.ToString(),
                Headers = GetResponseHeaderDictionary(responseMessage.Headers, responseMessage.Content.Headers),
                StatusCode = (int)responseMessage.StatusCode,
                Error = null,
            };

            if (!responseMessage.IsSuccessStatusCode && options.ThrowExceptionOnErrorResponse)
                throw new WebException($"Request to '{input.Url}' failed with status code {(int)responseMessage.StatusCode}.");

            return response;
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    internal static void ClearClientCache()
    {
        var cacheKeys = ClientCache.Select(kvp => kvp.Key).ToList();
        foreach (var cacheKey in cacheKeys)
        {
            ClientCache.Remove(cacheKey);
        }
    }

    // Combine response- and responsecontent header to one dictionary
    private static Dictionary<string, string> GetResponseHeaderDictionary(HttpResponseHeaders responseMessageHeaders, HttpContentHeaders contentHeaders)
    {
        var responseHeaders = responseMessageHeaders.ToDictionary(h => h.Key, h => string.Join(";", h.Value));
        var allHeaders = contentHeaders?.ToDictionary(h => h.Key, h => string.Join(";", h.Value)) ?? new Dictionary<string, string>();
        responseHeaders.ToList().ForEach(x => allHeaders[x.Key] = x.Value);
        return allHeaders;
    }

    private static IDictionary<string, string> GetHeaderDictionary(Header[] headers, Options options)
    {
        if (!headers.Any(header => header.Name.ToLower().Equals("authorization")))
        {
            var authHeader = new Header { Name = "Authorization" };
            switch (options.Authentication)
            {
                case Authentication.Basic:
                    authHeader.Value = $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"))}";
                    headers = headers.Concat(new[] { authHeader }).ToArray();
                    break;
                case Authentication.OAuth:
                    authHeader.Value = $"Bearer {options.Token}";
                    headers = headers.Concat(new[] { authHeader }).ToArray();
                    break;
            }
        }

        // Ignore case for headers and key comparison
        return headers.ToDictionary(key => key.Name, value => value.Value, StringComparer.InvariantCultureIgnoreCase);
    }

    private static HttpClient GetHttpClientForOptions(Options options)
    {
        var cacheKey = GetHttpClientCacheKey(options);

        if (ClientCache.Get(cacheKey) is HttpClient httpClient)
            return httpClient;

        httpClient = ClientFactory.CreateClient(options);
        httpClient.SetDefaultRequestHeadersBasedOnOptions(options);

        ClientCache.Add(cacheKey, httpClient, CachePolicy);

        return httpClient;
    }

    private static string GetHttpClientCacheKey(Options options)
    {
        // Includes everything except for options.Token, which is used on request level, not http client level
        var raw = $"{options.Authentication}:{options.Username}:{options.Password}:{options.ClientCertificateSource}"
                  + $":{options.ClientCertificateFilePath}:{options.ClientCertificateInBase64}:{options.ClientCertificateKeyPhrase}"
                  + $":{options.CertificateThumbprint}:{options.LoadEntireChainForCertificate}:{options.ConnectionTimeoutSeconds}"
                  + $":{options.FollowRedirects}:{options.AllowInvalidCertificate}:{options.AllowInvalidResponseContentTypeCharSet}"
                  + $":{options.ThrowExceptionOnErrorResponse}:{options.AutomaticCookieHandling}"
                  + $":{options.SslProtocolVersion}:{options.HttpProtocolVersion}"
                  + $":{options.UseProxy}:{options.ProxyUrl}:{options.ProxyUsername}:{options.ProxyPassword}";

        var salt = Encoding.UTF8.GetBytes("Frends.HTTP.SendAndReceiveBytes.HttpClientCacheKey");
        var key = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(raw),
            salt,
            10000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);
        return Convert.ToBase64String(key);
    }

    private static async Task<HttpResponseMessage> GetHttpRequestResponseAsync(
            HttpClient httpClient,
            string method,
            string url,
            HttpContent content,
            IDictionary<string, string> headers,
            Options options,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Only POST, PUT, PATCH and DELETE can have content, otherwise the HttpClient will fail
        var isContentAllowed = Enum.TryParse(method, ignoreCase: true, result: out SendMethod _);

        using var request = new HttpRequestMessage(new HttpMethod(method), new Uri(url))
        {
            Content = isContentAllowed ? content : null,
        };

        // Clear default headers
        content.Headers.Clear();
        foreach (var header in headers)
        {
            var requestHeaderAddedSuccessfully = request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            if (!requestHeaderAddedSuccessfully && request.Content != null)
            {
                // Could not add to request headers try to add to content headers
                // this check is probably not needed anymore as the new HttpClient does not seem fail on malformed headers
                var contentHeaderAddedSuccessfully = content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                if (!contentHeaderAddedSuccessfully)
                    Trace.TraceWarning($"Could not add header {header.Key}:{header.Value}");
            }
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException canceledException)
        {
            if (cancellationToken.IsCancellationRequested)

                // Cancellation is from outside -> Just throw
                throw;

            // Cancellation is from inside of the request, mostly likely a timeout
            throw new Exception("HttpRequest was canceled, most likely due to a timeout.", canceledException);
        }

        // this check is probably not needed anymore as the new HttpClient does not fail on invalid charsets
        if (options.AllowInvalidResponseContentTypeCharSet && response.Content.Headers?.ContentType != null)
            response.Content.Headers.ContentType.CharSet = null;

        return response;
    }
}
