using System.Collections.Generic;

namespace Frends.HTTP.UploadFile.Definitions;


/// <summary>
/// Represents the response received from the HTTP server after sending a request.
/// </summary>
public class Response
{
    /// <summary>
    /// The body of the response.
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// {
    ///     "message": "Hello, world!",
    ///     "status": 200
    /// }
    /// ]]>
    /// </example>
    public string Body { get; set; }

    /// <summary>
    /// The headers of the response, as a dictionary of key-value pairs.
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// {
    ///     "Content-Type": "application/json",
    ///     "Cache-Control": "no-cache",
    ///     "X-Auth-Token": "abcdef123456"
    /// }
    /// ]]>
    /// </example>
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>
    /// The status code of the response, as an integer value.
    /// </summary>
    /// <example>200</example>
    public int StatusCode { get; set; }
}