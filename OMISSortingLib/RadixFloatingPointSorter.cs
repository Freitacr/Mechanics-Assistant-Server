using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OMISSortingLib
{
    public static class RadixFloatingPointSorter
    {
        public static void RadixSort(this float[] listIn)
        {
            //ASSUMES ALL FLOATS ARE POSITIVE. I CANNOT STRESS THIS ENOUGH.
            //Based on a 32 bit definition of both float and int
            Span<float> floats = listIn;
            Span<int> asInts = MemoryMarshal.Cast<float, int>(floats);
            Span<int> tempArray = new Span<int>(new int[listIn.Length]);
            int[] counts = new int[256];
            int[] offsets = new int[256];
            int mask = 0xff;
            foreach (int i in asInts)
                counts[(i & mask)]++;
            
            for(int i = 0; i < 4; i++)
            {
                int nextMask = mask << 8;
                CalcOffsets();
                foreach(int curr in asInts)
                {
                    int radix = (curr & mask) >> (i * 8);
                    int index = offsets[radix];
                    offsets[radix]++;
                    tempArray[index] = curr;
                    counts[(curr & nextMask) >> ((i + 1) * 8)]++;
                }
                mask <<= 8;
                var swap = tempArray;
                tempArray = asInts;
                asInts = swap;
            }

            floats = MemoryMarshal.Cast<int, float>(asInts);
            for (int i = 0; i < listIn.Length; i++)
                listIn[i] = floats[i];

            void CalcOffsets()
            {
                offsets[0] = 0;
                for(int i = 1; i < offsets.Length; i++)
                {
                    offsets[i] = offsets[i - 1] + counts[i - 1];
                    counts[i - 1] = 0;
                }
            }
        }

        public static void RadixSort(this double[] listIn)
        {
            //ASSUMES ALL Doubles ARE POSITIVE. I CANNOT STRESS THIS ENOUGH.
            //Based on a 64 bit definition of both double and long
            Span<double> floats = listIn;
            Span<long> asInts = MemoryMarshal.Cast<double, long>(floats);
            Span<long> tempArray = new Span<long>(new long[listIn.Length]);
            int[] counts = new int[256];
            int[] offsets = new int[256];
            long mask = 0xff;
            foreach (long i in asInts)
                counts[(i & mask)]++;

            for (int i = 0; i < 8; i++)
            {
                long nextMask = mask << 8;
                CalcOffsets();
                foreach (long curr in asInts)
                {
                    int radix = (int)((curr & mask) >> (i * 8));
                    if (radix < 0)
                        radix = ~radix + 1;
                    int index = offsets[radix];
                    offsets[radix]++;
                    tempArray[index] = curr;
                    int nextRadix = (int)((curr & nextMask) >> ((i + 1) * 8));
                    if (nextRadix < 0)
                        nextRadix = ~nextRadix + 1;
                    counts[nextRadix]++;
                }
                mask <<= 8;
                var swap = tempArray;
                tempArray = asInts;
                asInts = swap;
            }

            floats = MemoryMarshal.Cast<long, double>(asInts);
            for (int i = 0; i < listIn.Length; i++)
                listIn[i] = floats[i];

            void CalcOffsets()
            {
                offsets[0] = 0;
                for (int i = 1; i < offsets.Length; i++)
                {
                    offsets[i] = offsets[i - 1] + counts[i - 1];
                    counts[i - 1] = 0;
                }
            }
        }
    }
}
