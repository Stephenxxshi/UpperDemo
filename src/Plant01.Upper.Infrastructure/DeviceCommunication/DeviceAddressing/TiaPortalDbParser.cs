using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;

public class TiaPortalDbParser
{
    private class ParsingContext
    {
        public int DbNumber { get; set; }
        public int ByteOffset { get; set; }
        public int BitOffset { get; set; }

        public string GetCurrentAddress()
        {
            if (BitOffset > 0 || IsBoolProcessing)
            {
                return $"DB{DbNumber}.DBX{ByteOffset}.{BitOffset}";
            }
            return $"DB{DbNumber}.DBW{ByteOffset}"; // Default to Word/Byte based, but caller fixes prefix
        }

        public bool IsBoolProcessing { get; set; }
    }

    private class DbFieldDefinition
    {
        public string Name { get; set; } = "";
        public string RawType { get; set; } = "";
        public bool IsStruct { get; set; }
        public List<DbFieldDefinition> Children { get; set; } = new();
    }

    public List<ScannedTag> Parse(string content)
    {
        // 1. 提取 DB 号
        // 匹配 DATA_BLOCK "1001Name" 或 DATA_BLOCK "Name"
        var dbMatch = Regex.Match(content, @"DATA_BLOCK\s+""([^""]+)""");
        int dbNumber = 0;
        if (dbMatch.Success)
        {
            var dbName = dbMatch.Groups[1].Value;
            var numMatch = Regex.Match(dbName, @"^(\d+)");
            if (numMatch.Success)
            {
                dbNumber = int.Parse(numMatch.Groups[1].Value);
            }
        }

        // 2. 解析所有 TYPE 定义
        var types = new Dictionary<string, List<DbFieldDefinition>>();
        var typeMatches = Regex.Matches(content, @"TYPE\s+""?([^""\s]+)""?.*?STRUCT(.*?)END_STRUCT.*?(?:END_TYPE)", RegexOptions.Singleline);
        foreach (Match tm in typeMatches)
        {
            var typeName = tm.Groups[1].Value;
            var body = tm.Groups[2].Value;
            types[typeName] = ParseFields(body);
        }

        // 3. 解析 DATA_BLOCK 主体
        // 查找 DATA_BLOCK ... STRUCT ... END_STRUCT
        var dbBlockMatch = Regex.Match(content, @"DATA_BLOCK\s+.*?STRUCT(.*?)END_STRUCT", RegexOptions.Singleline);
        if (!dbBlockMatch.Success) return new List<ScannedTag>();

        var dbBody = dbBlockMatch.Groups[1].Value;
        var rootFields = ParseFields(dbBody);

        // 4. 扁平化并计算地址
        var tags = new List<ScannedTag>();
        var context = new ParsingContext { DbNumber = dbNumber };

        foreach (var field in rootFields)
        {
            FlattenField(field, "", context, types, tags);
        }

        return tags;
    }

