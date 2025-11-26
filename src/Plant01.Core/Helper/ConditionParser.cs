using System.Globalization;
using System.Linq.Expressions;

namespace Plant01.Core.Helper;

public static class ConditionParser
{

    public static Func<T, bool> Parse<T>(string condition)
    {
        var expr = ParseToExpression<T>(condition);
        return expr.Compile();
    }

    public static Expression<Func<T, bool>> ParseToExpression<T>(string condition)
    {
        condition = condition.Trim();
        var parameter = Expression.Parameter(typeof(T), "x");

        if (typeof(T) == typeof(double) || typeof(T) == typeof(int))
            return (Expression<Func<T, bool>>)(object)ParseNumericCondition(condition);
        else if (typeof(T) == typeof(string))
            return (Expression<Func<T, bool>>)(object)ParseStringCondition(condition, parameter);
        else
            throw new NotSupportedException($"Type {typeof(T)} is not supported.");
    }

    // ===== 数值比较方法 =====
    public static Expression<Func<double, bool>> ParseNumericCondition(string condition)
    {
        var parameter = Expression.Parameter(typeof(double), "x");
        Expression body = condition switch
        {
            _ when condition.Contains("±") => ParseTolerance(condition, parameter),
            _ when condition.Contains("~") => ParseRange(condition, parameter),
            _ => ParseNumericComparison(condition, parameter)
        };
        return Expression.Lambda<Func<double, bool>>(body, parameter);
    }

    private static Expression ParseTolerance(string condition, ParameterExpression parameter)
    {
        var parts = condition.Split('±');
        double target = ParseDouble(parts[0]);
        double tolerance = ParseDouble(parts[1]);

        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(parameter, Expression.Constant(target - tolerance)),
            Expression.LessThanOrEqual(parameter, Expression.Constant(target + tolerance))
        );
    }

    private static Expression ParseRange(string condition, ParameterExpression parameter)
    {
        var parts = condition.Split('~');
        double min = ParseDouble(parts[0]);
        double max = ParseDouble(parts[1]);

        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(parameter, Expression.Constant(min)),
            Expression.LessThanOrEqual(parameter, Expression.Constant(max))
        );
    }

    private static Expression ParseNumericComparison(string condition, ParameterExpression parameter)
    {
        if (double.TryParse(condition, NumberStyles.Any, CultureInfo.InvariantCulture, out double exactValue))
            return Expression.Equal(parameter, Expression.Constant(exactValue));

        Func<Expression, Expression, BinaryExpression> op = condition[0] switch
        {
            '>' when condition.Length > 1 && condition[1] == '=' => Expression.GreaterThanOrEqual,
            '>' => Expression.GreaterThan,
            '<' when condition.Length > 1 && condition[1] == '=' => Expression.LessThanOrEqual,
            '<' => Expression.LessThan,
            '=' => Expression.Equal,
            _ => throw new FormatException($"Unsupported numeric condition: {condition}")
        };

        int skip = op == Expression.GreaterThanOrEqual || op == Expression.LessThanOrEqual ? 2 : 1;
        if (condition[0] == '=') skip = 1; // 显式支持 =12.34 格式
        double value = ParseDouble(condition.Substring(skip));
        return op(parameter, Expression.Constant(value));
    }


    // ===== 字符串比较方法 =====
    public static Expression<Func<string, bool>> ParseStringCondition(
        string condition, ParameterExpression parameter)
    {
        // 关键修复：添加显式类型转换
        Expression body = condition switch
        {
            _ when condition.Contains("==") => ParseStringEqual(condition, parameter),
            _ when condition.Contains("!=") => ParseStringNotEqual(condition, parameter),
            _ when condition.Contains("~=") => ParseStringContains(condition, parameter),
            _ when condition.Contains("length>=") => ParseStringLength(condition, parameter, Expression.GreaterThanOrEqual),
            _ when condition.Contains("length<=") => ParseStringLength(condition, parameter, Expression.LessThanOrEqual),
            _ => throw new FormatException($"Unsupported string condition: {condition}")
        };
        return Expression.Lambda<Func<string, bool>>(body, parameter);
    }

    private static Expression ParseStringEqual(string condition, ParameterExpression parameter)
    {
        var parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
        string value = parts[1].Trim().Trim('"', '\'');
        return Expression.Equal(
            parameter,
            Expression.Constant(value)
        );
    }

    private static Expression ParseStringNotEqual(string condition, ParameterExpression parameter)
    {
        var parts = condition.Split(new[] { "!=" }, StringSplitOptions.None);
        string value = parts[1].Trim().Trim('"', '\'');
        return Expression.NotEqual(
            parameter,
            Expression.Constant(value)
        );
    }

    private static Expression ParseStringContains(string condition, ParameterExpression parameter)
    {
        var parts = condition.Split(new[] { "~=" }, StringSplitOptions.None);
        string value = parts[1].Trim().Trim('"', '\'');
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        return Expression.Call(
            parameter,
            containsMethod,
            Expression.Constant(value)
        );
    }

    private static Expression ParseStringLength(
        string condition,
        ParameterExpression parameter,
        Func<Expression, Expression, BinaryExpression> comparer)
    {
        string[] separators = { "length>=", "length<=" };
        var parts = condition.Split(separators, StringSplitOptions.None);

        if (parts.Length < 2) throw new FormatException("Invalid length condition format");

        int length = int.Parse(parts[1]);
        var lengthProperty = typeof(string).GetProperty("Length");
        return comparer(
            Expression.Property(parameter, lengthProperty),
            Expression.Constant(length)
        );
    }

    // ===== 工具方法 =====
    private static double ParseDouble(string s)
    {
        return double.Parse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture);
    }
}