using System;
using System.IO;
using System.Collections.Generic;

namespace EncodingUtilities
{
    public class BitWriter
    {
        private byte Buffer = 0;
        private int CurrentPosition = 7;
        public Stream Output { get; private set; }
        private bool HasWrittenSinceFlush = false;
        public Action Flush { get; private set; }


        public BitWriter(Stream outputStream, bool reversed = false)
        {
            Output = outputStream;
            if (reversed)
                Flush = FlushReversed;
            else
                Flush = FlushNormal;
        }

        public void WriteBit(bool bitIn)
        {
            HasWrittenSinceFlush = true;
            if(bitIn)
                Buffer = (byte)(Buffer | (1u << CurrentPosition));
            CurrentPosition--;
            if(CurrentPosition==-1)
                Flush();
        }

        public void WriteBits(List<bool> bitsIn)
        {
            foreach (bool b in bitsIn)
                WriteBit(b);
        }

        public void WriteByte(byte byteIn)
        {
            if(CurrentPosition == 7)
            {
                Output.WriteByte(byteIn);
                return;
            }
            for(int i = 7; i >= 0; i--)
                WriteBit((byteIn & (1u << i)) != 0);
        }

        public void WriteLong(long longIn, byte startingPosition)
        {
            for (int i = startingPosition - 1; i >= 0; i--)
            {
                uint mask = 1u << i;
                WriteBit((longIn & mask) != 0);
            }
        }

        public void FlushNormal()
        {
            if (HasWrittenSinceFlush)
            {
                Output.WriteByte(Buffer);
                Buffer = 0;
                CurrentPosition = 7;
                HasWrittenSinceFlush = false;
                Output.Flush();
            }
        }

        public void FlushReversed()
        {
            if(HasWrittenSinceFlush)
            {
                Stack<bool> bitStack = new Stack<bool>();
                for (int i = 7; i >= 0; i--)
                    bitStack.Push( (Buffer & (1u << i)) != 0 );
                Buffer = 0;
                for(int i = 0; i < 8; i++)
                    if (bitStack.Pop())
                        Buffer = (byte)(Buffer | (1u << i));
            }
        }

        public int GetBitsToFill()
        {
            return CurrentPosition;
        }

        public void Close()
        {
            Flush();
            Output.Close();
        }

    }
}
