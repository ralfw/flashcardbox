using nsimpleeventstore;

namespace flashcardbox.events
{
    public class CardWasChanged : Event
    {
        public string CardId;
        public string Question;
        public string Answer;
        public string Tags;
        
        public override string ToString() => $"CardWasChanged({CardId},Q:{Question},A:{Answer},T:{Tags})";
    }
}