using Plant01.Upper.Application.Models;
using Plant01.Upper.Domain.Entities;

using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Plant01.Upper.Application.Services;

public static partial class TagValueTransformEvaluator
{
    private static readonly ConcurrentDictionary<string, byte> LoggedErrors = new(StringComparer.OrdinalIgnoreCase);

    public static object? EvaluateOrFallback(TagMappingDto mapping, object? sourceValue, ILogger logger)
    {
        return EvaluateOrFallbackInternal(mapping.TagCode, mapping.ValueTransformExpression, mapping.DataType, sourceValue, logger);
    }

    public static object? EvaluateOrFallback(EquipmentTagMapping mapping, object? sourceValue, ILogger logger)
    {
        return EvaluateOrFallbackInternal(mapping.TagCode, mapping.ValueTransformExpression, mapping.DataType, sourceValue, logger);
    }

    private static object? EvaluateOrFallbackInternal(
        string tagCode,
        string? expression,
        FinalDataType dataType,
        object? sourceValue,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return sourceValue;
        }

        try
        {
            var evaluated = EvaluateCore(expression, sourceValue);
            return ConvertToTargetType(evaluated, dataType);
        }
        catch (Exception ex)
        {
            var key = $"{tagCode}|{expression}";
            if (LoggedErrors.TryAdd(key, 0))
            {
                logger.LogWarning(ex,
                    "[ 标签转换 ] 标签 {TagCode} 的 ValueTransformExpression 执行失败，已回退原始值。表达式: {Expression}",
                    tagCode,
                    expression);
            }

            return sourceValue;
        }
    }

    private static object? EvaluateCore(string expression, object? sourceValue)
    {
        if (sourceValue == null)
        {
            return null;
        }

        var exp = expression.Trim();
        if (exp.StartsWith("=", StringComparison.Ordinal))
        {
            exp = $"value {exp}";
        }

        exp = VariableRegex().Replace(exp, _ => ToLiteral(sourceValue));

        var table = new DataTable
        {
            Locale = CultureInfo.InvariantCulture
        };

        var result = table.Compute(exp, string.Empty);
        return result is DBNull ? null : result;
    }

    private static object? ConvertToTargetType(object? value, FinalDataType dataType)
    {
        if (value == null)
        {
            return null;
        }

        return dataType switch
        {
            FinalDataType.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
            FinalDataType.Byte => Convert.ToByte(value, CultureInfo.InvariantCulture),
            FinalDataType.Int16 => Convert.ToInt16(value, CultureInfo.InvariantCulture),
            FinalDataType.UInt16 => Convert.ToUInt16(value, CultureInfo.InvariantCulture),
            FinalDataType.Int32 => Convert.ToInt32(value, CultureInfo.InvariantCulture),
            FinalDataType.UInt32 => Convert.ToUInt32(value, CultureInfo.InvariantCulture),
            FinalDataType.Int64 => Convert.ToInt64(value, CultureInfo.InvariantCulture),
            FinalDataType.UInt64 => Convert.ToUInt64(value, CultureInfo.InvariantCulture),
            FinalDataType.Float => Convert.ToSingle(value, CultureInfo.InvariantCulture),
            FinalDataType.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture),
            FinalDataType.String => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            FinalDataType.DateTime => value is DateTime dt
                ? dt
                : DateTime.Parse(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty, CultureInfo.InvariantCulture),
            _ => value
        };
    }

    private static string ToLiteral(object value)
    {
        return value switch
        {
            bool b => b ? "1" : "0",
            string s => $"'{s.Replace("'", "''")}'",
            DateTime dt => dt.Ticks.ToString(CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => $"'{value.ToString()?.Replace("'", "''") ?? string.Empty}'"
        };
    }

    [GeneratedRegex(@"\b(value|x|source)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex VariableRegex();
}
