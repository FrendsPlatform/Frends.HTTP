using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using Frends.HTTP.Request.Definitions;

[assembly: InternalsVisibleTo("Frends.HTTP.Request.Tests")]

namespace Frends.HTTP.Request;

/// <summary>
/// Task class.
/// </summary>
public static class HTTP
{
    private static readonly ObjectCache ClientCache = MemoryCache.Default;

    private static readonly CacheItemPolicy CachePolicy = new()
    {
        SlidingExpiration = TimeSpan.FromHours(1),
    };

    private static HttpContent httpContent;
    private static HttpClient httpClient;
    private static HttpClientHandler httpClientHandler;
    private static HttpRequestMessage httpRequestMessage;
    private static HttpResponseMessage httpResponseMessage;
    private static X509Certificate2[] certificates = Array.Empty<X509Certificate2>();


    internal static void ClearClientCache()
    {
        var cacheKeys = ClientCache.Select(kvp => kvp.Key).ToList();

        foreach (var cacheKey in cacheKeys)
        {
            ClientCache.Remove(cacheKey);
        }
    }

    /// <summary>
    /// Frends Task for executing HTTP requests with String or JSON payload.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.HTTP.Request)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"/>
    /// <returns>Object { dynamic Body, Dictionary(string, string) Headers, int StatusCode }</returns>
    public static async Task<Result> Request(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (string.IsNullOrEmpty(input.Url)) throw new ArgumentNullException("Url can not be empty.");

            httpClient = GetHttpClientForOptions(options);
            var headers = GetHeaderDictionary(input.Headers, options);

            httpContent = GetContent(input, headers);
            using var responseMessage = await GetHttpRequestResponseAsync(
                    httpClient,
                    input.Method.ToString(),
                    input.Url,
                    httpContent,
                    headers,
                    options,
                    cancellationToken)
                .ConfigureAwait(false);

            Result response;

            switch (input.ResultMethod)
            {
                case ReturnFormat.String:
                    var hbody = responseMessage.Content != null
                        ? await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)
                        : null;
                    var hstatusCode = (int)responseMessage.StatusCode;
                    var hheaders =
                        GetResponseHeaderDictionary(responseMessage.Headers, responseMessage.Content?.Headers);
                    response = new Result(hbody, hheaders, hstatusCode);

                    break;
                case ReturnFormat.JToken:
                    var rbody = TryParseRequestStringResultAsJToken(await responseMessage.Content
                        .ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false));
                    var rstatusCode = (int)responseMessage.StatusCode;
                    var rheaders =
                        GetResponseHeaderDictionary(responseMessage.Headers, responseMessage.Content.Headers);
                    response = new Result(rbody, rheaders, rstatusCode);

