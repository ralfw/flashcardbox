using System.Linq;
using nsimplemessagepump.contract;

namespace flashcardbox.messages.queries
{
    public class ProgressQuery : Query {}

    public class ProgressQueryResult : QueryResult
    {
        public class Bin
        {
            public int Count;
            public int LowerDueThreshold;
            public int UpperDueThreshold;
            public bool IsDue;
        }

        public Bin[] Bins;

        
        public int TotalCount => Bins.Sum(b => b.Count);
    }
}