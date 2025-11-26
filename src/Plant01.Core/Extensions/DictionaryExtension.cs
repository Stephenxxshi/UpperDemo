using System.Text;

namespace Plant01.Core.Extensions
{
    public static class DictionaryExtension
    {
        #region 过滤数据
        /// <summary>
        /// 过滤数据
        /// </summary>
        /// <param name="dictionary">要过滤参数的字典</param>
        /// <param name="ignoreKeys">要忽略的键值</param>
        /// <returns>过滤后的字典</returns>
        public static T FilterNullAndEmpty<T>(this T dictionary, params string[] ignoreKeys) where T : IDictionary<string, string>, new()
        {
            var sArray = new T();
            foreach (var key in dictionary.Keys)
            {
                if (ignoreKeys.Contains(key) || string.IsNullOrWhiteSpace(dictionary[key]))
                    continue;
                sArray.Add(key, dictionary[key]);
            }
            return sArray;
        }
        #endregion

        #region 拼接链接
        /// <summary>
        /// 拼接链接
        /// </summary>
        /// <param name="dictionary">集合类型的变量</param>
        /// <param name="isIncludeKey">是否包含key</param>
        /// <param name="prefix">=</param>
        /// <param name="suffix">&</param>
        /// <returns>返回生成的url链接</returns>
        public static string SpliceUrl(this IDictionary<string, string> dictionary, bool isIncludeKey = true, string prefix = "=", string suffix = "&")
        {
            var builder = new StringBuilder();
            foreach (var dict in dictionary)
            {
                builder.Append($"{(isIncludeKey ? dict.Key : string.Empty)}{prefix}{dict.Value}{suffix}");
            }
            if (!string.IsNullOrWhiteSpace(suffix))
                builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }
        #endregion
    }
}