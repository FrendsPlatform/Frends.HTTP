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
using Frends.HTTP.RequestBytes.Definitions;
using Frends.HTTP.RequestBytes.Helpers;

[assembly: InternalsVisibleTo("Frends.HTTP.RequestBytes.Tests")]

namespace Frends.HTTP.RequestBytes;

/// <summary>
/// Task class.
/// </summary>
public static class HTTP
{
    private static readonly CacheItemPolicy CachePolicy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) };

    internal static IHttpClientFactory ClientFactory { get; set; } = new HttpClientFactory();

    internal static ObjectCache ClientCache { get; } = MemoryCache.Default;

    /// <summary>
    /// HTTP request with byte return type
    /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.HTTP.RequestBytes)
    /// </summary>
    /// <param name="input">Input parameters of the request.</param>
    /// <param name="options">Options for the request.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>Object { bool Success, byte[] Body, double BodySizeInMegaBytes, string ContentType, Dictionary(string, string) Headers, int StatusCode, Object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> RequestBytes([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        try
        {
            return await ExecuteRequest(input, options, cancellationToken).ConfigureAwait(false);
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

    private static async Task<Result> ExecuteRequest(Input input, Options options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(input.Url)) throw new ArgumentNullException("Url can not be empty.");

        var httpClient = GetHttpClientForOptions(options);
        var headers = GetHeaderDictionary(input.Headers, options);

        using (var content = GetContent(input, headers))
        {
            using (var responseMessage = await GetHttpRequestResponseAsync(
                    httpClient,
                    input.Method.ToString(),
                    input.Url,
                    content,
                    headers,
                    options,
                    cancellationToken)
                .ConfigureAwait(false))
            {
                var response = new Result
                {
                    Success = true,
                    Body = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false),
                    ContentType = responseMessage.Content.Headers.ContentType?.ToString(),
                    Headers = GetResponseHeaderDictionary(responseMessage.Headers, responseMessage.Content.Headers),
                    StatusCode = (int)responseMessage.StatusCode,
                };

                if (!responseMessage.IsSuccessStatusCode && options.ThrowExceptionOnErrorResponse)
                {
                    throw new WebException($"Request to '{input.Url}' failed with status code {(int)responseMessage.StatusCode}.");
                }

                return response;
            }
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

    private static HttpContent GetContent(Input input, IDictionary<string, string> headers)
    {
        if (headers.TryGetValue("content-type", out string contentTypeValue))
        {
            var contentTypeIsSetAndValid = MediaTypeWithQualityHeaderValue.TryParse(contentTypeValue, out var validContentType);
            if (contentTypeIsSetAndValid)
                return new StringContent(input.Message ?? string.Empty, Encoding.GetEncoding(validContentType.CharSet ?? Encoding.UTF8.WebName));
        }

        return new StringContent(input.Message ?? string.Empty);
    }

    private static HttpClient GetHttpClientForOptions(Options options)
    {
        var cacheKey = GetHttpClientCacheKey(options);

        if (ClientCache.Get(cacheKey) is HttpClient httpClient)
        {
            return httpClient;
        }

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

        var salt = Encoding.UTF8.GetBytes("Frends.HTTP.RequestBytes.HttpClientCacheKey");
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

        using (var request = new HttpRequestMessage(new HttpMethod(method), new Uri(url))
        {
            Content = isContentAllowed ? content : null,
        })
        {
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
                    {
                        Trace.TraceWarning($"Could not add header {header.Key}:{header.Value}");
                    }
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
                {
                    // Cancellation is from outside -> Just throw
                    throw;
                }

                // Cancellation is from inside of the request, mostly likely a timeout
                throw new Exception("HttpRequest was canceled, most likely due to a timeout.", canceledException);
            }

            // this check is probably not needed anymore as the new HttpClient does not fail on invalid charsets
            if (options.AllowInvalidResponseContentTypeCharSet && response.Content.Headers?.ContentType != null)
            {
                response.Content.Headers.ContentType.CharSet = null;
            }

            return response;
        }
    }
}
