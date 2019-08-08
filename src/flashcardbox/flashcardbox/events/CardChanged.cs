using nsimpleeventstore;

namespace flashcardbox.events
{
    public class CardChanged : Event
    {
        public string CardId;
        public string Question;
        public string Answer;
        public string Tags;
    }
}