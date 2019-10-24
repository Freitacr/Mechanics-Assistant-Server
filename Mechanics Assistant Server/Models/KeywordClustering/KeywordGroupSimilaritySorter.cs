using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Mechanics Assistant Server Tests")]
#endif

namespace OldManInTheShopServer.Models.KeywordClustering
{
    /**<summary>Class to sort keyword groups based on shared examples. The sorting is based on the most similar to all sorted groups, with similarity being weighted by the distance
     * from a group to the new potential sorted slot</summary>*/
    class KeywordGroupSimilaritySorter
    {

        internal class GroupSimilarity
        {
            public KeywordGroup ContainedGroup;
            public double Similarity;

            public GroupSimilarity(KeywordGroup groupIn, double similarity)
            {
                ContainedGroup = groupIn;
                Similarity = similarity;
            }

            public override string ToString()
            {
                return ContainedGroup.ToString() + ": " + Similarity;
            }
        }

        internal class GroupSimilaritySorter : IComparer<GroupSimilarity>
        {
            public static readonly GroupSimilaritySorter Instance = new GroupSimilaritySorter();
            public int Compare(GroupSimilarity x, GroupSimilarity y)
            {
                return y.Similarity.CompareTo(x.Similarity);
            }
        }

        public static List<KeywordGroup> SortKeywordGroups(HashSet<KeywordGroup> unsortedGroups)
        {
            Dictionary<KeywordGroup, Dictionary<KeywordGroup, double>> rankingDictionaries = 
                new Dictionary<KeywordGroup, Dictionary<KeywordGroup, double>>();
            FillRankingDictionaries(unsortedGroups, rankingDictionaries);
            return SortGroupsByRankingDictionary(rankingDictionaries);
        }

        private static void FillRankingDictionaries(HashSet<KeywordGroup> unsortedGroups, Dictionary<KeywordGroup, Dictionary<KeywordGroup, double>> rankingDictionaries)
        {
            foreach(KeywordGroup group in unsortedGroups)
            {
                rankingDictionaries.Add(group, new Dictionary<KeywordGroup, double>());
                foreach (KeywordGroup group2 in unsortedGroups)
                {
                    rankingDictionaries[group][group2] = group.CalculateSimilarityScore(group2);   
                }
                rankingDictionaries[group][group] = -1;
            }
        }

        private static List<KeywordGroup> SortGroupsByRankingDictionary(Dictionary<KeywordGroup, Dictionary<KeywordGroup, double>> rankingDictionaries)
        {
            List<KeywordGroup> orderedGroups = new List<KeywordGroup>();
            orderedGroups.Add(RetrieveGroupWithLeastSimilarity(rankingDictionaries));
            while(orderedGroups.Count != rankingDictionaries.Count)
            {
                List<GroupSimilarity> potentialMatches = new List<GroupSimilarity>();
                foreach (KeywordGroup potentialMatch in rankingDictionaries.Keys)
                {
                    double score = 0.0;
                    int index = 1;
                    foreach(KeywordGroup orderedGroup in orderedGroups)
                    {
                        if (score == -1)
                            continue;
                        double pairingScore = rankingDictionaries[orderedGroup][potentialMatch];
                        if (pairingScore == -1)
                        {
                            score = -1;
                            break;
                        }
                        pairingScore *= index / (double)orderedGroup.Count;
                        score += pairingScore;
                        index++;
                    }
                    potentialMatches.Add(new GroupSimilarity(potentialMatch, score));
                }
                potentialMatches.Sort(GroupSimilaritySorter.Instance);
                orderedGroups.Add(potentialMatches[0].ContainedGroup);
            }
            return orderedGroups;
        }

        private static KeywordGroup RetrieveGroupWithLeastSimilarity(Dictionary<KeywordGroup, Dictionary<KeywordGroup, double>> rankingDictionaries)
        {
            KeywordGroup leastSimilarGroup = null;
            double minimumSimilarity = double.MaxValue;
            foreach(KeyValuePair<KeywordGroup, Dictionary<KeywordGroup, double>> topPair in rankingDictionaries)
            {
                double totalSimilarity = 0.0;
                foreach(KeyValuePair<KeywordGroup, double> similarityPair in topPair.Value)
                {
                    if (similarityPair.Value == -1)
                        continue;
                    totalSimilarity += similarityPair.Value;
                }
                if(totalSimilarity < minimumSimilarity)
                {
                    minimumSimilarity = totalSimilarity;
                    leastSimilarGroup = topPair.Key;
                }
            }
            return leastSimilarGroup;
        }
    }
}
