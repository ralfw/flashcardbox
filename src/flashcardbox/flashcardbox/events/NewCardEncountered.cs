using nsimpleeventstore;

namespace flashcardbox.events
{
    public class NewCardEncountered : Event
    {
        public string Question;
        public string Answer;
        public string Tags;
        
        public override string ToString() => $"NewCardEncountered({Id},Q:{Question},A:{Answer},T:{Tags})";
    }
}