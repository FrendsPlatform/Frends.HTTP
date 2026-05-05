namespace Frends.HTTP.Request.Definitions
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
        /// </summary>
        Http20
    }
}
