using Microsoft.Extensions.Logging;
using Plant01.Upper.Infrastructure.Configs.Parsers;

namespace Plant01.Upper.Infrastructure.Services;

public class MultiFormatConfigLoader
{
    private readonly ConfigParserFactory _parserFactory;
    private readonly ILogger<MultiFormatConfigLoader> _logger;

    public MultiFormatConfigLoader(ILogger<MultiFormatConfigLoader> logger)
    {
        _parserFactory = new ConfigParserFactory();
        _logger = logger;
    }

    public List<T> LoadFromDirectory<T>(string directoryPath, string searchPattern = "*.*")
    {
        var results = new List<T>();
        if (!Directory.Exists(directoryPath))
        {
            _logger.LogWarning($"[ 多格式配置加载 ] 未找到目录: {directoryPath}");
            return results;
        }

        var files = Directory.GetFiles(directoryPath, searchPattern)
            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            try
            {
                var parser = _parserFactory.GetParser(file);
                var items = parser.Parse<T>(file);
                results.AddRange(items);
                _logger.LogInformation($"[ 多格式配置加载 ] 已从 {Path.GetFileName(file)} 读取 {items.Count} 条数据");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ 多格式配置加载 ] 解析配置文件失败: {file}");
            }
        }

        return results;
    }
    
    public List<T> LoadFromFile<T>(string filePath)
    {
         if (!File.Exists(filePath))
        {
            _logger.LogWarning($"[ 多格式配置加载 ]未找到文件: {filePath}");
            return new List<T>();
        }
        
        try
        {
            var parser = _parserFactory.GetParser(filePath);
            return parser.Parse<T>(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"解析配置文件失败: {filePath}");
            return new List<T>();
        }
    }
}
