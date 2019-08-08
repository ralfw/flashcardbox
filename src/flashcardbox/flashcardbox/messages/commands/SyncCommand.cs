using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands
{
    public class SyncCommand : Command
    {
        public string FlashcardboxPath;
    }

    internal class SyncCommandMessageContextManagement : IMessageContextModelLoader
    {
        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
        {
            throw new System.NotImplementedException();
        }
    }
    
    internal class SyncCommandProcessor : ICommandProcessor
    {
        public (CommandStatus, Event[], string, Notification[]) Process(IMessage msg, IMessageContextModel ctx, string version)
        {
            throw new System.NotImplementedException();
        }
    }
}