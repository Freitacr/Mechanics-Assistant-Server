using System;
using System.IO;
using System.Collections.Generic;

namespace EncodingUtilities
{
    public class BitReader
    {
        public Action Close { get; private set; }
        public Stream Input { get; private set; }
        private byte CurrentBuffer = 0;
        private int CurrentPosition = -1;
        public bool EOF { get; private set; } = false;
        public bool WereBitsReadOnEOF = false;

        public BitReader(Stream inputStream)
        {
            Input = inputStream;
            Close = Input.Close;
        }

        private void ReplenishBuffer()
        {
            int byteIn = Input.ReadByte();
            if (byteIn == -1)
                throw new EndOfStreamException();
            CurrentBuffer = (byte)byteIn;
            CurrentPosition = 7;
        }

        public bool ReadBit()
        {
            if (CurrentPosition == -1)
            {
                ReplenishBuffer();
            }
            uint mask = 1u << CurrentPosition;
            uint val = CurrentBuffer & mask;
            CurrentPosition--;
            return val != 0;
        }

        public bool Peek()
        {
            bool ret = ReadBit();
            CurrentPosition++;
            return ret;
        }

        public List<bool> ReadBits(int count)
        {
            List<bool> ret = new List<bool>();
            for (int i = 0; i < count; i++)
            {
                try
                {
                    ret.Add(ReadBit());
                } catch (EndOfStreamException)
                {
                    EOF = true;
                    WereBitsReadOnEOF = i != 0;
                    break;
                }
            }
            return ret;
        }


        public byte ReadByte()
        {
            if (CurrentPosition == -1)
            {
                ReplenishBuffer();
                CurrentPosition = -1;
                return CurrentBuffer;
            }
            byte ret = 0;
            for (int i = 7; i >= 0; i--)
            {
                try
                {
                    if (ReadBit())
                        ret = (byte)(ret | (1u << i));
                } catch (EndOfStreamException)
                {
                    EOF = true;
                    WereBitsReadOnEOF = i != 7;
                    break;
                }
            }
            return ret;
        }

        public int ReadInt(int count)
        {
            int ret = 0;
            for (int i = count - 1; i >= 0; i--)
            {
                try
                {
                    if (ReadBit())
                        ret |= (1 << i);
                } catch (EndOfStreamException)
                {
                    EOF = true;
                    WereBitsReadOnEOF = i != count-1;
                    break;
                }
            }
            return ret;
        }

        public long ReadLong(int count)
        {
            long ret = 0;
            for (int i = count-1; i >=0; i--)
            {
                try
                {
                    if (ReadBit())
                        ret |= (1u << i);
                }
                catch (EndOfStreamException)
                {
                    EOF = true;
                    WereBitsReadOnEOF = i != count-1;
                    break;
                }
            }
            return ret;
        }

        public uint ReadUint(int count)
        {
            uint ret = 0;
            for (int i = count - 1; i >= 0; i--)
            {
                try
                {
                    if (ReadBit())
                        ret |= (1u << i);
                }
                catch (EndOfStreamException)
                {
                    EOF = true;
                    WereBitsReadOnEOF = i != count-1;
                    break;
                }
            }
            return ret;
        }

        public int ReadInts(int[] arrIn, int offset, int count, byte numberBitsPerInt)
        {
            int readInts = 0;
            int offsetCount = offset + count;
            for (int i = offset; i < offsetCount; i++)
            {
                arrIn[i] = ReadInt(numberBitsPerInt);
                if (EOF)
                {
                    if (WereBitsReadOnEOF)
                        readInts++;
                    return readInts;
                }
                readInts++;
            }
            return readInts;
        }
    }
}
