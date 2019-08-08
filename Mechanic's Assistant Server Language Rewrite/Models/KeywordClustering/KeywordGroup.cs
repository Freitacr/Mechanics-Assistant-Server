using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Models.KeywordClustering
{
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

        public void UpdateSelectedKeywords(int maxGroupSize)
        {
            double globalThreshold = 1 -
            ( (ContainedMembers.Count / (double)maxGroupSize) *
              (1 - (SelectedKeywords.Count / 4.0))
            );

            bool keywordFound = false;
            foreach (string keyword in ContainedKeywords.Keys)
            {
                if (!SelectedKeywords.Contains(keyword))
                    if (ContainedKeywords[keyword] / (double)ContainedMembers.Count >= globalThreshold)
                    {
                        SelectedKeywords.AddKeyword(keyword);
                        ChangeState = true;
                        keywordFound = true;
                        break;
                    }
            }
            if (!keywordFound)
            {
                ChangeState = false;
                return;
            }
            UpdateMembers();
        }

        private void UpdateMembers()
        {
            List<ClaimableKeywordExample> toRemove = new List<ClaimableKeywordExample>();
            foreach(ClaimableKeywordExample ex in ContainedMembers)
                if (SelectedKeywords.CountSimilar(ex.ContainedExample) != SelectedKeywords.Count)
                    toRemove.Add(ex);
            foreach (ClaimableKeywordExample ex in toRemove)
                RemoveMember(ex);
        }

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
    }
}
