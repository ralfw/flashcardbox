using nsimpleeventstore;

namespace flashcardbox.backend.events
{
    public class CardMovedTo : Event
    {
        public string CardId;
        public int BinIndex;

        public override string ToString() => $"CardMovedTo({CardId},{BinIndex})";
    }
}