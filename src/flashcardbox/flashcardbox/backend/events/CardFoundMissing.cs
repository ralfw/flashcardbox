using nsimpleeventstore;

namespace flashcardbox.backend.events
{
    public class CardFoundMissing : Event
    {
        public string CardId;

        public override string ToString() => $"CardFoundMissing({CardId})";
    }
}