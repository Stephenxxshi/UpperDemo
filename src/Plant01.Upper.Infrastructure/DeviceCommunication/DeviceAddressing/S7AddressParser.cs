using System;
using System.Text.RegularExpressions;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;

public enum S7AddressKind
{
    DBX, // bit: DBx.DBXbyte.bit
    DBW, // word: DBx.DBWoffset
    DBD, // dword/float/double: DBx.DBDoffset
    DBS  // string: DBx.DBSoffset[len]
}

public readonly struct S7Address
{
    public int Db { get; }
    public S7AddressKind Kind { get; }
    public int Offset { get; } // byte offset
    public int Bit { get; } // for DBX
    public int StringLength { get; }

    public S7Address(int db, S7AddressKind kind, int offset, int bit = -1, int stringLength = 0)
    {
        Db = db;
        Kind = kind;
        Offset = offset;
        Bit = bit;
        StringLength = stringLength;
    }
}

public static class S7AddressParser
{
    private static readonly Regex DbxRegex = new(@"^DB(?<db>\d+)\.DBX(?<byte>\d+)\.(?<bit>\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DbwRegex = new(@"^DB(?<db>\d+)\.DBW(?<off>\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DbdRegex = new(@"^DB(?<db>\d+)\.DBD(?<off>\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DbsRegex = new(@"^DB(?<db>\d+)\.DBS(?<off>\d+)(?:\[(?<len>\d+)\])?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool TryParse(string address, out S7Address result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(address)) return false;

        var m = DbxRegex.Match(address);
        if (m.Success)
        {
            var db = int.Parse(m.Groups["db"].Value);
            var b = int.Parse(m.Groups["byte"].Value);
            var bit = int.Parse(m.Groups["bit"].Value);
            result = new S7Address(db, S7AddressKind.DBX, b, bit);
            return true;
        }

        m = DbwRegex.Match(address);
        if (m.Success)
        {
            var db = int.Parse(m.Groups["db"].Value);
            var off = int.Parse(m.Groups["off"].Value);
            result = new S7Address(db, S7AddressKind.DBW, off);
            return true;
        }

        m = DbdRegex.Match(address);
        if (m.Success)
        {
            var db = int.Parse(m.Groups["db"].Value);
            var off = int.Parse(m.Groups["off"].Value);
            result = new S7Address(db, S7AddressKind.DBD, off);
            return true;
        }

        m = DbsRegex.Match(address);
        if (m.Success)
        {
            var db = int.Parse(m.Groups["db"].Value);
            var off = int.Parse(m.Groups["off"].Value);
            var len = 0;
            if (m.Groups["len"].Success) len = int.Parse(m.Groups["len"].Value);
            result = new S7Address(db, S7AddressKind.DBS, off, -1, len);
            return true;
        }

        return false;
    }
}
