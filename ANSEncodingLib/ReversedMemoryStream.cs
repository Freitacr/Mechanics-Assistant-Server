using System;
using System.Collections.Generic;
using System.IO;
using EncodingUtilities;

namespace ANSEncodingLib
{
    public class ReversedMemoryStream : Stream
    {

        private LinkedList<byte> Buffer; //Using doubly linked list to speed adding at first position
        private BitStack BufferStack;
        public ReversedMemoryStream()
        {
            Buffer = new LinkedList<byte>();
            BufferStack = new BitStack();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => Buffer == null ? 0 : Buffer.Count;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            // a flush shouldn't do anything
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            LinkedListNode<byte> currNode = Buffer.First;
            int numBytesRead = 0;
            for(int i = 0; i < count; i++, numBytesRead++)
            {
                buffer[offset + i] = currNode.Value;
                currNode = currNode.Next;
                if (currNode == null)
                    break;
            }
            return numBytesRead;
        }

        public override int ReadByte()
        {
            if (Buffer.Count == 0)
                return -1;
            int ret = Buffer.First.Value;
            Buffer.RemoveFirst();
            return ret;
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
            byte val = value;
            for(int i = 0; i < 8; i++)
            {
                WriteBit(
                    (val & 128u) != 0
                );
                val <<= 1;
            }
        }
        public void WriteBit(bool bitIn)
        {
            BufferStack.PushBit(bitIn);
            if(BufferStack.ByteFull)
            {
                Buffer.AddFirst((byte)BufferStack.Underlying);
                BufferStack.Clear();
            }
        }

        public void ReadToStream(BitWriter writerOut)
        {
            if (!BufferStack.Empty)
                writerOut.WriteLong(BufferStack.Underlying, (byte)(BufferStack.CurrentPos));
            foreach (byte b in Buffer)
                writerOut.WriteByte(b);
            Buffer.Clear();
            BufferStack.Clear();
        }


        public byte[] ToArray()
        {
            byte[] ret = new byte[Buffer.Count];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = (byte)ReadByte();
            return ret;
        }

        public override void Close()
        {
            Buffer = null;
            BufferStack = null;
        }
    }
}
