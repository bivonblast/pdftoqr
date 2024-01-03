using System.Text;

// ReSharper disable once CheckNamespace (should be globally available)
namespace System;

#if DEBUG
public static class DbgDump
{
    public static string Dump(this byte[] byteArray) => BitConverter.ToString(byteArray);

    public static string DumpPretty(this byte[] byteArray, int bytesPerLine = 32)
    {
        var sb = new StringBuilder();
        for (int line = 0; line < byteArray.Length; line += bytesPerLine)
        {
            byte[] lineBytes = byteArray.Skip(line).Take(bytesPerLine).ToArray();
            sb.Append($"{line:x8} ")
                .Append(string.Join(" ", lineBytes.Select(b => b.ToString("x2")).ToArray()).PadRight(bytesPerLine * 3))
                .Append(' ')
                .AppendLine(new string(lineBytes.Select(b => b < 32 ? '.' : (char) b).ToArray()));
        }

        return sb.ToString();
    }
}

#endif