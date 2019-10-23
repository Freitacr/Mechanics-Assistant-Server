using System.Collections.Generic;

namespace OldManinTheShopServer.Data
{
    /**
     * <summary>Interface-like class that has the responsibility of loading Mechanic Queries and KeywordTrainingExamples
     * from a source of data as well as adding Mechanic Queries to that source </summary>
     */
    public abstract class DataSource
    {
        public abstract List<MechanicQuery> LoadMechanicQueries();

        public abstract List<KeywordTrainingExample> LoadKeywordTrainingExamples();

        public abstract bool AddData(MechanicQuery toAdd);
    }
}
