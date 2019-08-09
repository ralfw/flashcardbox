using nsimpleeventstore;

namespace flashcardbox.events
{
    public class CardDeleted : Event
    {
        public string CardId;
    }
}