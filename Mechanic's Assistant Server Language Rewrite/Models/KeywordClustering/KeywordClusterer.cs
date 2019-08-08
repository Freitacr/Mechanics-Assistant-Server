using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Models.KeywordClustering
{
    public class KeywordClusterer
    {
        private static readonly int DEFAULT_NUMBER_OF_GROUPS = 8;
        private static readonly int DEFAULT_MAX_EPOCHS = 200;

        private List<KeywordGroup> ContainedGroups;

        public KeywordClusterer()
        {
            ContainedGroups = new List<KeywordGroup>();
        }

        public void Train(List<KeywordExample> data)
        {
            double minimumMembers = Math.Pow(1.2,
                    (Math.Log(data.Count) / Math.Log(1.88))
                );
            minimumMembers -= 3;

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
            throw new NotImplementedException();
        }

        private HashSet<KeywordGroup> GetGroupsFromData(List<ClaimableKeywordExample> data, List<string> topKeywords, double minimumMembers, int maxEpochs)
        {
            HashSet<KeywordGroup> ret = new HashSet<KeywordGroup>(), 
                previousGroups = new HashSet<KeywordGroup>();
            List<ClaimableKeywordExample> defaults = data;
            List<string> currentDisplayedKeywords = topKeywords;
            AddGroupsFromData(data, topKeywords, minimumMembers, defaults, ret);
            bool groupListChanged = true;
            int epochCount = 0;
            while (groupListChanged && epochCount < maxEpochs)
            {
                
                bool groupChangesObserved = false;
                if (ret.Count == previousGroups.Count)
                {
                    foreach (KeywordGroup g in ret)
                    {
                        if (!previousGroups.Contains(g))
                        {
                            groupChangesObserved = true;
                            break;
                        }
                    }
                }

                //Strange loop break condition.
                //Basically, if the list of active groups didn't actually change last epoch
                //And we've tried as many times as we are expecting there to be KeywordExamples in each group
                //Then break... TODO: Decide about the fate of this condition.
                if (!groupChangesObserved && epochCount > minimumMembers)
                {
                    groupListChanged = false;
                    continue;
                }
                int largestGroupSize = CalculateLargestGroupSize(ret);
                foreach(KeywordGroup g in ret)
                {
                    g.UpdateSelectedKeywords(largestGroupSize);
                    if (g.ChangeState)
                    {
                        if (g.Count > largestGroupSize)
                            largestGroupSize = g.Count;
                    }
                }
                defaults = GetDefaultExamples(data);
                currentDisplayedKeywords = GetKKeywordsFromData(defaults, DEFAULT_NUMBER_OF_GROUPS);
                previousGroups = new HashSet<KeywordGroup>(ret);
                AddGroupsFromData(data, currentDisplayedKeywords, minimumMembers, defaults, ret);

                groupListChanged = false;
                foreach (KeywordGroup g in ret)
                {
                    if (!previousGroups.Contains(g))
                    {
                        groupListChanged = true;
                        break;
                    }
                }

                //This is necessary because the hashCode of the KeywordGroups are based off of mutable data
                //This is essentially resyncing the hashes to the changed data
                ret = RemoveDuplicateAndInvalidGroups(ret, minimumMembers);
            }
            return ret;
        }

        private HashSet<KeywordGroup> RemoveDuplicateAndInvalidGroups(HashSet<KeywordGroup> groupsIn, double minimumMembers)
        {
            HashSet<KeywordGroup> ret = new HashSet<KeywordGroup>();
            foreach(KeywordGroup g in groupsIn)
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
                    if (tempGroup.Count > minimumMembers && !IsGroupInContainedGroups(tempGroup))
                    {
                        bool wasAdded = currentGroups.Add(tempGroup);
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
                averageGroupSize = CalculateAverageGroupSize();
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

        private int CalculateLargestGroupSize(HashSet<KeywordGroup> groupsIn)
        {
            int currMax = 0;
            foreach(KeywordGroup g in groupsIn)
            {
                if (currMax < g.Count)
                    currMax = g.Count;
            }
            return currMax;
        }
    }
}