                    break;
                default: throw new InvalidOperationException();
            }

            if (!responseMessage.IsSuccessStatusCode && options.ThrowExceptionOnErrorResponse)
            {
                throw new WebException(
                    $"Request to '{input.Url}' failed with status code {(int)responseMessage.StatusCode}. Response body: {response.Body}");
            }

            return response;
        }
        finally
        {
            httpContent?.Dispose();
            httpClient?.Dispose();
            httpClientHandler?.Dispose();
            httpRequestMessage?.Dispose();
            httpResponseMessage?.Dispose();
            foreach (var cert in certificates) cert?.Dispose();
        }
    }

    // Combine response- and responsecontent header to one dictionary
    private static Dictionary<string, string> GetResponseHeaderDictionary(HttpResponseHeaders responseMessageHeaders,
        HttpContentHeaders contentHeaders)
    {
        var responseHeaders = responseMessageHeaders.ToDictionary(h => h.Key, h => string.Join(";", h.Value));
        var allHeaders = contentHeaders?.ToDictionary(h => h.Key, h => string.Join(";", h.Value)) ??
                         new Dictionary<string, string>();
        responseHeaders.ToList().ForEach(x => allHeaders[x.Key] = x.Value);

        return allHeaders;
    }

    private static IDictionary<string, string> GetHeaderDictionary(Header[] headers, Options options)
    {
        if (!headers.Any(header => header.Name.ToLower().Equals("authorization")))
        {
            var authHeader = new Header
            {
                Name = "Authorization"
            };

            switch (options.Authentication)
            {
                case Authentication.Basic:
                    authHeader.Value =
                        $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"))}";
                    headers = headers.Concat(new[]
                    {
                        authHeader
                    }).ToArray();

                    break;
                case Authentication.OAuth:
                    authHeader.Value = $"Bearer {options.Token}";
                    headers = headers.Concat(new[]
                    {
                        authHeader
                    }).ToArray();

                    break;
            }
        }

        //Ignore case for headers and key comparison
        return headers.ToDictionary(key => key.Name, value => value.Value, StringComparer.InvariantCultureIgnoreCase);
    }

    private static HttpContent GetContent(Input input, IDictionary<string, string> headers)
    {
        var methodsWithBody = new[]
        {
            Method.POST, Method.PUT, Method.PATCH, Method.DELETE
        };

        if (!methodsWithBody.Contains(input.Method))
        {
            return new StringContent(string.Empty);
        }

        if (headers.TryGetValue("content-type", out string contentTypeValue))
        {
            var contentTypeIsSetAndValid =
                MediaTypeWithQualityHeaderValue.TryParse(contentTypeValue, out var validContentType);

            if (contentTypeIsSetAndValid)
                return new StringContent(input.Message ?? string.Empty,
                    Encoding.GetEncoding(validContentType.CharSet ?? Encoding.UTF8.WebName));
        }

        return new StringContent(input.Message ?? string.Empty);
    }

    private static object TryParseRequestStringResultAsJToken(string response)
    {
        try
        {
            return string.IsNullOrWhiteSpace(response) ? new JValue("") : JToken.Parse(response);
        }
        catch (JsonReaderException)
        {
            throw new JsonReaderException($"Unable to read response message as json: {response}");
        }
    }

    private static HttpClient GetHttpClientForOptions(Options options)
    {
        string cacheKey = null;

        if (options.CacheHttpClient)
        {
            cacheKey = GetHttpClientCacheKey(options);

            if (ClientCache.Get(cacheKey) is HttpClient client)
            {
                return client;
            }
        }

        httpClientHandler = new HttpClientHandler();
        httpClientHandler.SetHandlerSettingsBasedOnOptions(options, ref certificates);
        httpClient = new HttpClient(httpClientHandler);
        httpClient.SetDefaultRequestHeadersBasedOnOptions(options);

        if (cacheKey != null) ClientCache.Add(cacheKey, httpClient, CachePolicy);

        return httpClient;
    }

    [ExcludeFromCodeCoverage]
    private static string GetHttpClientCacheKey(Options options)
    {
        // Includes everything except for options.Token, which is used on request level, not http client level
        return $"{options.Authentication}:{options.Username}:{options.Password}:{options.ClientCertificateSource}"
               + $":{options.ClientCertificateFilePath}:{options.ClientCertificateInBase64}:{options.ClientCertificateKeyPhrase}"
               + $":{options.CertificateThumbprint}:{options.LoadEntireChainForCertificate}:{options.ConnectionTimeoutSeconds}"
               + $":{options.FollowRedirects}:{options.AllowInvalidCertificate}:{options.AllowInvalidResponseContentTypeCharSet}"
               + $":{options.ThrowExceptionOnErrorResponse}:{options.AutomaticCookieHandling}";
    }

    private static async Task<HttpResponseMessage> GetHttpRequestResponseAsync(
        HttpClient client, string method, string url,
        HttpContent content, IDictionary<string, string> headers,
        Options options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        httpRequestMessage = new HttpRequestMessage(new HttpMethod(method), new Uri(url));
        httpRequestMessage.Content = content;

        //Clear default headers
        content.Headers.Clear();

        foreach (var header in headers)
        {
            var requestHeaderAddedSuccessfully =
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (!requestHeaderAddedSuccessfully)
            {
                //Could not add to request headers try to add to content headers
                // this check is probably not needed anymore as the new HttpClient does not seem fail on malformed headers
                var contentHeaderAddedSuccessfully = content.Headers.TryAddWithoutValidation(header.Key, header.Value);

                if (!contentHeaderAddedSuccessfully)
                {
                    Trace.TraceWarning($"Could not add header {header.Key}:{header.Value}");
                }
            }
        }

        try
        {
            httpResponseMessage = await client.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
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
        if (options.AllowInvalidResponseContentTypeCharSet && httpResponseMessage.Content.Headers?.ContentType != null)
        {
            httpResponseMessage.Content.Headers.ContentType.CharSet = null;
        }

        return httpResponseMessage;
    }
}
