using CsvHelper;
using CsvHelper.Configuration;

using System.Globalization;
using System.Text.Json;

namespace Plant01.Upper.Infrastructure.Configs.Parsers;

public interface IConfigParser
{
    bool CanParse(string filePath);
    List<T> Parse<T>(string filePath);
}

public class JsonConfigParser : IConfigParser
{
    public bool CanParse(string filePath) => Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase);

    public List<T> Parse<T>(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        try
        {
            // 尝试解析为列表
            return JsonSerializer.Deserialize<List<T>>(json, options) ?? new List<T>();
        }
        catch (JsonException)
        {
            // 尝试解析为单个对象
            var item = JsonSerializer.Deserialize<T>(json, options);
            return item != null ? new List<T> { item } : new List<T>();
        }
    }
}

public class CsvConfigParser : IConfigParser
{
    public bool CanParse(string filePath) => Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public List<T> Parse<T>(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.ToLower(),
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }
}

public class ConfigParserFactory
{
    private readonly IEnumerable<IConfigParser> _parsers;

    public ConfigParserFactory()
    {
        _parsers = new List<IConfigParser>
        {
            new JsonConfigParser(),
            new CsvConfigParser()
            // 未来支持: new TomlConfigParser()
        };
    }

    public IConfigParser GetParser(string filePath)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(filePath));
        if (parser == null)
        {
            throw new NotSupportedException($"未找到适用的解析器: {filePath}");
        }
        return parser;
    }
}
