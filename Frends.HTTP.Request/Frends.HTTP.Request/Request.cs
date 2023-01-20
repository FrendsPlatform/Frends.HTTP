using Frends.HTTP.Request.Definitions;
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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Runtime.Caching;

namespace Frends.HTTP.Request;

/// <summary>
/// Task class.
/// </summary>
public class HTTP
{
    // For tests
    public static readonly ObjectCache ClientCache = MemoryCache.Default;

    private static readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) };

    public static void ClearClientCache()
    {
        var cacheKeys = ClientCache.Select(kvp => kvp.Key).ToList();
        foreach (var cacheKey in cacheKeys)
        {
            ClientCache.Remove(cacheKey);
        }
    }
    // For tests
    public static IHttpClientFactory ClientFactory = new HttpClientFactory();

    /// <summary>
    /// Execute a stored procedure to MySQL.
    /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.HTTP.Request)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"/>
    /// <returns>Object { int AffectedRows }</returns>
    public static async Task<Result> Request(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken
    )
    {
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

                cancellationToken.ThrowIfCancellationRequested();

                dynamic response;

                switch (input.ResultMethod) {
                    case ResultMethod.HTTP:
                        var hbody = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : null;
                        var hstatusCode = (int)responseMessage.StatusCode;
                        var hheaders = GetResponseHeaderDictionary(responseMessage.Headers, responseMessage.Content?.Headers);
                        response = new Result(hbody, hheaders, hstatusCode);
                        break;
                    case ResultMethod.REST:
                        var rbody = TryParseRequestStringResultAsJToken(await responseMessage.Content.ReadAsStringAsync()
                        .ConfigureAwait(false));
                        var rstatusCode = (int)responseMessage.StatusCode;
                        var rheaders = GetResponseHeaderDictionary(responseMessage.Headers, responseMessage.Content.Headers);
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

        //Ignore case for headers and key comparison
        return headers.ToDictionary(key => key.Name, value => value.Value, StringComparer.InvariantCultureIgnoreCase);
    }

    private static HttpContent GetContent(Input input, IDictionary<string, string> headers)
    {
        //Check if Content-Type exists and is set and valid
        var contentTypeIsSetAndValid = false;
        MediaTypeWithQualityHeaderValue validContentType = null;
        if (headers.TryGetValue("content-type", out string contentTypeValue))
        {
            contentTypeIsSetAndValid = MediaTypeWithQualityHeaderValue.TryParse(contentTypeValue, out validContentType);
        }

        return contentTypeIsSetAndValid
            ? new StringContent(input.Message ?? "", Encoding.GetEncoding(validContentType.CharSet ?? Encoding.UTF8.WebName))
            : new StringContent(input.Message ?? "");
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
        var cacheKey = GetHttpClientCacheKey(options);

        if (ClientCache.Get(cacheKey) is HttpClient httpClient)
        {
            return httpClient;
        }

        httpClient = ClientFactory.CreateClient(options);
        httpClient.SetDefaultRequestHeadersBasedOnOptions(options);

        ClientCache.Add(cacheKey, httpClient, _cachePolicy);

        return httpClient;
    }

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
            HttpClient httpClient, string method, string url,
            HttpContent content, IDictionary<string, string> headers,
            Options options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Only POST, PUT, PATCH and DELETE can have content, otherwise the HttpClient will fail
        var isContentAllowed = Definitions.Method.TryParse(method, ignoreCase: true, result: out SendMethod _);

        using (var request = new HttpRequestMessage(new HttpMethod(method), new Uri(url))
        {
            Content = isContentAllowed ? content : null,
        })
        {

            //Clear default headers
            content.Headers.Clear();
            foreach (var header in headers)
            {
                var requestHeaderAddedSuccessfully = request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                if (!requestHeaderAddedSuccessfully && request.Content != null)
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

public interface IHttpClientFactory
{
    HttpClient CreateClient(Options options);
}
public class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(Options options)
    {
        var handler = new HttpClientHandler();
        handler.SetHandlerSettingsBasedOnOptions(options);
        return new HttpClient(handler);
    }
}

public static class Extensions
{
    internal static void SetHandlerSettingsBasedOnOptions(this HttpClientHandler handler, Options options)
    {
        switch (options.Authentication)
        {
            case Authentication.WindowsIntegratedSecurity:
                handler.UseDefaultCredentials = true;
                break;
            case Authentication.WindowsAuthentication:
                var domainAndUserName = options.Username.Split('\\');
                if (domainAndUserName.Length != 2)
                {
                    throw new ArgumentException(
                        $@"Username needs to be 'domain\username' now it was '{options.Username}'");
                }

                handler.Credentials =
                    new NetworkCredential(domainAndUserName[1], options.Password, domainAndUserName[0]);
                break;
            case Authentication.ClientCertificate:
                handler.ClientCertificates.AddRange(GetCertificates(options));
                break;
        }

        handler.AllowAutoRedirect = options.FollowRedirects;
        handler.UseCookies = options.AutomaticCookieHandling;

        if (options.AllowInvalidCertificate)
        {
            handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;
        }
    }

    internal static void SetDefaultRequestHeadersBasedOnOptions(this HttpClient httpClient, Options options)
    {
        //Do not automatically set expect 100-continue response header
        httpClient.DefaultRequestHeaders.ExpectContinue = false;
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("content-type", "application/json");
        httpClient.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(options.ConnectionTimeoutSeconds));
    }

    private static X509Certificate[] GetCertificates(Options options)
    {
        X509Certificate2[] certificates;

        switch (options.ClientCertificateSource)
        {
            case CertificateSource.CertificateStore:
                var thumbprint = options.CertificateThumbprint;
                certificates = GetCertificatesFromStore(thumbprint, options.LoadEntireChainForCertificate);
                break;
            case CertificateSource.File:
                certificates = GetCertificatesFromFile(options.ClientCertificateFilePath, options.ClientCertificateKeyPhrase);
                break;
            case CertificateSource.String:
                certificates = GetCertificatesFromString(options.ClientCertificateInBase64, options.ClientCertificateKeyPhrase);
                break;
            default:
                throw new Exception("Unsupported Certificate source");
        }

        return certificates.Cast<X509Certificate>().ToArray();
    }

    private static X509Certificate2[] GetCertificatesFromString(string certificateContentsBase64, string keyPhrase)
    {
        var certificateBytes = Convert.FromBase64String(certificateContentsBase64);

        return LoadCertificatesFromBytes(certificateBytes, keyPhrase);
    }

    private static X509Certificate2[] LoadCertificatesFromBytes(byte[] certificateBytes, string keyPhrase)
    {
        var collection = new X509Certificate2Collection();

        if (!string.IsNullOrEmpty(keyPhrase))
        {
            collection.Import(certificateBytes, keyPhrase, X509KeyStorageFlags.PersistKeySet);
        }
        else
        {
            collection.Import(certificateBytes, null, X509KeyStorageFlags.PersistKeySet);
        }
        return collection.Cast<X509Certificate2>().OrderByDescending(c => c.HasPrivateKey).ToArray();

    }

    private static X509Certificate2[] GetCertificatesFromFile(string clientCertificateFilePath, string keyPhrase)
    {
        return LoadCertificatesFromBytes(File.ReadAllBytes(clientCertificateFilePath), keyPhrase);
    }

    private static X509Certificate2[] GetCertificatesFromStore(string thumbprint,
        bool loadEntireChain)
    {
        thumbprint = Regex.Replace(thumbprint, @"[^\da-zA-z]", string.Empty).ToUpper();
        using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
        {
            store.Open(OpenFlags.ReadOnly);
            var signingCert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (signingCert.Count == 0)
            {
                throw new FileNotFoundException(
                    $"Certificate with thumbprint: '{thumbprint}' not found in current user cert store.");
            }

            var certificate = signingCert[0];


            if (!loadEntireChain)
            {
                return new[] { certificate };
            }

            var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.Build(certificate);

            // include the whole chain
            var certificates = chain
                .ChainElements.Cast<X509ChainElement>()
                .Select(c => c.Certificate)
                .OrderByDescending(c => c.HasPrivateKey)
                .ToArray();

            return certificates;
        }
    }
}
