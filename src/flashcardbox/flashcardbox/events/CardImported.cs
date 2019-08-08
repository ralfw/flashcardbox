using nsimpleeventstore;

namespace flashcardbox.events
{
    public class CardImported : Event
    {
        public string Question;
        public string Answer;
        public string Tags;
        public string Hash;
    }
}