using nsimpleeventstore;

namespace flashcardbox.events
{
    public class QuestionAnsweredCorrectly : Event
    {
        public string CardId;
        
        public override string ToString() => $"QuestionAnsweredCorrectly({CardId})";
    }
}