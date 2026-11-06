using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.UploadFile.Definitions;

namespace Frends.HTTP.UploadFile;

/// <summary>
/// Represents a task that posts a file to a web API endpoint.
/// </summary>
public static class UploadFileTask
{
    /// <summary>
    /// Send file using StreamContent, the file data is read from a Stream and sent as the content of an HTTP request.
    /// StreamContent is a class provided by the .NET framework that allows you to send content from a Stream in an HTTP request
    /// </summary>
    /// <param name="input">The input parameters specifying the file to be sent.</param>
    /// <param name="options">The optional parameters controlling the file upload behavior.</param>
    /// <param name="cancellationToken">The cancellation token that can be used to cancel the upload operation.</param>
    /// <returns>Object { string Body, Dictionary[string, string] Headers, int StatusCode }</returns>
    public static async Task<Response> UploadFile([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        using var handler = new HttpClientHandler();
        handler.SetHandleSettingsBasedOnOptions(options);

        using var httpClient = new HttpClient(handler);
        var responseMessage = await GetHttpRequestResponseAsync(httpClient, input, options, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        string body = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        HttpHeaders contentHeaders = responseMessage.Content.Headers;
        HttpHeaders headers = responseMessage.Headers;

        var responseHeaders = CombineHeaders(new HttpHeaders[] { headers, contentHeaders });

        var response = new Response
        {
            Body = body,
            StatusCode = (int)responseMessage.StatusCode,
            Headers = responseHeaders
        };

        if (!responseMessage.IsSuccessStatusCode && options.ThrowExceptionOnErrorResponse)
        {
            throw new WebException($"Request to '{input.Url}' failed with status code {(int)responseMessage.StatusCode}. Response body: {response.Body}");
        }

        return response;
    }

    //Combine http headears collections into one dictionary
    private static Dictionary<string, string> CombineHeaders(HttpHeaders[] headers)
    {
        var result = headers
            .SelectMany(dict => dict)
            .ToDictionary(pair => pair.Key, pair => string.Join(";", pair.Value));
        return result;
    }

    private static async Task<HttpResponseMessage> GetHttpRequestResponseAsync(HttpClient httpClient, Input input, Options options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (options.Authentication == Authentication.Basic || options.Authentication == Authentication.OAuth)
        {
            switch (options.Authentication)
            {
                case Authentication.Basic:
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}")));
                    break;
                case Authentication.OAuth:
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        options.Token);
                    break;
            }
        }

        using MemoryStream reader = new(File.ReadAllBytes(input.FilePath));
        using HttpContent content = new StreamContent(reader);
        var headerDict = input.Headers.ToDictionary(key => key.Name, value => value.Value, StringComparer.InvariantCultureIgnoreCase);

        foreach (var header in headerDict)
        {
            var requestHeaderAddedSuccessfully = httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            if (!requestHeaderAddedSuccessfully)
            {
                // Could not add to request headers, try to add to content headers
                var contentHeaderAddedSuccessfully = content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                if (!contentHeaderAddedSuccessfully)
                {
                    Trace.TraceWarning($"Could not add header {header.Key}:{header.Value}");
                }
            }
        }

        var request = new HttpRequestMessage(new HttpMethod(input.Method.ToString()), new Uri(input.Url))
        {
            Content = content
        };

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        return response;
    }
}