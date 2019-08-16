using nsimpleeventstore;

namespace flashcardbox.backend.events
{
    public class QuestionAnsweredIncorrectly : Event
    {
        public string CardId;
        
        public override string ToString() => $"QuestionAnsweredIncorrectly({CardId})";
    }
}