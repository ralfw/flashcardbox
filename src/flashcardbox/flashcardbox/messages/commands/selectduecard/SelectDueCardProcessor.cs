using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands.selectduecard
{
    internal class SelectDueCardProcessor : ICommandProcessor {
        public (CommandStatus, Event[], string, Notification[]) Process(IMessage msg, IMessageContextModel ctx, string version)
        {
            throw new System.NotImplementedException();
        }
    }
}