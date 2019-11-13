using System;
using System.Collections.Generic;
using System.Text;
using EncodingUtilities;

namespace ANSEncodingLib
{
    internal class CodingTableStreamer
    {
        private readonly int LargestSymbolBits;
        private readonly int LargestFrequencyBits;
        private readonly int NumFrequencies = 0;
        public Type SymbolType { get; private set; }
        public Dictionary<int, Fraction> FreqDict { get; private set; }
        public int Denominator { get; private set; }
        public CodingTableStreamer(Dictionary<int, Fraction> frequencyDictionary)
        {
            int largestNumberBitsSymbol = 0,
                largestNumberBitsFrequency = 0;
            foreach(KeyValuePair<int, Fraction> pair in frequencyDictionary)
            {
                int numBitsSymbol = BitUtilities.GetNumberBitsToRepresent(pair.Key);
                int numBitsFrequency = BitUtilities.GetNumberBitsToRepresent(pair.Value.Numerator);
                if (numBitsSymbol > largestNumberBitsSymbol)
                    largestNumberBitsSymbol = numBitsSymbol;
                if (numBitsFrequency > largestNumberBitsFrequency)
                    largestNumberBitsFrequency = numBitsFrequency;
                NumFrequencies++;
            }
            LargestSymbolBits = largestNumberBitsSymbol;
            CalculateType();
            LargestFrequencyBits = largestNumberBitsFrequency;
            FreqDict = frequencyDictionary;
        }

        private void CalculateType()
        {
            if (LargestSymbolBits <= 8)
                SymbolType = typeof(byte);
            else if (LargestSymbolBits <= 16)
                SymbolType = typeof(short);
            else
                SymbolType = typeof(int);
        }

        public CodingTableStreamer(BitReader readerIn)
        {
            LargestSymbolBits = readerIn.ReadInt(6);
            CalculateType();
            LargestFrequencyBits = readerIn.ReadInt(6);
            NumFrequencies = readerIn.ReadInt(32);
            List<int> numerators = new List<int>();
            List<int> symbols = new List<int>();
            int denominator = 0;
            for(int i = 0; i < NumFrequencies; i++)
            {
                symbols.Add(readerIn.ReadInt(LargestSymbolBits));
                numerators.Add(readerIn.ReadInt(LargestFrequencyBits));
                denominator += numerators[numerators.Count - 1];
            }
            FreqDict = new Dictionary<int, Fraction>();
            for(int i = 0; i < numerators.Count; i++)
                FreqDict[symbols[i]] = new Fraction(numerators[i], denominator);
            Denominator = denominator;
        }

        public void WriteTable(BitWriter writer)
        {
            writer.WriteLong(LargestSymbolBits, 6);
            writer.WriteLong(LargestFrequencyBits, 6);
            writer.WriteLong(NumFrequencies, 32);
            foreach (KeyValuePair<int, Fraction> pair in FreqDict)
            {
                writer.WriteLong(pair.Key, (byte)LargestSymbolBits);
                writer.WriteLong(pair.Value.Numerator, (byte)LargestFrequencyBits);
            }
        }
    }
}
