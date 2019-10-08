namespace MechanicsAssistantServer.Models.KeywordClustering
{
    /**<summary>Represents a keyword example that can be claimed, and keeps tracks of the number of claims that it has</summary>*/
    public class ClaimableKeywordExample
    {
        public KeywordExample ContainedExample { get; private set; }
        private short NumberOfClaims = 0;
        public bool Claimed { get { return NumberOfClaims > 0; } }

        public ClaimableKeywordExample(KeywordExample exampleIn)
        {
            ContainedExample = exampleIn;
        }

        public void Claim() { NumberOfClaims++; }
        public void ReleaseClaim() { NumberOfClaims--; }

        public override string ToString()
        {
            return ContainedExample.ToString();
        }
    }
}
