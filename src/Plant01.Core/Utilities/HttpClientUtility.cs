using System.Net.Http.Headers;
using System.Security.Authentication;

namespace Plant01.Core.Utilities
{
    public static class HttpClientUtility
    {
        #region HttpClient 实例
        /// <summary>
        /// HttpClient
        /// </summary>
        /// <returns></returns>
        public static HttpClient DefaultClient
        {
            get
            {
                var handler = new SocketsHttpHandler
                {
                    UseCookies = false,
                    AllowAutoRedirect = false,
                    UseProxy = false,
                    MaxConnectionsPerServer = 256,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                        RemoteCertificateValidationCallback = delegate { return true; }
                    }
                };

                var httpClient = new HttpClient(handler, true)
                {
                    Timeout = new TimeSpan(0, 1, 0)
                };

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("KeepAlive", "false");
                httpClient.DefaultRequestHeaders.ExpectContinue = false;

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                return httpClient;
            }
        }
        #endregion
    }
}