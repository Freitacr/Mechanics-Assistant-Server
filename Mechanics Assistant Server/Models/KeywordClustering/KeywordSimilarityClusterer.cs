﻿using System;
using System.Collections.Generic;
using System.IO;

namespace OldManInTheShopServer.Models.KeywordClustering
{
    /**<summary>Keyword Clusterer designed to cluster keywords together into groups based on the frequency the keywords are used with one another</summary>*/
    public class KeywordSimilarityClusterer : IKeywordClusterer
    {
        internal class KeyValuePairSorter : IComparer<KeyValuePair<string, int>>
        {
            public int Compare(KeyValuePair<string, int> x, KeyValuePair<string, int> y)
            {
                return y.Value.CompareTo(x.Value);
            }
        }

        private static readonly int DEFAULT_NUMBER_OF_GROUPS = 8;
        private static readonly int DEFAULT_MAX_EPOCHS = 25;

        private List<KeywordGroup> ContainedGroups;

        public KeywordSimilarityClusterer()
        {
            ContainedGroups = new List<KeywordGroup>();
        }

        public void Train(List<KeywordExample> data)
        {
            double minimumMembers = Math.Pow(1.2,
                    (Math.Log(data.Count) / Math.Log(1.88))
                );
            //minimumMembers -= 3;

            List<ClaimableKeywordExample> claimableData = new List<ClaimableKeywordExample>();
            foreach (KeywordExample ex in data)
                claimableData.Add(new ClaimableKeywordExample(ex));

            List<string> topKeywords = GetKKeywordsFromData(claimableData, DEFAULT_NUMBER_OF_GROUPS);
            HashSet<KeywordGroup> unsortedGroups = GetGroupsFromData(claimableData, topKeywords, minimumMembers, DEFAULT_MAX_EPOCHS);
            ContainedGroups = SortGroups(unsortedGroups);
        }

        private List<KeywordGroup> SortGroups(HashSet<KeywordGroup> unsortedGroups)
        {
            return KeywordGroupSimilaritySorter.SortKeywordGroups(unsortedGroups);
        }

        private List<ClaimableKeywordExample> GetDefaultExamples(List<ClaimableKeywordExample> dataIn)
        {
            List<ClaimableKeywordExample> ret = new List<ClaimableKeywordExample>();
            foreach (ClaimableKeywordExample ex in dataIn)
                if (!ex.Claimed)
                    ret.Add(ex);
            return ret;
        }

        private List<string> GetKKeywordsFromData(List<ClaimableKeywordExample> dataIn, int k)
        {
            List<string> ret = new List<string>();
            Dictionary<string, int> amountDictionaries = new Dictionary<string, int>();
            foreach(ClaimableKeywordExample ex in dataIn)
            {
                if(!ex.Claimed)
                {
                    foreach(string keyword in ex.ContainedExample)
                    {
                        if (!amountDictionaries.ContainsKey(keyword))
                            amountDictionaries.Add(keyword, 0);
                        amountDictionaries[keyword] += 1;
                    }
                }
            }
            List<KeyValuePair<string, int>> sortedKeywords = new List<KeyValuePair<string, int>>();
            foreach(KeyValuePair<string, int> keywordPair in amountDictionaries)
                sortedKeywords.Add(keywordPair);
            sortedKeywords.Sort(new KeyValuePairSorter());
            foreach(KeyValuePair<string, int> keywordPair in sortedKeywords)
            {
                ret.Add(keywordPair.Key);
                if (ret.Count == k)
                    break;
            }
            return ret;
        }

        private HashSet<KeywordGroup> GetGroupsFromData(List<ClaimableKeywordExample> data, List<string> topKeywords, double minimumMembers, int maxEpochs)
        {
            HashSet<KeywordGroup> ret = new HashSet<KeywordGroup>();
            List<ClaimableKeywordExample> defaults = data;
            AddGroupsFromData(data, topKeywords, minimumMembers, defaults, ret);
            int largestGroupSize = CalculateLargestGroupSize(ret);
            List<KeywordGroup> subGroups = new List<KeywordGroup>();
            foreach (KeywordGroup x in ret)
                subGroups.AddRange(x.GenerateSubGroups(largestGroupSize, minimumMembers));
            ret.UnionWith(subGroups);
            return ret;
        }

        private HashSet<KeywordGroup> RemoveDuplicateAndInvalidGroups(HashSet<KeywordGroup> groupsIn, double minimumMembers)
        {
            HashSet<KeywordGroup> ret = new HashSet<KeywordGroup>();
            foreach (KeywordGroup g in groupsIn)
            {
                if (g.Count < minimumMembers)
                {
                    g.DeleteClaims();
                    continue;
                }

                if (!ret.Add(g))
                    g.DeleteClaims();
            }
            return ret;
        }

