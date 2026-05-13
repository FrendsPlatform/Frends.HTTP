namespace Frends.HTTP.SendBytes.Definitions
{
    /// <summary>
    /// HTTP protocol version used for requests.
    /// </summary>
    public enum HttpVersion
    {
        /// <summary>
        /// HTTP/1.1 - default, widely supported.
        /// </summary>
        Http11,
        /// <summary>
        /// HTTP/2 - multiplexed, requires server support. 
        /// If the server does not support HTTP/2 via ALPN, the request will fail with an exception instead of falling back to HTTP/1.1.
        /// </summary>
        Http20
    }
}
