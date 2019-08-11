using System.Windows.Input;
using nsimplemessagepump.contract;

namespace flashcardbox.messages.commands.registeranswer
{
    public class RegisterAnswerCommand : Command
    {
        public string CardId;
        public bool CorrectlyAnswered;
    }
}