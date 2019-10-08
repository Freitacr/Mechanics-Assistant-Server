using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Mechanics Assistant Server Tests")]
#endif

namespace MechanicsAssistantServer.Models.KeywordClustering
{
    /**<summary>Class that represents a group of keyword examples. 
     * This group is defined by a set of selected keywords, 
     * and all elements of the group must contain all of the defining keywords</summary>*/
    public class KeywordGroup
    {
        private List<ClaimableKeywordExample> ContainedMembers;
        private Dictionary<string, int> ContainedKeywords;
        public KeywordExample SelectedKeywords;
        public int Count { get { return ContainedMembers.Count; } }
        public bool ChangeState { get; private set; } = true;
        //private static readonly KeywordExampleComparer Cmp = new KeywordExampleComparer();

        public KeywordGroup(string keyword)
        {
            SelectedKeywords = new KeywordExample();
            SelectedKeywords.AddKeyword(keyword);
            ContainedKeywords = new Dictionary<string, int>();
            ContainedMembers = new List<ClaimableKeywordExample>();
        }

        private void AddMember(ClaimableKeywordExample toAdd)
        {
            ContainedMembers.Add(toAdd);
            foreach (string keyword in toAdd.ContainedExample)
            {
                if (!ContainedKeywords.ContainsKey(keyword))
                    ContainedKeywords[keyword] = 0;
                ContainedKeywords[keyword] += 1;
            }
            toAdd.Claim();
        }

        private void RemoveMember(ClaimableKeywordExample toRemove)
        {
            ContainedMembers.Remove(toRemove);
            foreach(string keyword in toRemove.ContainedExample)
            {
                ContainedKeywords[keyword] -= 1;
                if (ContainedKeywords[keyword] == 0)
                    ContainedKeywords.Remove(keyword);
            }
            toRemove.ReleaseClaim();
        }

        /**<summary>Generates all sub groups of the current group. A sub group is created if a quarter of the keywords contained within its examples are the same. If so,
         * then those keywords are added to its definition, and it becomes a new group. As a note, its parent group still exists as well.</summary>*/
        public List<KeywordGroup> GenerateSubGroups(int maxGroupSize, double minimumMembers)
        {
            double globalThreshold = .25;
            HashSet<KeywordGroup> ret = new HashSet<KeywordGroup>();

            foreach(string keyword in ContainedKeywords.Keys)
            {
                if(!SelectedKeywords.Contains(keyword))
                {
                    if (ContainedKeywords[keyword] / (double)ContainedMembers.Count >= globalThreshold)
                    {
                        var keywords = SelectedKeywords.ContainedKeywords;
                        keywords.MoveNext();
                        KeywordGroup temp = new KeywordGroup(keywords.Current);
                        while (keywords.MoveNext())
                            temp.SelectedKeywords.AddKeyword(keywords.Current);
                        temp.SelectedKeywords.AddKeyword(keyword);
                        temp.UpdateMembers(ContainedMembers);
                        if(temp.ContainedMembers.Count < minimumMembers)
                        {
                            temp.DeleteClaims();
                            continue;
                        }
                        foreach (KeywordGroup tempSubGroup in temp.GenerateSubGroups(maxGroupSize, minimumMembers))
                            if (!ret.Add(tempSubGroup))
                                tempSubGroup.DeleteClaims();
                        ret.Add(temp);
                    }
                }
            }
            return new List<KeywordGroup>(ret);
        }

        /** 
         * Updates the contained keyword examples based on the data passed in. An example is only added to this group if it contains
         * all of the keywords that are within the groups definition.
         *
         */
        public void UpdateMembers(List<ClaimableKeywordExample> dataIn)
        {
            ContainedMembers = new List<ClaimableKeywordExample>();
            foreach(ClaimableKeywordExample ex in dataIn)
                if (SelectedKeywords.CountSimilar(ex.ContainedExample) == SelectedKeywords.Count)
                    AddMember(ex);
        }

        public double CalculateSimilarityScore(KeywordExample example)
        {
            if (example.Count == 0)
                return 0;
            return SelectedKeywords.CountSimilar(example) / (double)example.Count;
        }

        public double CalculateSimilarityScore(KeywordGroup otherGroup)
        {
            double ret = 0;
            foreach(ClaimableKeywordExample x in ContainedMembers)
                if (otherGroup.ContainedMembers.Contains(x))
                    ret += 1;
            return ret / ContainedMembers.Count;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return (obj as KeywordGroup).SelectedKeywords.Equals(SelectedKeywords);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SelectedKeywords.GetHashCode();
        }

        public void DeleteClaims()
        {
            foreach (ClaimableKeywordExample ex in ContainedMembers)
                ex.ReleaseClaim();
        }

        public override string ToString()
        {
            List<string> keywords = new List<string>();
            foreach (string s in SelectedKeywords)
                keywords.Add(s);
            return string.Join(' ', keywords);
        }
    }
}
