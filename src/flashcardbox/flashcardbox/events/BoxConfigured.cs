using nsimpleeventstore;

namespace flashcardbox.events
{
    public class BoxConfigured : Event
    {
        public class Bin
        {
            public int UpperDueThreshold;
            public int LowerDueThreshold;
        }
        
        public Bin[] Bins;
    }
}