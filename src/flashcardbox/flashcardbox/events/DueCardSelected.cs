using nsimpleeventstore;

namespace flashcardbox.events
{
    public class DueCardSelected : Event
    {
        public string CardId;
    }
}