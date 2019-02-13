using System.Collections.Generic;

namespace Framework.WebAPI.CrossClient
{
    /// <summary>
    /// This class represents a bad request response from a Web API
    /// </summary>
    public class BadRequestResponse
    {
        public Dictionary<string, string[]> ModelState { get; set; }
    }
}
