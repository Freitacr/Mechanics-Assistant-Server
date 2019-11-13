using System.Collections.Generic;
using System.IO;
using System;
using EncodingUtilities;

namespace ANSEncodingLib
{
    public class AnsBlockEncoder
    {
        private int[] Block;
        private BitWriter Output;
        private int MaskBits;

        public AnsBlockEncoder(int blockSize, BitWriter bitWriterOut)
        {
            if (blockSize >= 4194304)
                throw new ArgumentOutOfRangeException("blockSize");
            Block = new int[blockSize];
            Output = bitWriterOut;
        }

        public void EncodeStream(Stream streamIn, byte numberBitsPerSymbol, int targetDenominator = -1)
        {
            BitReader readerIn = new BitReader(streamIn);
            long pos = Output.Output.Position;
            Output.WriteByte(0);
            Output.WriteByte(0);
            Output.WriteByte(0);
            int testByte = readerIn.ReadInt(numberBitsPerSymbol);
            if (readerIn.EOF)
                throw new EndOfStreamException("Stream ended before numberBitsPerSymbol bits were read. This is not worth encoding.");
            int numBlocks = 0;
            while(!readerIn.EOF)
            {
                int numReadBytes = readerIn.ReadInts(Block, 1, Block.Length - 1, numberBitsPerSymbol);
                Block[0] = testByte;
                testByte = readerIn.ReadInt(numberBitsPerSymbol);
                if (readerIn.EOF && readerIn.WereBitsReadOnEOF)
                {
                    int[] newBlock = new int[Block.Length+1];
                    Block.CopyTo(newBlock, 0);
                    newBlock[Block.Length] = testByte;
                    EncodeBlock(numReadBytes + 2, targetDenominator);
                }
                else
                {
                    EncodeBlock(numReadBytes + 1, targetDenominator);
                }
                numBlocks++;
            }
            Output.Flush();
            Output.Output.Seek(pos, SeekOrigin.Begin);
            Output.WriteLong(numBlocks, 24);
        }

        private void EncodeBlock(int blockContentLength, int targetDenominator)
        {
            Output.WriteLong(blockContentLength, 23);
            
            FrequencyDictionary<int> frequencies = new FrequencyDictionary<int>();
            for(int i = 0; i < blockContentLength; i++)
                frequencies.AddData(Block[i]);
            AnsCodingTable blockTable = new AnsCodingTable(frequencies, targetDenominator);
            blockTable.WriteTable(Output);
            WriteEncoding(blockContentLength, blockTable);
        }

        public void EncodeBlock(int[] blockIn, int count, int targetDenominator, byte[] encryptionKey = null)
        {
            Output.WriteLong(count, 23);
            Block = blockIn;
            FrequencyDictionary<int> frequencies = new FrequencyDictionary<int>();
            int offsetCount = count;
            for (int i = 0; i < offsetCount; i++)
                frequencies.AddData(Block[i]);
            AnsCodingTable blockTable = new AnsCodingTable(frequencies, targetDenominator, encryptionKey);
            blockTable.WriteTable(Output);
            WriteEncoding(count, blockTable);
        }

        private void WriteEncoding(int blockContentLength, AnsCodingTable blockTable)
        {
            ReversedMemoryStream reversedStream = new ReversedMemoryStream();
            uint initialState = ConstructMask(blockTable.Denominator);
            AnsState stateIn = new AnsState(MaskBits, reversedStream, initialState);
            for (int i = 0; i < blockContentLength; i++)
            {
                int row = stateIn.ToInt();
                int symbol = Block[i];
                var validRows = blockTable[symbol];
                while (!validRows.ContainsKey(row))
                {
                    stateIn.PopLower();
                    row = stateIn.ToInt();
                }
                stateIn.Underlying = (uint)validRows[row];
            }
            stateIn.Flush();
            reversedStream.ReadToStream(Output);
        }

        private uint ConstructMask(int denominator)
        {
            MaskBits = BitUtilities.GetNumberBitsToRepresent(denominator);
            uint retMask = 0;
            for(int i = 0; i < MaskBits; i++)
            {
                retMask |= 1;
                retMask <<= 1;
            }
            return retMask >> 1;
        }
    }
}
