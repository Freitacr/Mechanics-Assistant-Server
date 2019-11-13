using System;
using System.Collections.Generic;
using System.Text;
using EncodingUtilities;

namespace ANSEncodingLib
{
    public class AnsCodingTable
    {
        public static readonly int DEFAULT_DENOMINATOR = 4096;

        public int Denominator { get; private set; }
        private Dictionary<int, Fraction> FrequencyDictionary;
        private Dictionary<int, Dictionary<int, int>> Table;
        private Dictionary<int, KeyValuePair<int, int>> DecodingDictionary;
        private CodingTableStreamer Streamer;
        public Type SymbolType { get; private set; }

        public AnsCodingTable(FrequencyDictionary<int> observedFrequencies, int denominator = -1, byte[] encryptionKey = null)
        {
            if (denominator <= 0)
                denominator = DEFAULT_DENOMINATOR;

            Denominator = (int)observedFrequencies.TotalFrequency;
            FrequencyDictionary = new Dictionary<int, Fraction>();
            foreach(FrequencyStorage<int> feature in observedFrequencies.GetSortedFrequencies())
                FrequencyDictionary[feature.ContainedObject] = new Fraction(feature.Frequency, Denominator);
            List<Fraction> toSimplify = new List<Fraction>();
            foreach (KeyValuePair<int, Fraction> pair in FrequencyDictionary)
                toSimplify.Add(pair.Value);
            toSimplify.Sort(Fraction.SORTER);
            int sharedDenominator = denominator;
            try
            {
                Fraction.ScaleFractionsToDenominator(toSimplify, sharedDenominator);
            } catch (ArgumentOutOfRangeException e)
            {
                throw new ArgumentException("Denominator insufficient to construct table without losing frequency fidelity. Either reduce the block size, or increase the denominator", e);
            }
            Streamer = new CodingTableStreamer(FrequencyDictionary);
            Denominator = toSimplify[0].Denominator;
            GenerateTable(encryptionKey);
        }

        private AnsCodingTable()
        {

        }

        public Dictionary<int, int> this[int x]
        {
            get
            {
                return Table[x];
            }
        }

        private void GenerateTable(byte[] encryptionKey)
        {
            List<int> viableStates = new List<int>();
            int maxState = (Denominator << 1);
            for (int i = Denominator; i < maxState; i++)
                viableStates.Add(i);
            if (encryptionKey != null)
            {
                SeededRng rng = new SeededRng(encryptionKey);
                RandomShuffler<int>.ShuffleList(viableStates, rng);
            }
            else
            {
                RandomShuffler<int>.ShuffleList(viableStates);
            }
            Table = new Dictionary<int, Dictionary<int, int>>();
            GenerateStateRanges(out var stateRanges);
            FillTable(stateRanges, viableStates);
            
            
        }

        private void GenerateStateRanges(out Dictionary<int, List<int>> stateRanges)
        {
            stateRanges = new Dictionary<int, List<int>>();
            foreach (KeyValuePair<int, Fraction> pair in FrequencyDictionary)
            {
                List<int> stateRange = new List<int>();
                int max = pair.Value.Numerator << 1;
                for (int i = pair.Value.Numerator; i < max; i++)
                    stateRange.Add(i);
                stateRanges[pair.Key] = stateRange;
            }
        }

        private void FillTable(Dictionary<int, List<int>> stateRanges, List<int> viableStates)
        {
            foreach(KeyValuePair<int, List<int>> pair in stateRanges)
            {
                Table[pair.Key] = new Dictionary<int, int>();
                List<int> states = viableStates.GetRange(0, pair.Value.Count);
                viableStates.RemoveRange(0, pair.Value.Count);
                states.Sort();
                for(int i = 0; i < pair.Value.Count; i++)
                    Table[pair.Key][pair.Value[i]] = states[i];
            }
        }

        public KeyValuePair<int, int> DecodePoint(int numberIn)
        {
            if (DecodingDictionary == null)
                GenerateDecodingDictionary();
            return DecodingDictionary[numberIn];
        }

        private void GenerateDecodingDictionary()
        {
            //mapping the stats number to their symbol and the row
            DecodingDictionary = new Dictionary<int, KeyValuePair<int, int>>();
            foreach(KeyValuePair<int, Dictionary<int, int>> pair in Table)
            {
                int symbol = pair.Key;
                foreach(KeyValuePair<int, int> rowState in pair.Value)
                {
                    int row = rowState.Key;
                    int state = rowState.Value;
                    DecodingDictionary[state] = new KeyValuePair<int, int>(symbol, row);
                }
            }
        }

        public void WriteTable(BitWriter writer)
        {
            Streamer.WriteTable(writer);
        }

        public static AnsCodingTable ReadTable(BitReader reader, byte[] encryptionKey = null)
        {
            AnsCodingTable ret = new AnsCodingTable();
            ret.Streamer = new CodingTableStreamer(reader);
            ret.FrequencyDictionary = ret.Streamer.FreqDict;
            ret.Denominator = ret.Streamer.Denominator;
            ret.SymbolType = ret.Streamer.SymbolType;
            ret.GenerateTable(encryptionKey);
            return ret;
        }
    }
}
