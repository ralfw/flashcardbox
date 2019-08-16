using nsimplemessagepump.contract;

namespace flashcardbox.messages
{
    public class RegisterAnswerCommand : Command
    {
        public string CardId;
        public bool CorrectlyAnswered;
    }
}