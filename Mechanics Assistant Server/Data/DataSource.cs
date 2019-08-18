using System.Collections.Generic;

namespace MechanicsAssistantServer.Data
{
    public abstract class DataSource
    {
        public abstract List<MechanicQuery> LoadMechanicQueries();

        public abstract List<KeywordTrainingExample> LoadKeywordTrainingExamples();

        public abstract bool AddData(MechanicQuery toAdd);
    }
}
