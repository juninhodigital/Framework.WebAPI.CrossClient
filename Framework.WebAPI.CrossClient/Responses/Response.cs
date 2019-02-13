using System.Collections.Generic;
using System.Net;

namespace Framework.WebAPI.CrossClient
{
    /// <summary>
    /// Web API response object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Response<T>
    {
        /// <summary>
        /// Web API response result
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        ///  Gets a value that indicates if the HTTP response was successful.
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// Contains the values of status codes defined for HTTP.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Get the error message from the current api response
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Model states messages
        /// </summary>
        public Dictionary<string, string[]> ModelState { get; set; }
    }

}
