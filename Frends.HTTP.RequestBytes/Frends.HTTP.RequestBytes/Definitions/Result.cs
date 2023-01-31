using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Frends.HTTP.RequestBytes.Definitions;

/// <summary>
/// Result class
/// </summary>
public class Result
{
    /// <summary>
    /// Body of response
    /// </summary>
    /// <example>{"id": "abcdefghijkl123456789",  "success": true,  "errors": []}</example>
    public byte[] Body { get; private set; }

    /// <summary>
    /// Body size of response
    /// </summary>
    /// <example>10.0</example>
    public double BodySizeInMegaBytes => Math.Round((Body?.Length / (1024 * 1024d) ?? 0), 3);

    /// <summary>
    /// Header type of response
    /// </summary>
    /// <example>10.0</example>
    public MediaTypeHeaderValue ContentType { get; private set; }

    /// <summary>
    /// Headers of response
    /// </summary>
    /// <example>{[ "content-type": "application/json", ... ]}</example>
    public Dictionary<string, string> Headers { get; private set; }

    /// <summary>
    /// Statuscode of response
    /// </summary>
    /// <example>200</example>
    public int StatusCode { get; private set; }

    internal Result(byte[] body, MediaTypeHeaderValue contentType, Dictionary<string, string> headers, int statusCode)
    {
        Body = body;
        Headers = headers;
        StatusCode = statusCode;
        ContentType = contentType;
    }
}
