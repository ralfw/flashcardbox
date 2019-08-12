using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands.selectduecard
{
    /*
     * The processor selects the due card based on how cards are distributed across bins compared
     * to the current box configuration. For details see analysis.md.
     *
     * Fail on missing config!
     */
    internal class SelectDueCardProcessor : ICommandProcessor {
        public (CommandStatus, Event[], string, Notification[]) Process(IMessage msg, IMessageContextModel ctx, string version)
        {
            throw new System.NotImplementedException();
        }
    }
}