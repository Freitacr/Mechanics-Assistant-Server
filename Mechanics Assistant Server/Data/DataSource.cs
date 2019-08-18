using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Data
{
    public abstract class DataSource
    {
        public abstract List<MechanicQuery> LoadMechanicQueries();

        public abstract List<KeywordTrainingExample> LoadKeywordTrainingExamples();

        public abstract bool AddData(MechanicQuery toAdd);
    }
}
