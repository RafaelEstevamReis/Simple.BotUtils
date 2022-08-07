#if !NETSTANDARD1_0
using System;
using System.IO;
using System.Text;

namespace Simple.BotUtils.Data
{
    /// <summary>
    /// Fetch the end of the log file, and return last X lines
    /// </summary>
    public class LogTail
    {
        public static string[] LastLines(string fileName, int count)
        {
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (fs.Length == 0) return null;

            string[] arr = new string[count];

            // skip new line
            fs.Position = fs.Length;

            int line;
            for (line = count - 1; line >= 0; line--)
            {
                long end = fs.Position;
                while (fs.Position > 0) // not EOF
                {
                    fs.Position--;
                    var b = fs.ReadByte();
                    if (b == '\n') // keep \n consumed
                    {
                        break;
                    }
                    fs.Position--; // undoReadByte
                }
                long start = fs.Position;

                int lineLen = (int)(end - start);
                byte[] bytes = new BinaryReader(fs).ReadBytes(lineLen);
                if (bytes.Length > 0 && bytes[bytes.Length - 1] == '\r')
                {
                    lineLen--;
                }
                arr[line] = Encoding.UTF8.GetString(bytes, 0, lineLen);

                fs.Position = start; // going backwards
                // skip \n
                if (fs.Position > 0) fs.Position--;

                if (fs.Position == 0) break;
            }

            if (line > 0)
            {
                int diff = count - line;
                if (diff <= 0) return new string[0];

                var arrTrim = new string[diff];
                Array.Copy(arr, line, arrTrim, 0, diff);
                return arrTrim;
            }

            return arr;
        }

    }
}
#endif