using nsimpleeventstore;

namespace flashcardbox.events
{
    public class CardMovedTo : Event
    {
        public string CardId;
        public int BinIndex;
    }
}