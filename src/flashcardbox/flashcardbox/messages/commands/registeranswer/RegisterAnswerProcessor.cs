using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands.registeranswer
{
    /*
     * The card is moved to the next bin if answered correctly.
     * Otherwise it's put back in bin 1.
     */
    internal class RegisterAnswerProcessor : ICommandProcessor
    {
        //TODO: Write test for RegisterAnswerProcessor
        //TODO: Implement RegisterAnswerProcessor
        public (CommandStatus, Event[], string, Notification[]) Process(IMessage msg, IMessageContextModel ctx, string version)
        {
            throw new System.NotImplementedException();
        }
    }
}