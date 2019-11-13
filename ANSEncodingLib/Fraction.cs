using System;
using System.Collections.Generic;
using System.Text;

namespace ANSEncodingLib
{
    public class FractionComparer : IComparer<Fraction>
    {
        public int Compare(Fraction x, Fraction y)
        {
            double dx = (double)x.Numerator / x.Denominator;
            return dx.CompareTo((double)y.Numerator / y.Denominator);
        }
    }

    public class Fraction
    {
        public int Numerator;
        public int Denominator;
        public static readonly FractionComparer SORTER = new FractionComparer();

        public Fraction(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        private int GCM(int a, int b)
        {
            int divA, divB;
            if(a < b)
            {
                divB = a;
                divA = b;
            } else
            {
                divA = a;
                divB = b;
            }

            int remainder = divA % divB;
            while(remainder != 0)
            {
                divA = divB;
                divB = remainder;
                remainder = divA % divB;
            }
            return divB;
        }

        public void Simplify()
        {
            int gcm = GCM(Denominator, Numerator);
            while (gcm != 1)
            {
                Denominator /= gcm;
                Numerator /= gcm;
                gcm = GCM(Denominator, Numerator);
            }
        }

        public override string ToString()
        {
            return Numerator + "/" + Denominator;
        }

        public static void SharedSimplify(ICollection<Fraction> fractionsIn)
        {
            var enumerator = fractionsIn.GetEnumerator();
            enumerator.MoveNext();
            long denominator = enumerator.Current.Denominator;
            while (enumerator.MoveNext())
                if (enumerator.Current.Denominator != denominator)
                    throw new ArgumentException("Fraction " + enumerator.Current.ToString() + " did not have the denominator " + denominator);
            foreach(Fraction fraction in fractionsIn)
                fraction.Simplify();
            int largestDenominator = GetLargestDenominator(fractionsIn);
            foreach(Fraction fraction in fractionsIn)
            {
                int multiple = largestDenominator / fraction.Denominator;
                fraction.Numerator *= multiple;
                fraction.Denominator *= multiple;
            }
        }

        public static void ScaleFractionsToDenominator(ICollection<Fraction> fractionsIn, int targetDenominator)
        {
            int sum = 0;
            foreach(Fraction f in fractionsIn)
            {
                if (targetDenominator == f.Denominator)
                    continue;
                double denominatorProportion = (double)targetDenominator / f.Denominator;
                f.Denominator = targetDenominator;
                f.Numerator = (int)Math.Floor(denominatorProportion * f.Numerator);
                if (f.Numerator == 0)
                    f.Numerator++;
                sum += f.Numerator;
            }
            if(sum < targetDenominator)
            {
                LinkedList<Fraction> fracs = new LinkedList<Fraction>(fractionsIn);
                LinkedListNode<Fraction> curr = fracs.Last;
                int numToCorrect = targetDenominator - sum;
                for (int i = 0; i < numToCorrect && curr != null; i++)
                {
                    if(curr.Value.Numerator == targetDenominator-1)
                    {
                        i--;
                        continue;
                    }
                    curr.Value.Numerator++;
                    sum++;
                    curr = curr.Previous;
                }
                if (sum != targetDenominator)
                    throw new ArgumentOutOfRangeException("targetDenominator", "targetDenominator not large enough to support numerator normalization");
                
            } else if (sum > targetDenominator)
            {
                IEnumerator<Fraction> enumerator = fractionsIn.GetEnumerator();
                int numToCorrect = targetDenominator - sum;
                for (int i = 0; i < numToCorrect && enumerator.MoveNext(); i++)
                {
                    if(enumerator.Current.Numerator == 1)
                    {
                        i--;
                        continue;
                    }
                    enumerator.Current.Numerator--;
                    sum--;
                }
                if (sum != targetDenominator)
                    throw new ArgumentOutOfRangeException("targetDenominator", "targetDenominator not large enough to support numerator normalization");
            }
        }

        private static int GetLargestDenominator(ICollection<Fraction> fractions)
        {
            int largest = 0;
            foreach (Fraction f in fractions)
                if (f.Denominator > largest)
                    largest = f.Denominator;
            return largest;
        }

        public Fraction Copy()
        {
            return new Fraction(Numerator, Denominator);
        }
    }
}
