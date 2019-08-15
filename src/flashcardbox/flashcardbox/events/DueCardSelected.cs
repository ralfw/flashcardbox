using nsimpleeventstore;

namespace flashcardbox.events
{
    public class DueCardSelected : Event
    {
        public string CardId;
        public int BinIndex;
        
        public override string ToString() => $"DueCardSelected({CardId},{BinIndex})";
    }
}