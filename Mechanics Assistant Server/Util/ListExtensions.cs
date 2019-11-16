using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> listIn)
        {
            if (listIn == null)
                throw new ArgumentNullException("listIn");
            Random shuffler = new Random();
            for(int i = listIn.Count-1; i > 0; i--)
            {
                int index = shuffler.Next(0, i);
                SwapElements(listIn, index, i);
            }
        }

        public static List<List<T>> Split<T>(this List<T> listIn, int numberSplits)
        {
            if (listIn == null)
                throw new ArgumentNullException("listIn");
            List<List<T>> ret = new List<List<T>>();
            int splitSize = listIn.Count / numberSplits;
            for (int i = 0; i < numberSplits-1; i++)
            {
                ret.Add(listIn.GetRange((i * splitSize), splitSize));
            }
            ret.Add(new List<T>());
            for (int i = (numberSplits - 1) * splitSize; i < listIn.Count; i++)
                ret.Last().Add(listIn[i]);
            return ret;
        }

        private static void SwapElements<T>(List<T> listIn, int index1, int index2)
        {
            T temp = listIn[index1];
            listIn[index1] = listIn[index2];
            listIn[index2] = temp;
        }
    }
}
