using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace EncodingUtilities
{
    public class RandomShuffler<T>
    {
        public static void ShuffleList(List<T> listIn)
        {
            listIn.Add(listIn[0]);
            listIn.RemoveAt(0);
            for(int i = 2; i < 5; i++)
            {
                LinkedList<int> swappingPositions = new LinkedList<int>();
                LinkedList<int> nonSwapPositions = new LinkedList<int>();
                for(int j = 0; j < listIn.Count; j++)
                {
                    if (j % i == 0)
                        swappingPositions.AddLast(j);
                    else
                        nonSwapPositions.AddLast(j);
                }
                int count = swappingPositions.Count;
                for(int j = 0; j < count; j++)
                {
                    T temp = listIn[swappingPositions.First.Value];
                    listIn[swappingPositions.First.Value] = listIn[nonSwapPositions.First.Value];
                    listIn[nonSwapPositions.First.Value] = temp;
                    swappingPositions.RemoveFirst();
                    nonSwapPositions.RemoveFirst();
                }
            }
        }

        public static void ShuffleList(List<T> listIn, SeededRng rng)
        {
            for(int i = listIn.Count-1; i>0; i--)
            {
                int swapIndex = (int)rng.Next((uint)listIn.Count);
                T temp = listIn[swapIndex];
                listIn[swapIndex] = listIn[i];
                listIn[i] = temp;
            }
        }
    }
}
