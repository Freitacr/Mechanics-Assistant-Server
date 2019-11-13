using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using EncodingUtilities;

namespace ANSEncodingLib
{
    public class AnsEncoderStream : Stream
    {
        private readonly BitWriter Base;
        private readonly AnsBlockEncoder Encoder;
        private readonly long NumberBlocksStreamPosition;
        private int NumberBlocks;
        private readonly int[] Block;
        private int BlockPosition;
        private readonly int Denominator;
        private readonly byte[] EncryptionKey;

        public AnsEncoderStream(Stream baseStream, int blockSize, int targetDenominator, byte[] encryptionKey = null)
        {
            Base = new BitWriter(baseStream);
            Encoder = new AnsBlockEncoder(blockSize, Base);
            NumberBlocksStreamPosition = baseStream.Position;
            Base.WriteLong(0, 24);
            Block = new int[blockSize];
            NumberBlocks = 0;
            BlockPosition = 0;
            Denominator = targetDenominator;
            EncryptionKey = encryptionKey;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => -1;

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            if(BlockPosition != 0)
            {
                Encoder.EncodeBlock(Block, BlockPosition, Denominator, EncryptionKey);
                BlockPosition = 0;
                NumberBlocks++;
            }
            Base.Flush();
            Base.Output.Seek(NumberBlocksStreamPosition, SeekOrigin.Begin);
            Base.WriteLong(NumberBlocks, 24);
            Base.Output.Seek(0, SeekOrigin.End);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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
            int offsetCount = offset + count;
            for (int i = offset; i < offsetCount; i++)
                WriteByte(buffer[i]);
        }

        public override void WriteByte(byte value)
        {
            if(BlockPosition == Block.Length)
            {
                Encoder.EncodeBlock(Block, BlockPosition, Denominator, EncryptionKey);
                BlockPosition = 0;
                NumberBlocks++;
            }
            Block[BlockPosition] = value;
            BlockPosition++;
            //base.WriteByte(value);
        }
    }
}
