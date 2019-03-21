using System;
using System.Net;
using System.Net.Http;

namespace Framework.WebAPI.CrossClient
{
    /// <summary>
    /// Singleton pattern to use properly the HttpClient class by reusing its open sockets (tcp) before they time out regardless whether the dispose was called or not
    /// This issue was raised by a Microsoft employee during th Microsoft Build 2018 event.
    /// <see cref="https://www.red-gate.com/simple-talk/dotnet/c-programming/working-with-the-httpclient-class/"/>
    /// <see cref="https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/"/>
    /// </summary>
    public class HttpClientSingleton : HttpClient
    {
        #region| Properties | 

        private static readonly Lazy<HttpClientSingleton> instance = new Lazy<HttpClientSingleton>(() => new HttpClientSingleton(true), true);
        public static HttpClientSingleton Instance
        {
            get
            {
                return instance.Value;
            }
        }

        #endregion

        #region| Constructor |

        static HttpClientSingleton() { }

        private HttpClientSingleton(bool useDefaultCredentials = true) : base
        (
            new HttpClientHandler()
            {
                UseDefaultCredentials = useDefaultCredentials,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            }
        )
        {
            Timeout = TimeSpan.FromMilliseconds(15000);
            MaxResponseContentBufferSize = 256000;
        }

        #endregion

        #region| Methods |

        public static HttpClient GetClient()
        {
            return instance.Value;
        }

        #endregion

    }
}