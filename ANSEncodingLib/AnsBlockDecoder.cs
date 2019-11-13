using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using EncodingUtilities;

namespace ANSEncodingLib
{
    public class AnsBlockDecoder
    {
        public Stream Output;
        private byte[] EncryptionKey;

        public AnsBlockDecoder(Stream output, byte[] encryptionKey = null)
        {
            Output = output;
            EncryptionKey = encryptionKey;
        }

        public void DecodeStream(Stream streamIn)
        {
            BitReader reader = new BitReader(streamIn);

            int numBlocks = reader.ReadInt(24);
            for (int i = 0; i < numBlocks; i++)
                DecodeBlock(reader);


        }

        private void DecodeBlock(BitReader readerIn)
        {
            int numCodedSymbols = readerIn.ReadInt(23);
            //Change to use stack only if the output stream is non-seekable
            //If it is seekable, then simply write the number of bytes that will be put into the stream
            //Then seek backward, writing them one by one
            Stack<int> symbolsStack = new Stack<int>();
            AnsCodingTable decodeTable = AnsCodingTable.ReadTable(readerIn, EncryptionKey);
            int bitCapacity = BitUtilities.GetNumberBitsToRepresent(decodeTable.Denominator);
            AnsState internalsState = new AnsState(bitCapacity, null, 0);
            internalsState.Underlying = readerIn.ReadUint(bitCapacity);
            KeyValuePair<int, int> symbolRow;
            for (int i = 0; i < numCodedSymbols; i++)
            {
                symbolRow = decodeTable.DecodePoint(internalsState.ToInt());
                symbolsStack.Push(symbolRow.Key);
                internalsState.Underlying = (uint)symbolRow.Value;
                int bitsToRead = bitCapacity - internalsState.Contained;
                for (int j = 0; j < bitsToRead; j++)
                    internalsState.PushLower(readerIn.ReadBit());
            }

            //Decoding finished...
            BitWriter symbolWriter = new BitWriter(Output);
            byte numBitsToWrite;
            if (decodeTable.SymbolType == typeof(byte))
                numBitsToWrite = 8;
            else if (decodeTable.SymbolType == typeof(short))
                numBitsToWrite = 16;
            else
                numBitsToWrite = 32;
            while (symbolsStack.Count != 0)
            {
                int symbol = symbolsStack.Pop();
                symbolWriter.WriteLong(symbol, numBitsToWrite);
            }
        }

        public void DecodeBlock(BitReader readerIn, Stream streamOut)
        {
            Stream save = Output;
            Output = streamOut;
            DecodeBlock(readerIn);
            Output = save;
        }
    }
}
