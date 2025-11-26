namespace Plant01.Core.Helper
{
    public static class EnumHelper
    {
        /// <summary>
        /// 将整数安全转换为枚举类型，不合法则返回默认值
        /// </summary>
        public static TEnum ToEnum<TEnum>(this int value, TEnum defaultValue = default)
            where TEnum : struct, Enum
        {
            return Enum.IsDefined(typeof(TEnum), value)
                ? (TEnum)(object)value
                : defaultValue;
        }

        
    }

}
