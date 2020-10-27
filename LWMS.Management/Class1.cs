using CLUNL;
using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LWMS.Management
{
    /// <summary>
    /// Abstract interface of IManageCommand will be used in ServerController.
    /// </summary>
    public interface IManageCommand
    {
        string CommandName { get; }
        List<string> Alias { get; }
        int Version { get; }
        void Invoke(params CommandPack[] args);
    }
    public static class Output
    {
        public static IBaseWR CoreStream;
        static Output() {

        }
        public static void WriteLine(string str)
        {
        }
        public static void Write(string str)
        {

        }
    }
    public class PipedRoutedWR : IBaseWR
    {
        public long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public long Length { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool AutoFlush { get; set; }

        long IBaseReader.Length => throw new NotSupportedException();

        public void Dispose()
        {
            throw new NotSupportedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public Task FlushAsync()
        {
            throw new NotImplementedException();
        }

        public byte[] Read(int length, int offset)
        {
            throw new NotImplementedException();
        }

        public char ReadChar()
        {
            throw new NotImplementedException();
        }

        public string ReadLine()
        {
            throw new NotImplementedException();
        }

        public void SetLength(long Length)
        {
            throw new NotImplementedException();
        }

        public void Write(string Str)
        {
            throw new NotImplementedException();
        }

        public void Write(char c)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(char c)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string Str)
        {
            throw new NotImplementedException();
        }

        public void WriteBytes(byte[] b, int length, int offset)
        {
            throw new NotImplementedException();
        }

        public Task WriteBytesAsync(byte[] b, int length, int offset)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(string Str)
        {
            throw new NotImplementedException();
        }

        public Task WriteLineAsync(string str)
        {
            throw new NotImplementedException();
        }
    }
    public enum PipedRoutedWROperation
    {
        WRITE,WRITEBYTES,WRITELINE,READ,READBYTES,READLINE,FLUSH,DISPOSE
    }
    /// <summary>
    /// Command Pack. Contains result resolved from a command line or others. e.g.: -Option=1 => PackTotal = "-Option=1", PackParted= {"Option","1"}
    /// </summary>
    public class CommandPack
    {
        public string PackTotal;
        public List<string> PackParted = new List<string>();
        public static CommandPack FromRegexMatch(Match m)
        {
            CommandPack cp = new CommandPack();
            cp.PackTotal = m.Value.Trim();
            string _ = null;
            for (int i = 1; i < m.Groups.Count; i++)
            {
                if ((_ = m.Groups[i].Value.Trim()) != "")
                {
                    cp.PackParted.Add(_);
                }

            }
            return cp;
        }
        public static implicit operator string(CommandPack p)
        {
            return p.PackTotal;
        }
        public static implicit operator CommandPack(Match m)
        {
            return FromRegexMatch(m);
        }
        public string ToUpper()
        {
            return PackTotal.ToUpper();
        }
    }
}
