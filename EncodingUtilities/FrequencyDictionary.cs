using System;
using System.Collections.Generic;

namespace EncodingUtilities
{
    internal class FrequencyStorageComparer<C> : IComparer<FrequencyStorage<C>>
    {
        public int Compare(FrequencyStorage<C> x, FrequencyStorage<C> y)
        {
            if (y.Frequency != x.Frequency)
                return y.Frequency.CompareTo(x.Frequency);
            else
            {
                if (y.ContainedObject is IComparable yComp)
                    return yComp.CompareTo(x.ContainedObject);
                return 0;
            }
            throw new NotImplementedException();
        }
    }
    public class FrequencyStorage<C>
    {
        public C ContainedObject { get; private set; }
        public int Frequency { get; private set; }

        public FrequencyStorage(C containedObject)
        {
            ContainedObject = containedObject;
            Frequency = 0;
        }

        public void Increment() { Frequency++; }

        public void Decrement() { Frequency--; }

        public override bool Equals(object obj)
        {
            if (!(obj is FrequencyStorage<C> other))
                return false;
            return other.ContainedObject.Equals(ContainedObject) && other.Frequency == Frequency;
        }

        public override int GetHashCode()
        {
            return ContainedObject.GetHashCode() + Frequency;
        }

    }

    public class FrequencyDictionary<T>
    {

        private readonly HashSet<FrequencyStorage<T>> ContainedElements = new HashSet<FrequencyStorage<T>>();
        private readonly Dictionary<T, FrequencyStorage<T>> TempElements = new Dictionary<T, FrequencyStorage<T>>();

        public long TotalFrequency
        {
            get
            {
                long ret = 0;
                foreach (FrequencyStorage<T> storage in ContainedElements)
                    ret += storage.Frequency;
                return ret;
            }
        }

        public FrequencyDictionary()
        {

        }

        private static int CalculateContainedBits(int bitsIn)
        {
            for (int i = 31; i >= 0; i--)
                if ((bitsIn & (1u << i)) != 0)
                    return i + 1;
            return 0;
        }

        public static void WriteDictionary(FrequencyDictionary<int> toWrite, BitWriter writerOut)
        {
            writerOut.WriteLong(toWrite.ContainedElements.Count, 16);
            int largestBitsNumerator = 0;
            int largestBitsSymbol = 0;
            foreach(FrequencyStorage<int> storage in toWrite.ContainedElements)
            {
                int numeratorBits = CalculateContainedBits(storage.Frequency);
                if (numeratorBits > largestBitsNumerator)
                    largestBitsNumerator = numeratorBits;
                int symbolBits = CalculateContainedBits(storage.ContainedObject);
                if (symbolBits > largestBitsSymbol)
                    largestBitsSymbol = symbolBits;
            }
            writerOut.WriteLong(largestBitsNumerator-1, 7);
            writerOut.WriteLong(largestBitsSymbol-1, 7);
            foreach(FrequencyStorage<int> storage in toWrite.ContainedElements)
            {
                writerOut.WriteLong(storage.ContainedObject, (byte)largestBitsSymbol);
                writerOut.WriteLong(storage.Frequency, (byte)largestBitsNumerator);
            }
        }

        public static FrequencyDictionary<int> ReadDictionary(BitReader readerIn)
        {
            FrequencyDictionary<int> ret = new FrequencyDictionary<int>();
            int numElements = readerIn.ReadInt(16);
            int largestNumeratorBits = readerIn.ReadInt(7)+1;
            int largestSymbolBits = readerIn.ReadInt(7)+1;
            for(int i = 0; i < numElements; i++)
            {
                int symbol = readerIn.ReadInt(largestSymbolBits);
                int frequency = readerIn.ReadInt(largestNumeratorBits);
                for (int j = 0; j < frequency; j++)
                    ret.AddData(symbol);
            }
            return ret;
        }

        public bool AddData(T toAdd)
        {
            if (!TempElements.ContainsKey(toAdd))
            {
                TempElements[toAdd] = new FrequencyStorage<T>(toAdd);
                ContainedElements.Add(TempElements[toAdd]);
            }
            TempElements[toAdd].Increment();
            return true;
        }

        public bool AddData(ICollection<T> elementsToAdd)
        {
            bool ret = true;
            foreach (T elem in elementsToAdd)
                ret &= AddData(elem);
            return ret;
        }

        public List<T> SortByFrequency()
        {
            List<FrequencyStorage<T>> storages = new List<FrequencyStorage<T>>(ContainedElements);
            storages.Sort(new FrequencyStorageComparer<T>());
            List<T> ret = new List<T>();
            foreach (FrequencyStorage<T> storage in storages)
                ret.Add(storage.ContainedObject);
            return ret;
        }

        public List<T> SortByFrequency(ICollection<T> elementsToSort)
        {
            ContainedElements.Clear();
            AddData(elementsToSort);
            return SortByFrequency();
        }

        public List<FrequencyStorage<T>> GetSortedFrequencies()
        {
            List<FrequencyStorage<T>> storages = new List<FrequencyStorage<T>>(ContainedElements);
            storages.Sort(new FrequencyStorageComparer<T>());
            return storages;
        }

        public List<FrequencyStorage<T>> GetSortedFrequencies(ICollection<T> elementsToSort)
        {
            ContainedElements.Clear();
            AddData(elementsToSort);
            return GetSortedFrequencies();
        }

        public void Clear()
        {
            ContainedElements.Clear();
        }
    }
}
