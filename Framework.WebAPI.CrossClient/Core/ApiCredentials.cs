using System;

namespace Framework.WebAPI.CrossClient
{
    /// <summary>
    /// API client credentials used in the authentication and authorization process
    /// </summary>
    public class ApiCredentials
    {
        #region| Properties |

        /// <summary>
        /// Base Address
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// Client identification
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Application identification
        /// </summary>
        public int ApplicationCode { get; set; }

        /// <summary>
        /// Client shared secret
        /// </summary>
        public string SharedSecret { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Logged user
        /// </summary>
        public ITokenUser User { get; set; } 

        #endregion
    }
}
