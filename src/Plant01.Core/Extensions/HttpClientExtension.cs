using System.Net.Http.Headers;
using System.Text;

namespace Plant01.Core.Extensions
{
    public static class HttpClientExtension
    {
        #region 设置授权
        /// <summary>
        /// 设置授权
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="value"></param>
        /// <param name="scheme"></param>
        public static void SetAuthorization(this HttpClient httpClient, string value, string scheme = "bearer")
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, value);
        }
        #endregion

        #region GET
        /// <summary>
        /// GET
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public static async Task<string> Get(this HttpClient httpClient, string url)
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        #endregion

        #region POST
        /// <summary>
        /// POST
        /// </summary>
        /// <param name="httpClient">HttpClient</param>
        /// <param name="url">URL</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public static async Task<string> PostFormAsync(this HttpClient httpClient, string url, string parameter)
        {
            var response = await httpClient.PostAsync(url, new StringContent(parameter, Encoding.UTF8, "application/x-www-form-urlencoded"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        #endregion

        #region POST JSON
        /// <summary>
        /// POST JSON
        /// </summary>
        /// <param name="httpClient">HttpClient</param>
        /// <param name="url">URL</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public static async Task<string> PostJSONAsync(this HttpClient httpClient, string url, string parameter)
        {
            var response = await httpClient.PostAsync(url, new StringContent(parameter, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        #endregion
    }
}