    private List<DbFieldDefinition> ParseFields(string body)
    {
        var fields = new List<DbFieldDefinition>();
        var lines = body.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        var stack = new Stack<List<DbFieldDefinition>>();
        stack.Push(fields);
        
        var structStack = new Stack<DbFieldDefinition>();

        foreach (var line in lines)
        {
            var trim = line.Trim();
            if (string.IsNullOrWhiteSpace(trim)) continue;
            if (trim.StartsWith("//")) continue;
            
            // 去除行内注释
            var commentIndex = trim.IndexOf("//");
            if (commentIndex > 0) trim = trim.Substring(0, commentIndex).Trim();

            if (trim.StartsWith("VERSION")) continue;
            if (trim.StartsWith("BEGIN")) continue;

            // 结束结构体
            if (trim.StartsWith("END_STRUCT"))
            {
                if (structStack.Count > 0)
                {
                    structStack.Pop();
                    stack.Pop();
                }
                continue;
            }

            // 字段定义: "Name" : Type;
            // 匹配: "Name" : Type; 或 Name : Type;
            var match = Regex.Match(trim, @"^""?([^""\s:]+)""?\s*:\s*(.*);?");
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var typePart = match.Groups[2].Value.Trim().TrimEnd(';');

                // 去除属性 { ... }
                var braceIndex = typePart.IndexOf('{');
                if (braceIndex > 0) typePart = typePart.Substring(0, braceIndex).Trim();

                var field = new DbFieldDefinition { Name = name, RawType = typePart };
                stack.Peek().Add(field);

                if (typePart.Equals("Struct", StringComparison.OrdinalIgnoreCase))
                {
                    field.IsStruct = true;
                    structStack.Push(field);
                    stack.Push(field.Children);
                }
            }
        }
        return fields;
    }

    private void FlattenField(DbFieldDefinition field, string prefix, ParsingContext ctx, Dictionary<string, List<DbFieldDefinition>> types, List<ScannedTag> tags)
    {
        var fullName = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}.{field.Name}";
        var typeName = field.RawType.Trim('"');

        // 检查是否是自定义类型 (UDT)
        bool isCustomType = types.ContainsKey(typeName);

        if (field.IsStruct || isCustomType)
        {
            // 结构体开始前对齐 (Word对齐)
            AlignToWord(ctx);

            var children = field.IsStruct ? field.Children : types[typeName];
            foreach (var child in children)
            {
                FlattenField(child, fullName, ctx, types, tags);
            }
            
            // 结构体结束后对齐 (Word对齐)
            AlignToWord(ctx);
        }
        else
        {
            // 基本类型
            Align(ctx, typeName);
            
            var address = FormatAddress(ctx, typeName);
            
            tags.Add(new ScannedTag 
            { 
                TagName = fullName, 
                Address = address, 
                DataType = MapDataType(typeName),
                Length = 1 // 暂不处理数组长度
            });
            
            Advance(ctx, typeName);
        }
    }

    private void Align(ParsingContext ctx, string type)
    {
        type = type.ToLowerInvariant();
        if (type == "bool")
        {
            // Bool 不需要字节对齐，紧接着上一个
            ctx.IsBoolProcessing = true;
        }
        else
        {
            // 非 Bool 类型，如果有未满的字节，先进位
            if (ctx.BitOffset > 0)
            {
                ctx.BitOffset = 0;
                ctx.ByteOffset++;
            }
            ctx.IsBoolProcessing = false;

            // Word/Int 等需要偶数地址对齐
            if (IsWordAlignedType(type))
            {
                if (ctx.ByteOffset % 2 != 0) ctx.ByteOffset++;
            }
        }
    }

    private void AlignToWord(ParsingContext ctx)
    {
        if (ctx.BitOffset > 0)
        {
            ctx.BitOffset = 0;
            ctx.ByteOffset++;
        }
        if (ctx.ByteOffset % 2 != 0) ctx.ByteOffset++;
        ctx.IsBoolProcessing = false;
    }

    private void Advance(ParsingContext ctx, string type)
    {
        type = type.ToLowerInvariant();
        if (type == "bool")
        {
            ctx.BitOffset++;
            if (ctx.BitOffset > 7)
            {
                ctx.BitOffset = 0;
                ctx.ByteOffset++;
            }
        }
        else
        {
            int size = GetSize(type);
            ctx.ByteOffset += size;
        }
    }

    private string FormatAddress(ParsingContext ctx, string type)
    {
        type = type.ToLowerInvariant();
        if (type == "bool") return $"DB{ctx.DbNumber}.DBX{ctx.ByteOffset}.{ctx.BitOffset}";
        if (type == "byte" || type == "char") return $"DB{ctx.DbNumber}.DBB{ctx.ByteOffset}";
        if (type == "int" || type == "word" || type == "int16" || type == "uint16") return $"DB{ctx.DbNumber}.DBW{ctx.ByteOffset}";
        if (type == "dint" || type == "dword" || type == "real" || type == "float") return $"DB{ctx.DbNumber}.DBD{ctx.ByteOffset}";
        return $"DB{ctx.DbNumber}.DBW{ctx.ByteOffset}";
    }

    private bool IsWordAlignedType(string type)
    {
        // S7 标准访问模式下，除了 Byte/Char/Bool，其他基本都是偶数对齐
        type = type.ToLowerInvariant();
        return type != "byte" && type != "char" && type != "bool";
    }

    private int GetSize(string type)
    {
        type = type.ToLowerInvariant();
        return type switch
        {
            "bool" => 0, // Handled separately
            "byte" or "char" => 1,
            "int" or "word" or "int16" or "uint16" => 2,
            "dint" or "dword" or "real" or "float" or "time" => 4,
            "lreal" or "lword" or "lint" => 8,
            _ => 2
        };
    }

    private string MapDataType(string rawType)
    {
        var t = rawType.ToLowerInvariant();
        return t switch
        {
            "bool" => "Boolean",
            "int" => "Int16",
            "word" => "UInt16",
            "dint" => "Int32",
            "dword" => "UInt32",
            "real" => "Float",
            "byte" => "Byte",
            _ => "Int16" // Default
        };
    }
}
