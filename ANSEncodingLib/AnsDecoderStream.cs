using System;
using System.Collections.Generic;
using System.IO;
using EncodingUtilities;

namespace ANSEncodingLib
{
    public class AnsDecoderStream : Stream
    {

        private BitReader Base;
        private AnsBlockDecoder Decoder;
        private byte[] Buffer;
        private int CurrentBufferPosition;
        private int NumberBlocks;
        private MemoryStream CurrMemoryStream;
        

        public AnsDecoderStream(Stream baseStream, byte[] encryptionKey = null)
        {
            Base = new BitReader(baseStream);
            Buffer = new byte[0];
            CurrentBufferPosition = 0;
            NumberBlocks = Base.ReadInt(24);
            Decoder = new AnsBlockDecoder(null, encryptionKey);
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int offsetCount = offset + count;
            int numRead = 0;
            for (int i = offset; i < offsetCount; i++)
            {
                int read = ReadByte();
                if (read == -1)
                    break;
                buffer[i] = (byte)read;
                numRead++;
            }
            return numRead;
        }

        public override int ReadByte()
        {
            if(Buffer.Length == CurrentBufferPosition)
            {
                if (NumberBlocks == 0)
                    return -1;
                DecodeBlock();
                CurrentBufferPosition = 0;
            }
            byte ret = Buffer[CurrentBufferPosition];
            CurrentBufferPosition++;
            return ret;
        }

        private void DecodeBlock()
        {
            CurrMemoryStream = new MemoryStream();
            Decoder.DecodeBlock(Base, CurrMemoryStream);
            Buffer = CurrMemoryStream.ToArray();
            NumberBlocks--;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
