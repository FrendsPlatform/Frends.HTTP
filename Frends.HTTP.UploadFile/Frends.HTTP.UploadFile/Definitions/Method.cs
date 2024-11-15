namespace Frends.HTTP.UploadFile.Definitions
{

    /// <summary>
    /// Represents the HTTP method to be used with the request.
    /// </summary>
    public enum Method
    {
        /// <summary>
        /// The HTTP POST method is used to submit an entity to the specified resource, often causing a change in state or side effects on the server.
        /// </summary>
        POST,
        /// <summary>
        /// The HTTP PUT method is used to replace or update a current resource with new content.
        /// </summary>
        PUT
    }
}