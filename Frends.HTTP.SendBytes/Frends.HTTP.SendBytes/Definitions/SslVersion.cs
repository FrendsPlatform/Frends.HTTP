namespace Frends.HTTP.SendBytes.Definitions
{
    /// <summary>
    /// SSL/TLS protocol version used for secure connections.
    /// </summary>
    public enum SslVersion
    {
        /// <summary>
        /// OS decides the protocol version.
        /// </summary>
        Default,
        /// <summary>
        /// TLS 1.2 only.
        /// </summary>
        Tls12,
        /// <summary>
        /// TLS 1.3 only.
        /// </summary>
        Tls13,
        /// <summary>
        /// TLS 1.2 and TLS 1.3 - use when server compatibility is uncertain.
        /// </summary>
        Tls12And13
    }
}
