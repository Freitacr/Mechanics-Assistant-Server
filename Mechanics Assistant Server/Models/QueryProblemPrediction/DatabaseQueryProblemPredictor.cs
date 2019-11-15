using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OMISSortingLib;

namespace OldManInTheShopServer.Models.QueryProblemPrediction
{
    class EntrySimilarity
    {
        public float Difference { get; set; }
        public JobDataEntry Entry { get; set; }
    }

    class DatabaseQueryProblemPredictor : IDatabaseQueryProblemPredictor
    {

        private static float CalcSimilarity(JobDataEntry query, JobDataEntry other)
        {
            float dist = 0.0f;
            if (!query.Make.Equals(other.Make))
                dist += 3;
            else if (!query.Model.Equals(other.Model))
                dist += 1;
            List<int> queryComplaintGroups = ExtractComplaintGroups(query);
            List<int> otherComplaintGroups = ExtractComplaintGroups(query);
            return dist + CalcDistance(queryComplaintGroups, otherComplaintGroups);
        }

        private static List<int> ExtractComplaintGroups(JobDataEntry entry)
        {
            string entryComplaintGroups = entry.ComplaintGroups.Substring(1, entry.ComplaintGroups.Length - 2);
            string[] complaintGroups = entryComplaintGroups.Split(',');
            List<int> ret = new List<int>();
            foreach (string s in complaintGroups)
                ret.Add(int.Parse(s));
            return ret;
        }

        private static float CalcDistance(List<int> firstPoint, List<int> otherPoint)
        {
            int maxIndex = Math.Min(firstPoint.Count, otherPoint.Count);
            float ret = 0.0f;
            for(int i = 0; i < maxIndex; i++)
            {
                ret += (float) Math.Pow(firstPoint[i] - otherPoint[i], 2);
            }
            return (float) Math.Sqrt(ret);
        }
    
        public List<EntrySimilarity> GetQueryResults(JobDataEntry query, List<JobDataEntry> potentials, int numRequested, int offset = 0)
        {
            Dictionary<float, List<EntrySimilarity>> distanceMappings = new Dictionary<float, List<EntrySimilarity>>();
            HashSet<float> keys = new HashSet<float>();
            List<EntrySimilarity> ret = new List<EntrySimilarity>();
            int requiredNum = numRequested;
            foreach (JobDataEntry other in potentials)
            {
                float dist = CalcSimilarity(query, other);
                if (!distanceMappings.ContainsKey(dist))
                {
                    keys.Add(dist);
                    distanceMappings.Add(dist, new List<EntrySimilarity>());
                }
                distanceMappings[dist].Add(new EntrySimilarity() { Entry = other, Difference = dist });
            }

            float[] sortedKeys = new float[keys.Count];
            keys.CopyTo(sortedKeys);
            sortedKeys.RadixSort();
            int keyIndex = 0;
            while (ret.Count <= requiredNum && keyIndex < sortedKeys.Length)
            {
                ret.AddRange(distanceMappings[sortedKeys[keyIndex]]);
                keyIndex++;
            }
            if (ret.Count < numRequested)
                return ret;
            return ret.GetRange(0, numRequested);
        }
    }
}
