#if !NETSTANDARD1_0
namespace Simple.BotUtils.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Fetch the end of the log file, and return last X lines
/// </summary>
public class LogTail
{
    private const int ChunkSize = 8192;

    /// <summary>
    /// Returns the last <paramref name="count"/> lines of a file, oldest first.
    /// Reads backwards in blocks, so it never loads more than the requested tail.
    /// </summary>
    public static string[] LastLines(string fileName, int count)
    {
        if (count <= 0) return [];

        using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        long length = fs.Length;
        if (length == 0) return [];

        // Read backwards, one block at a time, until we have gathered enough line
        // breaks to cover `count` lines (+1 tolerates a trailing newline) or hit BOF.
        var blocks = new List<byte[]>();
        long position = length;
        int newlines = 0;
        int total = 0;

        while (position > 0 && newlines <= count)
        {
            int size = (int)Math.Min(ChunkSize, position);
            position -= size;

            var block = new byte[size];
            fs.Position = position;
            readFully(fs, block, size);

            newlines += count1(block, (byte)'\n');
            total += size;
            blocks.Add(block);
        }

        // Stitch the blocks (collected end-first) back into forward order.
        var data = new byte[total];
        int offset = total;
        foreach (var block in blocks)
        {
            offset -= block.Length;
            Buffer.BlockCopy(block, 0, data, offset, block.Length);
        }

        return extractTail(data, total, count);
    }

    private static string[] extractTail(byte[] data, int length, int count)
    {
        // Ignore a single trailing line terminator so a file ending in "\n" does
        // not yield a phantom empty last line.
        if (length > 0 && data[length - 1] == (byte)'\n') length--;
        if (length > 0 && data[length - 1] == (byte)'\r') length--;

        var result = new string[count];
        int found = 0;

        var span = data.AsSpan(0, length);
        while (found < count)
        {
            int nl = span.LastIndexOf((byte)'\n');
            int start = nl + 1;

            int len = span.Length - start;
            if (len > 0 && span[span.Length - 1] == (byte)'\r') len--; // strip CR of a CRLF pair

            result[count - 1 - found] = Encoding.UTF8.GetString(data, start, len);
            found++;

            if (nl < 0) break; // consumed the first line, nothing before it
            span = span.Slice(0, nl);
        }

        if (found == count) return result;

        // File had fewer lines than requested: return only what was filled.
        var trimmed = new string[found];
        Array.Copy(result, count - found, trimmed, 0, found);
        return trimmed;
    }

    private static int count1(ReadOnlySpan<byte> data, byte value)
    {
        int total = 0;
        int idx;
        while ((idx = data.IndexOf(value)) >= 0)
        {
            total++;
            data = data.Slice(idx + 1);
        }
        return total;
    }

    private static void readFully(Stream stream, byte[] buffer, int count)
    {
        int read = 0;
        while (read < count)
        {
            int n = stream.Read(buffer, read, count - read);
            if (n == 0) throw new EndOfStreamException();
            read += n;
        }
    }
}

#endif