        private void AddGroupsFromData(List<ClaimableKeywordExample> data, List<string> topKeywords, double minimumMembers, List<ClaimableKeywordExample> ungroupedExamples, HashSet<KeywordGroup> currentGroups)
        {
            double averageGroupSize = 0;
            while (ungroupedExamples.Count > averageGroupSize)
            {
                int currentKeywordIndex = 0;
                while (true)
                {
                    KeywordGroup tempGroup = new KeywordGroup(topKeywords[currentKeywordIndex]);
                    tempGroup.UpdateMembers(data);
                    if (tempGroup.Count > minimumMembers && !IsGroupInContainedGroups(tempGroup) && !currentGroups.Contains(tempGroup))
                    {
                        bool wasAdded = currentGroups.Add(tempGroup);
                        //List<KeywordGroup> subGroups = tempGroup.GenerateSubGroups(CalculateLargestGroupSize(currentGroups), minimumMembers);
                        if (wasAdded)
                            break;
                        else
                            tempGroup.DeleteClaims();
                    }
                    currentKeywordIndex++;
                    if (currentKeywordIndex == topKeywords.Count)
                        break;
                }

                ungroupedExamples = GetDefaultExamples(data);
                topKeywords = GetKKeywordsFromData(data, DEFAULT_NUMBER_OF_GROUPS);
                averageGroupSize = CalculateAverageGroupSize(currentGroups);
                if (currentKeywordIndex == topKeywords.Count)
                    break;
            }
        }

        private bool IsGroupInContainedGroups(KeywordGroup groupIn)
        {
            foreach (KeywordGroup g in ContainedGroups)
                if (g == groupIn)
                    return true;
            return false;
        }

        private double CalculateAverageGroupSize()
        {
            double ret = 0;
            foreach (KeywordGroup g in ContainedGroups)
                ret += g.Count;
            return ret / ContainedGroups.Count;
        }

        private double CalculateAverageGroupSize(HashSet<KeywordGroup> currentGroups)
        {
            double ret = 0;
            foreach (KeywordGroup g in currentGroups)
                ret += g.Count;
            return ret / currentGroups.Count;
        }

        private int CalculateLargestGroupSize(HashSet<KeywordGroup> groupsIn)
        {
            int currMax = 0;
            foreach (KeywordGroup g in groupsIn)
            {
                if (currMax < g.Count)
                    currMax = g.Count;
            }
            return currMax;
        }

        public bool Load(Stream streamIn)
        {
            StreamReader fileReader;
            try
            {
                fileReader = new StreamReader(streamIn);
            } catch(FileNotFoundException)
            {
                return false;
            }
            ContainedGroups = new List<KeywordGroup>();
            while (!fileReader.EndOfStream)
            {
                string[] lineSplit = fileReader.ReadLine().Split(' ');
                KeywordGroup g = new KeywordGroup(lineSplit[0]);
                for (int i = 1; i < lineSplit.Length; i++)
                    g.SelectedKeywords.AddKeyword(lineSplit[i]);
                ContainedGroups.Add(g);
            }
            return true;
        }

        public bool Save(Stream streamIn)
        {
            StreamWriter fileWriter;
            try
            {
                fileWriter = new StreamWriter(streamIn);
            } catch(FileNotFoundException)
            {
                return false;
            }
            foreach (KeywordGroup g in ContainedGroups) {
                List<string> lineOut = new List<string>();
                var enumerator = g.SelectedKeywords.ContainedKeywords;
                while (enumerator.MoveNext())
                    lineOut.Add(enumerator.Current);
                fileWriter.WriteLine(string.Join(' ', lineOut.ToArray()));
            }
            fileWriter.Close();
            return true;
        }

        /**<summary>Calculates the similarity for the provided example to every one of the groups contained in this clusterer.</summary>*/
        public List<int> PredictGroupSimilarity(KeywordExample exampleIn)
        {
            SortedDictionary<double, List<int>> similarityScores = new SortedDictionary<double, List<int>>();
            for(int i = 0; i < ContainedGroups.Count; i++)
            {
                double similarityScore = ContainedGroups[i].CalculateSimilarityScore(exampleIn);
                if (similarityScore != 0)
                {
                    if (!similarityScores.ContainsKey(similarityScore))
                        similarityScores.Add(similarityScore, new List<int> { i+1 });
                    else
                        similarityScores[similarityScore].Add(i+1);
                }
            }
            if (similarityScores.Count == 0)
                similarityScores.Add(0, new List<int> { 0 });
            List<int> sortedGroups = new List<int>();
            foreach(KeyValuePair<double, List<int>> simScore in similarityScores)
                foreach (int group in simScore.Value)
                    sortedGroups.Add(group);
            return sortedGroups;
        }

        /**<summary>Calculates the similarity for the provided example to every one of the groups contained in this clusterer and returns the top n.</summary>*/
        public List<int> PredictTopNSimilarGroups(KeywordExample exampleIn, int n)
        {
            List<int> similarityScores = PredictGroupSimilarity(exampleIn);
            if (similarityScores.Count < n)
                for (int i = similarityScores.Count; i < n; i++)
                    similarityScores.Add(0);
            return similarityScores.GetRange(0, n);
        }
    }
}
