using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Framework.Core;
using Framework.Entity;

using Newtonsoft.Json;

namespace Framework.WebAPI.CrossClient
{
    /// <summary>
    /// Api Client in charge to connecting to the Web Api Gateway
    /// </summary>
    public class ApiEngine : IDisposable
    {
        #region| Fields |

        private string currentToken;
        public ApiCredentials Credentials { get; set; }

        public string ClientIP { get; private set; }

        #endregion

        #region| Properties |

        public string CurrentEndpoint { get; set; }
        public string CurrentToken
        {
            get
            {
                return this.currentToken;
            }
        }

        /// <summary>
        /// Gets or sets a value that controls whether default credentials are sent with requests by the handler.
        /// </summary>
        /// Returns true if the default credentials are used; otherwise false. The default value is false.
        private bool UseDefaultCredentials { get; set; }

        #endregion

        #region| Constructor |
        
        /// <summary>
        /// Constructor with authentication
        /// </summary>       
        public ApiEngine(ApiCredentials credentials, bool useCredentials = true, string clientIP = "")
        {
            this.Credentials           = credentials;
            this.UseDefaultCredentials = useCredentials;
            this.ClientIP              = clientIP;
        }

        #endregion

        #region| Methods |

        #region| GET |

        /// <summary>
        /// Gets a string from a web api endpoint using the GET HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>json or xml result</returns>
        public Response<string> Get(string url, ApiCredentials credentials = null)
        {
            var output = new Response<string>();

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.GetAsync(requestUri).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        GetResponseContent(output, response);
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Gets a string from a web api endpoint using the GET HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>json or xml result</returns>
        public async Task<Response<string>> GetAsync(string url, ApiCredentials credentials = null)
        {
            var result = await ValidateAsync(url, credentials);

            if (result.IsPunchoutRequired())
            {
                return PunchoutSetupRequestMessage();
            }

            if (result.IsAuthenticationRequired())
            {
                return AuthenticationRequiredMessage();
            }

            var output = new Response<string>();

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.GetAsync(requestUri);

                    if (response.IsSuccessStatusCode)
                    {
                        await GetResponseContentAsync(output, response);
                    }
                    else
                    {
                        await GetDetailsAsync(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Gets an object from a web api endpoint using the GET HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        public Response<T> Get<T>(string url, ApiCredentials credentials = null) where T : BusinessEntityStructure
        {
            var output = new Response<T>();

            var response = Get(url, credentials);

            if (response.IsOk)
            {
                output.Data = Convert<T>(response);
                output.IsOk = true;

                output.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                output.StatusCode = response.StatusCode;

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    output.ModelState = response.ModelState;
                }
                else
                {
                    output.ErrorMessage = response.ErrorMessage;
                }
            }

            return output;
        }

        /// <summary>
        /// Gets an object from a web api endpoint using the GET HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        public async Task<Response<T>> GetAsync<T>(string url, ApiCredentials credentials = null) where T : BusinessEntityStructure
        {
            var output = new Response<T>();

            var response = await GetAsync(url, credentials);

            if (response.IsOk)
            {
                output.Data = Convert<T>(response);
                output.IsOk = true;

                output.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                output.StatusCode = response.StatusCode;

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    output.ModelState = response.ModelState;
                }
                else
                {
                    output.ErrorMessage = response.ErrorMessage;
                }
            }

            return output;
        }

        /// <summary>
        /// Gets a list of object from a web api endpoint using the GET HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        public Response<IEnumerable<T>> GetItems<T>(string url, ApiCredentials credentials = null) where T : BusinessEntityStructure
        {
            var output = new Response<IEnumerable<T>>();

            var response = Get(url, credentials);

            if (response.IsOk)
            {
                output.Data = Convert<IEnumerable<T>>(response);
                output.IsOk = true;

                output.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    output.ModelState = response.ModelState;
                }
                else
                {
                    output.ErrorMessage = response.ErrorMessage;
                }
            }

            return output;
        }

        /// <summary>
        /// Gets a list of object from a web api endpoint using the GET HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        public async Task<Response<IEnumerable<T>>> GetItemsAsync<T>(string url, ApiCredentials credentials = null) where T : BusinessEntityStructure
        {
            var output = new Response<IEnumerable<T>>();

            var response = await GetAsync(url, credentials);

            if (response.IsOk)
            {
                output.Data = Convert<IEnumerable<T>>(response);
                output.IsOk = true;

                output.StatusCode = HttpStatusCode.OK;


            }
            else
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    output.ModelState = response.ModelState;
                }
                else
                {
                    output.ErrorMessage = response.ErrorMessage;
                }
            }

