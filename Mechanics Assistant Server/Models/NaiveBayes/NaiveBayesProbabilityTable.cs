using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MechanicsAssistantServer.Models
{
    [DataContract]
    public class NaiveBayesProbabilityTable
    {
        [DataMember]
        private List<List<ProbabilityTableEntry>> ProbabilityTable;
        public int Denominator {
            get { return ProbabilityTable[0][0].Denominator; }
            set { SetDenominator(value); }
        }
        public int Rows { get { return ProbabilityTable.Count; } }

        public NaiveBayesProbabilityTable()
        {
            ProbabilityTable = new List<List<ProbabilityTableEntry>>();
        }

        public List<ProbabilityTableEntry> this[int key]
        {
            get { return ProbabilityTable[key]; }
        }

        public void AddRow(int numElements)
        {
            var toAdd = new List<ProbabilityTableEntry>();
            for (int i = 0; i < numElements; i++)
            {
                toAdd.Add(new ProbabilityTableEntry());
            }
            ProbabilityTable.Add(toAdd);
        }

        private void SetDenominator(int denominator)
        {
            foreach(List<ProbabilityTableEntry> tableRow in ProbabilityTable)
            {
                foreach(ProbabilityTableEntry entry in tableRow)
                {
                    entry.Denominator = denominator;
                }
            }
        }

        public void DetermineDenominator()
        {
            int sum = 0;
            foreach (List<ProbabilityTableEntry> tableRow in ProbabilityTable)
                foreach (ProbabilityTableEntry currEntry in tableRow)
                    sum += currEntry.Numerator;
            Denominator = sum;
        }

        public override string ToString()
        {
            string ret = "[";
            foreach (List<ProbabilityTableEntry> tableRow in ProbabilityTable)
            {
                ret += "\n\t[" + String.Join(", ", tableRow) + "]";
            }
            ret += "\n]";
            return ret;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var otherTable = obj as NaiveBayesProbabilityTable;
            if (otherTable.Rows != Rows)
                return false;
            for (int i = 0; i < otherTable.Rows; i++)
            {
                List<ProbabilityTableEntry> otherRow = otherTable[i];
                List<ProbabilityTableEntry> thisRow = this[i];
                if (otherRow.Count != thisRow.Count)
                    return false;
                for (int j = 0; j < thisRow.Count; j++)
                {
                    ProbabilityTableEntry otherEntry = otherRow[j];
                    ProbabilityTableEntry thisEntry = thisRow[j];
                    if (!otherEntry.Equals(thisEntry))
                        return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

    [DataContract]
    public class ProbabilityTableEntry
    {
        [DataMember]
        public int Numerator { get; set; }
        [DataMember]
        public int Denominator { get; set; }

        public ProbabilityTableEntry()
        {
            Numerator = 0;
            Denominator = 0;
        }

        public double ToDouble()
        {
            return Numerator / (double)Denominator;
        }

        public override string ToString()
        {
            return Numerator + "/" + Denominator;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ProbabilityTableEntry other = obj as ProbabilityTableEntry;
            return other.Numerator == Numerator && other.Denominator == Denominator;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
