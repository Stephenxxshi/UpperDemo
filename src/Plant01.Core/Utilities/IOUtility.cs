using System.Text;

namespace Plant01.Core.Utilities
{
    public static class IOUtility
    {
        #region 与基目录组成新的路径
        /// <summary>
        /// 与基目录组成新的路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static string CombineWithBaseDirectory(string path)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
        #endregion

        #region 目录创建
        /// <summary>
        /// 目录创建
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static void CreateDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                return;
            }
            Directory.CreateDirectory(directoryPath);
        }
        #endregion

        #region 目录下的文件数
        /// <summary>
        /// 目录下的文件数
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <param name="searchPattern">文件后缀</param>
        /// <returns>无效目录返回-1</returns>
        public static int GetFilesCount(string directoryPath, string searchPattern = "*")
        {
            CreateDirectory(directoryPath);
            return Directory.EnumerateFiles(directoryPath, searchPattern).Count();
        }
        #endregion

        #region 写入文件内容
        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">内容</param>
        /// <returns></returns>
        public static async Task WriteAsync(string filePath, string content)
        {
            CreateDirectory(Path.GetDirectoryName(filePath));
            await File.WriteAllTextAsync(filePath, content);
        }
        #endregion

        #region 读取文件内容
        /// <summary>
        /// 读取每行的内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<string> ReadAllTextAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }
        #endregion

        #region 读取每行的内容
        /// <summary>
        /// 读取每行的内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<string[]> ReadLineAsync(string filePath)
        {
            List<string> contents = [];
            using (FileStream fileStream = new(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(fileStream, Encoding.UTF8);
                string? content = await reader.ReadLineAsync();
                while (!string.IsNullOrWhiteSpace(content))
                {
                    contents.Add(content);
                    content = await reader.ReadLineAsync();
                }
            }
            return [.. contents];
        }
        #endregion

        #region 文件是否存在
        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        #endregion
    }
}