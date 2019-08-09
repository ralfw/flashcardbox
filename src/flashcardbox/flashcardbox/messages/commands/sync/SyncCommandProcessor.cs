using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands
{
    internal class SyncCommandProcessor : ICommandProcessor
    {
        private readonly FlashcardboxDb _db;

        public SyncCommandProcessor(FlashcardboxDb db) { _db = db; }
        
        
        public (CommandStatus, Event[], string, Notification[]) Process(IMessage msg, IMessageContextModel ctx, string version)
        {
            throw new System.NotImplementedException();
        }
    }
}