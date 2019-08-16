using nsimplemessagepump.contract;

namespace flashcardbox.messages.commands
{
    public class RegisterAnswerCommand : Command
    {
        public string CardId;
        public bool CorrectlyAnswered;
    }
}