﻿using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using System;
using System.Threading.Tasks;

namespace LWMS.Core.WR
{
    /// <summary>
    /// This WR do not support async supports due to pipeline do not support work in async mode.
    /// </summary>
    public class PipedRoutedWR : IBaseWR
    {
        /// <summary>
        /// Should be initialized out side.
        /// </summary>
        public IPipelineProcessor Processor;
        public long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public long Length { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool AutoFlush { get; set; }


        public void Dispose()
        {
            Processor.Process(new PipelineData(null, null, new PipedRoutedWROption(PipedRoutedWROperation.DISPOSE, AutoFlush)));
        }

        public void Flush()
        {
            Processor.Process(new PipelineData(null, null, new PipedRoutedWROption(PipedRoutedWROperation.FLUSH, AutoFlush)));
        }

        public Task FlushAsync()
        {
            throw new NotSupportedException();
        }

        public byte[] Read(int length, int offset)
        {
            var result = Processor.Process(new PipelineData(new int[] { length, offset }, new byte[0],
                new PipedRoutedWROption(PipedRoutedWROperation.READBYTES, AutoFlush)));
            return result.SecondaryData as byte[];
        }

        public char ReadChar()
        {
            var result = Processor.Process(new PipelineData(null, char.MinValue,
                new PipedRoutedWROption(PipedRoutedWROperation.READ, AutoFlush)));
            return (char)result.SecondaryData;
        }

        public string ReadLine()
        {
            var result = Processor.Process(new PipelineData(null, string.Empty,
                new PipedRoutedWROption(PipedRoutedWROperation.READLINE, AutoFlush)));
            return (string)result.SecondaryData;
        }

        public void SetLength(long Length)
        {
            throw new NotSupportedException();
        }

        public void Write(string Str)
        {
            Processor.Process(new PipelineData(Str, null,
                  new PipedRoutedWROption(PipedRoutedWROperation.WRITE, AutoFlush)));
        }

        public void Write(char c)
        {
            Processor.Process(new PipelineData(c, null,
                  new PipedRoutedWROption(PipedRoutedWROperation.WRITECHAR, AutoFlush)));
        }

        public Task WriteAsync(char c)
        {
            throw new NotSupportedException();
        }

        public Task WriteAsync(string Str)
        {
            throw new NotSupportedException();
        }

        public void WriteBytes(byte[] b, int length, int offset)
        {
            Processor.Process(new PipelineData(b, new int[] { length, offset },
                  new PipedRoutedWROption(PipedRoutedWROperation.WRITEBYTES, AutoFlush)));
        }

        public Task WriteBytesAsync(byte[] b, int length, int offset)
        {
            throw new NotSupportedException();
        }

        public void WriteLine(string Str)
        {
            Processor.Process(new PipelineData(Str, null,
                  new PipedRoutedWROption(PipedRoutedWROperation.WRITELINE, AutoFlush)));
        }

        public Task WriteLineAsync(string str)
        {
            throw new NotSupportedException();
        }
    }
    public class PipedRoutedWROption
    {
        public PipedRoutedWROperation PipedRoutedWROperation;
        public bool isAutoFlush;
        public PipedRoutedWROption(PipedRoutedWROperation operation, bool AutoFlush)
        {
            isAutoFlush = AutoFlush;
            PipedRoutedWROperation = operation;
        }
    }
    public enum PipedRoutedWROperation
    {
        WRITE, WRITECHAR, WRITEBYTES, WRITELINE, READ, READBYTES, READLINE, FLUSH, DISPOSE
    }
}
