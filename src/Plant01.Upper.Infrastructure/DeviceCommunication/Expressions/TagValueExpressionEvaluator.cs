using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Expressions;

internal static partial class TagValueExpressionEvaluator
{
    private static readonly ConcurrentDictionary<string, byte> LoggedErrors = new(StringComparer.OrdinalIgnoreCase);

    public static object? EvaluateOrFallback(
        CommunicationTag tag,
        object? sourceValue,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(tag.ValueTransformExpression))
        {
            return sourceValue;
        }

        try
        {
            var evaluated = EvaluateCore(tag.ValueTransformExpression!, sourceValue);
            return ConvertToTagDataType(evaluated, tag.DataType, tag.ArrayLength);
        }
        catch (Exception ex)
        {
            var key = $"{tag.Code}|{tag.ValueTransformExpression}";
            if (LoggedErrors.TryAdd(key, 0))
            {
                logger.LogWarning(ex,
                    "[ 设备通信服务 ] 标签 {TagCode} 的 ValueTransformExpression 执行失败，已回退原始值。表达式: {Expression}",
                    tag.Code,
                    tag.ValueTransformExpression);
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

        exp = VariableRegex().Replace(exp, match => ToLiteral(sourceValue));

        var table = new DataTable
        {
            Locale = CultureInfo.InvariantCulture
        };

        var result = table.Compute(exp, string.Empty);
        return result is DBNull ? null : result;
    }

    private static object? ConvertToTagDataType(object? value, TagDataType dataType, ushort arrayLength)
    {
        if (value == null)
        {
            return null;
        }

        if (arrayLength > 1 && value is not string)
        {
            return value;
        }

        return dataType switch
        {
            TagDataType.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
            TagDataType.Byte => Convert.ToByte(value, CultureInfo.InvariantCulture),
            TagDataType.Int16 => Convert.ToInt16(value, CultureInfo.InvariantCulture),
            TagDataType.UInt16 => Convert.ToUInt16(value, CultureInfo.InvariantCulture),
            TagDataType.Int32 => Convert.ToInt32(value, CultureInfo.InvariantCulture),
            TagDataType.UInt32 => Convert.ToUInt32(value, CultureInfo.InvariantCulture),
            TagDataType.Int64 => Convert.ToInt64(value, CultureInfo.InvariantCulture),
            TagDataType.UInt64 => Convert.ToUInt64(value, CultureInfo.InvariantCulture),
            TagDataType.Float => Convert.ToSingle(value, CultureInfo.InvariantCulture),
            TagDataType.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture),
            TagDataType.String => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            TagDataType.DateTime => value is DateTime dt
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