            return output;
        }

        #endregion

        #region| POST |

        /// <summary>
        /// Sends a string to a web api endpoint using the POST HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        public Response<bool> Post(string url, ApiCredentials credentials = null)
        {
            Validate(url, credentials);

            var output = new Response<bool>();

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PostAsync(requestUri, new StringContent("")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Sends a string to a web api endpoint using the POST HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public async Task<Response<bool>> PostAsync(string url, ApiCredentials credentials = null)
        {
            await ValidateAsync(url, credentials);

            var output = new Response<bool>();

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.PostAsync(requestUri, new StringContent(""));

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Sends an object to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payload">payload object</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public Response<bool> Post<TInput>(string url, TInput payload, ApiCredentials credentials = null) where TInput : BusinessEntityStructure
        {
            Validate(url, credentials);

            var output = new Response<bool>();

            // Set to null the mapped properties cause it is not required in POST action
            payload.MappedProperties = null;

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PostAsJsonAsync(requestUri, payload).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Sends an object to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <typeparam name="TOutput">output generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payload">payload object</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public Response<TOutput> Post<TInput, TOutput>(string url, TInput payload, ApiCredentials credentials = null) where TInput : BusinessEntityStructure where TOutput: BusinessEntityStructure
        {
            Validate(url, credentials);

            var output = new Response<TOutput>();

            // Set to null the mapped properties cause it is not required in POST action
            payload.MappedProperties = null;

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PostAsJsonAsync(requestUri, payload).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        GetResponseContent<TOutput>(output, response);
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        ///  Sends an object asynchronously to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payload"></param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public async Task<Response<bool>> PostAsync<TInput>(string url, TInput payload, ApiCredentials credentials = null) where TInput : BusinessEntityStructure
        {
            await ValidateAsync(url, credentials);

            var output = new Response<bool>();

            // Set to null the mapped properties cause it is not required in POST action
            payload.MappedProperties = null;

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.PostAsJsonAsync(requestUri, payload);

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        ///  Sends an object asynchronously to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <typeparam name="TOutput">output generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payload"></param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public async Task<Response<TOutput>> PostAsync<TInput, TOutput>(string url, TInput payload, ApiCredentials credentials = null) where TInput : BusinessEntityStructure where TOutput : BusinessEntityStructure
        {
            await ValidateAsync(url, credentials);

            var output = new Response<TOutput>();

            // Set to null the mapped properties cause it is not required in POST action
            payload.MappedProperties = null;

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.PostAsJsonAsync(requestUri, payload);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        GetResponseContent<TOutput>(output, response);
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }


        /// <summary>
        /// Sends a list of object to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payloads">payload object</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public Response<bool> PostItems<TInput>(string url, IEnumerable<TInput> payloads, ApiCredentials credentials = null) where TInput : BusinessEntityStructure
        {
            Validate(url, credentials);

            var output = new Response<bool>();

            if (payloads.IsNotNull())
            {
                // Set to null the mapped properties cause it is not required in POST action
                foreach (var item in payloads.ToList())
                {
                    item.MappedProperties = null;
                }
            }

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PostAsJsonAsync(requestUri, payloads).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Sends a list of object to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <typeparam name="TOutput">output generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payloads">payload object</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public Response<TOutput> PostItems<TInput, TOutput>(string url, IEnumerable<TInput> payloads, ApiCredentials credentials = null) where TInput : BusinessEntityStructure where TOutput: BusinessEntityStructure
        {
            Validate(url, credentials);

            var output = new Response<TOutput>();

            if (payloads.IsNotNull())
            {
                // Set to null the mapped properties cause it is not required in POST action
                foreach (var item in payloads.ToList())
                {
                    item.MappedProperties = null;
                }
            }

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PostAsJsonAsync(requestUri, payloads).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        GetResponseContent<TOutput>(output, response);
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }


        /// <summary>
        /// Sends a list of object to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <typeparam name="TOutput">output generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payload">payload object</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public Response<IEnumerable<TOutput>> PostItems<TInput, TOutput>(string url, TInput payload, ApiCredentials credentials = null) where TInput : BusinessEntityStructure where TOutput : BusinessEntityStructure
        {
            Validate(url, credentials);

            var output = new Response<IEnumerable<TOutput>>();

            if (payload.IsNotNull())
            {
                payload.MappedProperties = null;
            }

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PostAsJsonAsync(requestUri, payload).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        GetResponseContent(output, response);
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        ///  Sends a list of objects to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payloads"></param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public async Task<Response<bool>> PostItemsAsync<TInput>(string url, IEnumerable<TInput> payloads, ApiCredentials credentials = null) where TInput : BusinessEntityStructure
        {
            await ValidateAsync(url, credentials);

            var output = new Response<bool>();

            if (payloads.IsNotNull())
            {
                // Set to null the mapped properties cause it is not required in POST action
                foreach (var item in payloads.ToList())
                {
                    item.MappedProperties = null;
                }
            }

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.PostAsJsonAsync(requestUri, payloads);

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        ///  Sends a list of objects to a web api endpoint using the POST Http verb
        /// </summary>
        /// <typeparam name="TInput">input generic param type</typeparam>
        /// <typeparam name="TOutput">output generic param type</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payloads"></param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public async Task<Response<TOutput>> PostItemsAsync<TInput, TOutput>(string url, IEnumerable<TInput> payloads, ApiCredentials credentials = null) where TInput : BusinessEntityStructure where TOutput : BusinessEntityStructure
        {
            await ValidateAsync(url, credentials);

            var output = new Response<TOutput>();

            if (payloads.IsNotNull())
            {
                // Set to null the mapped properties cause it is not required in POST action
                foreach (var item in payloads.ToList())
                {
                    item.MappedProperties = null;
                }
            }

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.PostAsJsonAsync(requestUri, payloads);

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        GetResponseContent<TOutput>(output, response);
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        #endregion

        #region| PUT |

        /// <summary>
        /// Sends a string to a web api endpoint using the PUT HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        public Response<bool> Put(string url, ApiCredentials credentials = null)
        {
            Validate(url, credentials);

            var output = new Response<bool>();

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PutAsync(requestUri, new StringContent("")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Sends a string to a web api endpoint using the PUT HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public async Task<Response<bool>> PutAsync(string url, ApiCredentials credentials = null)
        {
            await ValidateAsync(url, credentials);

            var output = new Response<bool>();

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.PutAsync(requestUri, new StringContent(""));

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Sends an object to a web api endpoint using the PUT Http verb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payload">payload object</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public Response<bool> Put<T>(string url, T payload, ApiCredentials credentials = null) where T : BusinessEntityStructure
        {
            Validate(url, credentials);

            var output = new Response<bool>();

            // Set to null the mapped properties cause it is not required in PUT action
            payload.MappedProperties = null;

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PutAsJsonAsync(requestUri, payload).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        ///  Sends an object asynchronously to a web api endpoint using the PUT Http verb
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payload"></param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public async Task<Response<bool>> PutAsync<T>(string url, T payload, ApiCredentials credentials = null) where T : BusinessEntityStructure
        {
            await ValidateAsync(url, credentials);

            var output = new Response<bool>();

            // Set to null the mapped properties cause it is not required in PUT action
            payload.MappedProperties = null;

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.PutAsJsonAsync(requestUri, payload);

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Sends a list of object to a web api endpoint using the PUT Http verb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payloads">payload object</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public Response<bool> PutItems<T>(string url, IEnumerable<T> payloads, ApiCredentials credentials = null) where T : BusinessEntityStructure
        {
            Validate(url, credentials);

            var output = new Response<bool>();

            if (payloads.IsNotNull())
            {
                // Set to null the mapped properties cause it is not required in PUT action
                foreach (var item in payloads.ToList())
                {
                    item.MappedProperties = null;
                }
            }

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.PutAsJsonAsync(requestUri, payloads).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        ///  Sends a list of objects to a web api endpoint using the PUT Http verb
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="payloads"></param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns>Response</returns>
        public async Task<Response<bool>> PutItemsAsync<T>(string url, IEnumerable<T> payloads, ApiCredentials credentials = null) where T : BusinessEntityStructure
        {
            await ValidateAsync(url, credentials);

            var output = new Response<bool>();

            if (payloads.IsNotNull())
            {
                // Set to null the mapped properties cause it is not required in PUT action
                foreach (var item in payloads.ToList())
                {
                    item.MappedProperties = null;
                }
            }

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.PutAsJsonAsync(requestUri, payloads);

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        #endregion

        #region| DELETE |

        /// <summary>
        /// Sends a string to a web api endpoint using the DELETE HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        public Response<string> Delete(string url, ApiCredentials credentials = null)
        {
            Validate(url, credentials);

            var output = new Response<string>();

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = client.DeleteAsync(requestUri).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        /// <summary>
        /// Sends a string to a web api endpoint using the PUT HttpVerb
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        public async Task<Response<string>> DeleteAsync(string url, ApiCredentials credentials = null)
        {
            await ValidateAsync(url, credentials);

            var output = new Response<string>();

            try
            {
                var requestUri = GetUrl(url);

                using (var client = GetClient())
                {
                    SetHeader(client);

                    var response = await client.DeleteAsync(requestUri);

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();

                        output.IsOk = true;
                        output.StatusCode = response.StatusCode;
                    }
                    else
                    {
                        GetDetails(output, response);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(output, e);
            }

            return output;
        }

        #endregion

        #region| Shared |

        /// <summary>
        /// Set the default request headers with all parameters required to invoke a WebAPI with authentication
        /// </summary>
        /// <param name="client">HttpClient</param>
        private void SetHeader(HttpClient client)
        {
            // Use the given url from the parameter method
            //client.BaseAddress = new Uri(endpoint);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add to the request header the client IP Address
            client.DefaultRequestHeaders.Add("clientIPAddress", this.ClientIP);

            if (this.CurrentToken.IsNotNull())
            {
                client.DefaultRequestHeaders.Add("tokenCode", this.CurrentToken);
            }
        }

        /// <summary>
        /// Try to get the response content from a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="response"></param>
        private void GetResponseContent(Response<string> output, HttpResponseMessage response)
        {
            output.IsOk = true;
            output.StatusCode = response.StatusCode;

            // by calling .Result you are performing a synchronous call
            var responseContent = response.Content;

            // by calling .Result you are synchronously reading the result
            var resultContent = responseContent.ReadAsStringAsync().Result;

            if (resultContent.IsNotNull())
            {
                resultContent = resultContent.Replace("\\", "").Trim(new char[1] { '"' });

                // Convert the received string to the given class
                output.Data = resultContent;
            }
        }

        /// <summary>
        /// Try to get the response content from a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="response"></param>
        private void GetResponseContent<T>(Response<T> output, HttpResponseMessage response)
        {
            output.IsOk = true;
            output.StatusCode = response.StatusCode;

            // by calling .Result you are performing a synchronous call
            var responseContent = response.Content;

            // by calling .Result you are synchronously reading the result
            var resultContent = responseContent.ReadAsStringAsync().Result;

            if (resultContent.IsNotNull())
            {
                // Convert the received string to the given class
                output.Data = Convert<T>(resultContent);
            }
        }

        /// <summary>
        /// Try to get the response content from a GET/POST request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="response"></param>
        private async Task GetResponseContentAsync(Response<string> output, HttpResponseMessage response)
        {
            output.IsOk = true;
            output.StatusCode = response.StatusCode;

            // by calling .Result you are performing a synchronous call
            var responseContent = response.Content;

            // by calling .Result you are synchronously reading the result
            var resultContent = await responseContent.ReadAsStringAsync();

            if (resultContent.IsNotNull())
            {
                // Convert the received string to the given class
                output.Data = resultContent;
            }
        }

        /// <summary>
        /// Get the details regarding whether it is a business rule validation error or internal error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="response"></param>
        private void GetDetails<T>(Response<T> output, HttpResponseMessage response)
        {
            output.IsOk = false;
            output.StatusCode = response.StatusCode;

            // Business error and validations
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                // by calling .Result you are performing a synchronous call
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                var badRequestResponse = response.Content.ReadAsAsync<BadRequestResponse>().Result;

                if (badRequestResponse.IsNotNull())
                {
                    // Convert the received string to the given class
                    output.ModelState = badRequestResponse.ModelState;
                }
            }
            else
            {
                if (output.StatusCode != HttpStatusCode.NotFound)
                {
                    output.ErrorMessage = response.ReasonPhrase + ". " + response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    output.ErrorMessage = "Endpoint or resource not found";
                }
            }
        }

        /// <summary>
        /// Get the details regarding whether it is a business rule validation error or internal error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="response"></param>
        private async Task GetDetailsAsync<T>(Response<T> output, HttpResponseMessage response)
        {
            output.IsOk = false;
            output.StatusCode = response.StatusCode;

            // Business error and validations
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                // by calling .Result you are performing a synchronous call
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                var badRequestResponse = await response.Content.ReadAsAsync<BadRequestResponse>();

                if (badRequestResponse.IsNotNull())
                {
                    // Convert the received string to the given class
                    output.ModelState = badRequestResponse.ModelState;
                }
            }
            else
            {
                if (output.StatusCode != HttpStatusCode.NotFound)
                {
                    output.ErrorMessage = response.ReasonPhrase + ". " + await response.Content.ReadAsStringAsync();
                }
                else
                {
                    output.ErrorMessage = "Endpoint or resource not found";
                }
            }
        }

        /// <summary>
        /// Get the endpoint url format
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetUrl(string url)
        {
            if (this.CurrentEndpoint.IsNotNull())
            {
                return this.CurrentEndpoint + url;
            }
            else
            {
                return url;
            }

        }

        /// <summary>
        ///  Convert the received string to the given class
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="content">The JSON to deserialize.</param>
        /// <returns></returns>
        public T Convert<T>(Response<string> input)
        {
            // Convert the received string to the given class
            var output = JsonConvert.DeserializeObject<T>(input.Data);

            return output;
        }

        /// <summary>
        ///  Convert the received string to the given class
        /// </summary>
        /// <typeparam name="T">param T</typeparam>
        /// <param name="content">The JSON to deserialize.</param>
        /// <returns></returns>
        public T Convert<T>(string input)
        {
            // Convert the received string to the given class
            var output = JsonConvert.DeserializeObject<T>(input);

            return output;
        }


        private void HandleError<T>(Response<T> output, Exception e)
        {
            output.IsOk = false;
            output.ErrorMessage = e.Message;

            if (e.InnerException.IsNotNull())
            {
                output.ErrorMessage += Environment.NewLine + e.InnerException.Message;

                if (e.InnerException.InnerException.IsNotNull())
                {
                    output.ErrorMessage += Environment.NewLine + e.InnerException.InnerException.Message;

                    if (e.InnerException.InnerException.InnerException.IsNotNull())
                    {
                        output.ErrorMessage += Environment.NewLine + e.InnerException.InnerException.InnerException.Message;
                    }
                }
            }
        }
        private HttpClient GetClient()
        {
            return HttpClientSingleton.GetClient();

            //HttpClient output = null;

            //if (UseDefaultCredentials)
            //{
            //    output = new HttpClient(new HttpClientHandler()
            //    {
            //        UseDefaultCredentials = true
            //    });
            //}
            //else
            //{
            //    output = new HttpClient();
            //}

            //return output;
        }
        #endregion

        #region| Authentication .

        /// <summary>
        /// PunchOut is an easy-to-implement protocol for interactive sessions managed across the Internet. Using real-time,
        /// synchronous messages, it enables communication between applications, providing seamless interaction at remote sites.
        /// </summary>
        public async Task<List<string>> PunchoutSetupRequest()
        {
            var output = new List<string>();

            if (output.IsNull())
            {
                try
                {
                    using (var client = GetClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Punchout setup request settings
                        client.DefaultRequestHeaders.Add("clientId", Credentials.ClientId);
                        client.DefaultRequestHeaders.Add("sharedSecret", Credentials.SharedSecret);
                        client.DefaultRequestHeaders.Add("clientIPAddress", this.ClientIP);

                        var response = await client.GetAsync(Credentials.BaseAddress);

                        if (response.IsSuccessStatusCode)
                        {
                            // by calling .Result you are performing a synchronous call
                            var responseContent = response.Content;

                            // by calling .Result you are synchronously reading the result
                            var resultContent = responseContent.ReadAsStringAsync().Result;

                            if (resultContent.IsNotNull())
                            {
                                resultContent = resultContent.Replace("\\", "").Trim(new char[1] { '"' });

                                this.CurrentEndpoint = resultContent;
                            }
                            else
                            {
                                output.Add("The api endpoint is null or empty");
                            }

                            output.Clear();
                        }
                        else
                        {
                            output.Add($"Status:{response.StatusCode.ToString()} - Detail: {response.ReasonPhrase}");
                        }
                    }
                }
                catch (AggregateException e)
                {
                    output.Add($"The api endpoint is not a valid string input. Details: { e.Message }");
                }
                catch (UriFormatException e)
                {
                    output.Add($"The api endpoint is not a valid string input. Details:{ e.Message }");
                }
                catch (Exception e)
                {
                    output.Add(e.Message);
                }
            }

            return output;
        }

        /// <summary>
        /// Acomplishe the user authentication and authorization process
        /// </summary>
        /// <returns></returns>
        public async Task<T> Authenticate<T>() where T: ITokenUser
        {
            var output = Activator.CreateInstance<T>();

            output.AuthenticationStatus = new List<string>();

            var IsAuthenticationEnabled = Core.Extensions.GetAppSettings("FRAMEWORK.CROSS.WEBAPI.IsAuthenticationEnabled");

            IsAuthenticationEnabled.ThrowIfNull();

            if (IsAuthenticationEnabled.IsEqual("true"))
            {
                try
                {
                    using (var client = GetClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Punchout setup request settings
                        client.DefaultRequestHeaders.Add("username", Credentials.Username);
                        client.DefaultRequestHeaders.Add("password", Credentials.Password);
                        client.DefaultRequestHeaders.Add("applicationCode", Credentials.ApplicationCode.ToString());

                        var requestUri = GetUrl("authenticate");

                        var response = await client.GetAsync(requestUri);

                        if (response.IsSuccessStatusCode)
                        {
                            // by calling .Result you are performing a synchronous call
                            var responseContent = response.Content;

                            // by calling .Result you are synchronously reading the result
                            var resultContent = responseContent.ReadAsStringAsync().Result;

                            output = JsonConvert.DeserializeObject<T>(resultContent);

                            this.currentToken = output.Token;

                            // Set the response status to null to indicate that everything is okay
                            output.AuthenticationStatus = null;
                        }
                        else
                        {
                            // by calling .Result you are performing a synchronous call
                            var responseContent = response.Content;

                            // by calling .Result you are synchronously reading the result
                            var resultContent = responseContent.ReadAsStringAsync().Result;

                            var badRequest = JsonConvert.DeserializeObject<BadRequestResponse>(resultContent);

                            if(badRequest.IsNotNull() && badRequest.ModelState.IsNotNull())
                            {
                                var details = string.Empty;

                                foreach (var item in badRequest.ModelState)
                                {
                                    details += item.Key + ":";

                                    foreach (var chd in item.Value)
                                    {
                                        details += chd;
                                    }

                                    details += Environment.NewLine;
                                }

                                output.AuthenticationStatus.Add($"Status:{response.StatusCode.ToString()} - Detail: {details}");

                            }
                            else
                            {
                                output.AuthenticationStatus.Add($"Status:{response.StatusCode.ToString()} - Detail: {response.ReasonPhrase}");
                            }

                            
                        }
                    }
                }
                catch (AggregateException e)
                {
                    output.AuthenticationStatus.Add($"The api endpoint is not a valid string input. Details: { e.Message }");
                }
                catch (UriFormatException e)
                {
                    output.AuthenticationStatus.Add($"The api endpoint is not a valid string input. Details:{ e.Message }");
                }
                catch (Exception e)
                {
                    output.AuthenticationStatus.Add(e.Message);
                }
            }

            return output;
        }

        /// <summary>
        /// Validate the punchout setup request and token based authentication
        /// </summary>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        private void Validate(string url, ApiCredentials credentials = null)
        {
            var IsAuthenticationEnabled = Core.Extensions.GetAppSettings("FRAMEWORK.CROSS.WEBAPI.IsAuthenticationEnabled");

            IsAuthenticationEnabled.ThrowIfNull();

            if (IsAuthenticationEnabled.IsEqual("true"))
            {
                if (credentials.IsNotNull())
                {
                    this.Credentials = credentials;
                }

                // Punchout setup request
                if (this.CurrentEndpoint.IsNull())
                {
                    throw new Exception("FRAMEWORK.WebAPI.CrossClient: Please carry out the punchout setup request in order to continue.");
                }

                //// Authentication / Authorization
                //if (url.StartsWith("authenticate") == false && this.CurrentToken.IsNull())
                //{
                //    Authenticate().Wait();
                //}
            }

        }

        /// <summary>
        /// Validate asynchronously the punchout setup request and token based authentication
        /// </summary>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="credentials">ApiCredentials</param>
        /// <returns></returns>
        private async Task<string> ValidateAsync(string url, ApiCredentials credentials = null)
        {
            var IsAuthenticationEnabled = Core.Extensions.GetAppSettings("FRAMEWORK.CROSS.WEBAPI.IsAuthenticationEnabled");

            IsAuthenticationEnabled.ThrowIfNull();

            if (IsAuthenticationEnabled.IsEqual("true"))
            {
                if (credentials.IsNotNull())
                {
                    this.Credentials = credentials;
                }

                // Punchout setup request
                if (this.CurrentEndpoint.IsNull())
                {
                    return "PUNCHOUT_SETUP_REQUEST";

                }

                // Authentication / Authorization
                if (url.StartsWith("authenticate") == false && this.CurrentToken.IsNull())
                {
                    //await Authenticate();
                    return "AUTHENTICATE";
                }
            }

            return "OK";
        }


        /// <summary>
        /// Returns a response from a task with punchout setup request notification
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static Response<string> PunchoutSetupRequestMessage()
        {
            var output = new Response<string>();

            output.IsOk = false;
            output.ErrorMessage = "Please, carry out the punchout setup request";
            output.StatusCode = HttpStatusCode.Unauthorized;

            return output;

        }

        /// <summary>
        /// Returns a response from a task that means that authentication is required in order to move forward
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static Response<string> AuthenticationRequiredMessage()
        {
            var output = new Response<string>();

            output.IsOk = false;
            output.ErrorMessage = "Please, carry out the authentication process";
            output.StatusCode = HttpStatusCode.Unauthorized;

            return output;

        }

        #endregion

        #endregion

        #region| IDisposable Members |

        public void Dispose()
        {
            
        }

        #endregion
    }
}
