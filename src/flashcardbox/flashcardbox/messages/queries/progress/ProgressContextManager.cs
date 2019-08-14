using flashcardbox.messages.commands.selectduecard;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;

namespace flashcardbox.messages.queries.progress
{
    internal class ProgressContextManager : SelectDueCardContextManager {
        public ProgressContextManager(IEventstore es) : base(es) {}
    }
}