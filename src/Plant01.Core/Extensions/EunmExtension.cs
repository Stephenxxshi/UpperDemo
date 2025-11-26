using System.ComponentModel;

namespace Plant01.Core.Extensions
{
    /// <summary>
    /// 枚举 扩展
    /// </summary>
    public static class EunmExtension
    {
        #region 获取枚举变量值的 Description 属性
        /// <summary>
        /// 获取枚举变量值的 Description 属性
        /// </summary>
        /// <param name="obj"></param>        
        /// <returns></returns>
        public static string GetDescription(this Enum obj)
        {
            var enumType = obj.GetType();
            var field = enumType.GetField(Enum.GetName(enumType, obj));
            var dna = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            if ((dna != null) && !string.IsNullOrWhiteSpace(dna.Description))
            {
                return dna.Description;
            }
            return obj.ToString();
        }
        #endregion
    }
}
