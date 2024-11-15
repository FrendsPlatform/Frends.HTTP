using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.HTTP.UploadFile.Definitions
{
    /// <summary>
    /// Represents the input data for the HTTP file upload operation.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// The HTTP Method to be used with the request.
        /// </summary>
        /// <example>GET</example>
        public Method Method { get; set; }

        /// <summary>
        /// The URL with protocol and path. You can include query parameters directly in the url.
        /// </summary>
        /// <example>https://example.org/path/to</example>
        [DefaultValue("https://example.org/path/to")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Url { get; set; }

        /// <summary>
        /// The file path to be posted
        /// </summary>
        /// <example>C:\Users</example>
        public string FilePath { get; set; }

        /// <summary>
        /// List of HTTP headers to be added to the request.
        /// </summary>
        /// <example>Name: Header, Value: HeaderValue</example>
        public Header[] Headers { get; set; }
    }
}
