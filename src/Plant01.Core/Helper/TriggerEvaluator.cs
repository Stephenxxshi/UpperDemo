using System.Globalization;

namespace Plant01.Core.Helper;

/// <summary>
/// 触发条件评估器
/// </summary>
public static class TriggerEvaluator
{
    /// <summary>
    /// 评估值是否满足条件
    /// </summary>
    /// <param name="value">当前值</param>
    /// <param name="condition">条件表达式 (e.g. "> 0", "== true", "= 100")</param>
    /// <returns>是否满足</returns>
    public static bool Evaluate(object? value, string? condition)
    {
        // 1. 空条件：检查 "Truthiness" (非零/非空/True)
        if (string.IsNullOrWhiteSpace(condition))
        {
            return IsTruthy(value);
        }

        condition = condition.Trim();

        // 2. 解析操作符
        string op = "==";
        string targetStr = condition;

        if (condition.StartsWith(">=")) { op = ">="; targetStr = condition[2..]; }
        else if (condition.StartsWith("<=")) { op = "<="; targetStr = condition[2..]; }
        else if (condition.StartsWith("!=")) { op = "!="; targetStr = condition[2..]; }
        else if (condition.StartsWith("==")) { op = "=="; targetStr = condition[2..]; }
        else if (condition.StartsWith(">")) { op = ">"; targetStr = condition[1..]; }
        else if (condition.StartsWith("<")) { op = "<"; targetStr = condition[1..]; }
        else if (condition.StartsWith("=")) { op = "=="; targetStr = condition[1..]; }

        targetStr = targetStr.Trim();

        // 3. 解析目标值
        // TODO: 支持 [TagName] 引用
        object? targetValue = ParseValue(targetStr);

        // 4. 比较
        return Compare(value, op, targetValue);
    }

    /// <summary>
    /// 判断值的真值性 (Truthiness)
    /// </summary>
    public static bool IsTruthy(object? value)
    {
        if (value == null) return false;

        if (value is bool bVal) return bVal;

        if (IsNumeric(value, out double dVal))
        {
            return Math.Abs(dVal) > double.Epsilon; // 非零即真
        }

        if (value is string sVal)
        {
            return !string.IsNullOrEmpty(sVal) && !string.Equals(sVal, "false", StringComparison.OrdinalIgnoreCase) && sVal != "0";
        }

        return true; // 非空对象视为真
    }

    private static bool Compare(object? left, string op, object? right)
    {
        // 处理 null
        if (left == null || right == null)
        {
            bool areEqual = left == right;
            return op switch
            {
                "==" => areEqual,
                "!=" => !areEqual,
                _ => false // null 不能参与大小比较
            };
        }

        // 尝试数值比较
        if (IsNumeric(left, out double dLeft) && IsNumeric(right, out double dRight))
        {
            return op switch
            {
                "==" => Math.Abs(dLeft - dRight) < 0.0001,
                "!=" => Math.Abs(dLeft - dRight) >= 0.0001,
                ">" => dLeft > dRight,
                ">=" => dLeft >= dRight,
                "<" => dLeft < dRight,
                "<=" => dLeft <= dRight,
                _ => false
            };
        }

        // 字符串/布尔比较 (仅支持 == 和 !=)
        // 尝试统一转为字符串比较
        string sLeft = left.ToString() ?? "";
        string sRight = right.ToString() ?? "";
        
        int compareResult = string.Compare(sLeft, sRight, StringComparison.OrdinalIgnoreCase);

        return op switch
        {
            "==" => compareResult == 0,
            "!=" => compareResult != 0,
            _ => false // 非数值不支持大小比较
        };
    }

    private static bool IsNumeric(object? value, out double result)
    {
        result = 0;
        if (value == null) return false;
        if (value is bool) return false; // bool 不是数字

        return double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static object? ParseValue(string str)
    {
        if (string.Equals(str, "true", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(str, "false", StringComparison.OrdinalIgnoreCase)) return false;
        if (string.Equals(str, "null", StringComparison.OrdinalIgnoreCase)) return null;
        
        if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double dVal)) return dVal;

        // 去除引号
        if ((str.StartsWith("\"") && str.EndsWith("\"")) || (str.StartsWith("'") && str.EndsWith("'")))
        {
            return str[1..^1];
        }

        return str;
    }
}
