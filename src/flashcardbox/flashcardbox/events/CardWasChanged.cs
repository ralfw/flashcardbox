using nsimpleeventstore;

namespace flashcardbox.events
{
    public class CardWasChanged : Event
    {
        public string CardId;
        public string Question;
        public string Answer;
        public string Tags;
    }
}