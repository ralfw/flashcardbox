namespace flashcardbox.backend.adapters
{
    public class FlashcardboxConfig
    {
        public class Bin {
            public int UpperDueThreshold;
            public int LowerDueThreshold;
        }
        
        public Bin[] Bins = new Bin[0];
    }
}