using System.Windows;
using System.Windows.Media;

namespace Plant01.WpfUI.Helpers
{
    public static class AppHelper
    {
        #region 搜索父级
        /// <summary>
        /// 搜索父级
        /// </summary>
        /// <param name="reference">依赖对象</param>
        /// <returns>TabControl</returns>
        public static T? FindParent<T>(DependencyObject reference) where T : class
        {
            var dObj = VisualTreeHelper.GetParent(reference);
            if (dObj == null)
                return default;
            if (dObj.GetType() == typeof(T))
                return dObj as T;
            else
                return FindParent<T>(dObj);
        }
        #endregion
    }
}
