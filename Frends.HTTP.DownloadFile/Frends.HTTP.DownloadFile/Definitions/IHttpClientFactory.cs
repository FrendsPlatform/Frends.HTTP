using System.Net.Http;

namespace Frends.HTTP.DownloadFile.Definitions;

/// <summary>
/// Http Client Factory Interface
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// Create client
    /// </summary>
    HttpClient CreateClient(Options options);
}
