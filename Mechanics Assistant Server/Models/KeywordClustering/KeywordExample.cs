using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if DEBUG
[assembly:InternalsVisibleTo("Mechanics Assistant Server Tests")]
#endif

namespace OldManinTheShopServer.Models.KeywordClustering
{
    /**<summary>Represents one keyword example. This example is made up of individual key words, and cannot contain duplicate keywords</summary>*/
    public class KeywordExample
    {
        private HashSet<string> ModifiableKeywords;
        public HashSet<string>.Enumerator ContainedKeywords { get { return ModifiableKeywords.GetEnumerator(); } }
        public int Count { get { return ModifiableKeywords.Count; } }

        public KeywordExample()
        {
            ModifiableKeywords = new HashSet<string>();
        }

        public int CountSimilar(KeywordExample other)
        {
            int ret = 0;
            foreach (string x in other.ModifiableKeywords)
                if (ModifiableKeywords.Contains(x))
                    ret++;
            return ret;
        }

        public void AddKeyword(string toAdd)
        {
            ModifiableKeywords.Add(toAdd.ToLower());
        }

        public bool Contains(string keyword)
        {
            return ModifiableKeywords.Contains(keyword);
        }

        public HashSet<string>.Enumerator GetEnumerator()
        {
            return ContainedKeywords;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var ex = obj as KeywordExample;
            return ex.Count == Count && ex.CountSimilar(this) == Count;
                
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            int ret = 0;
            foreach (string x in ModifiableKeywords)
                ret += x.GetHashCode();
            return ret + Count;
        }

        public KeywordExample Copy()
        {
            KeywordExample ret = new KeywordExample();
            foreach (string keyword in this)
                ret.AddKeyword(keyword);
            return ret;
        }

        public override string ToString()
        {
            return string.Join(' ', ModifiableKeywords);
        }
    }
}
