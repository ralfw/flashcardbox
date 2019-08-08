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

    /*
     * context model for flashcard import:
     * -id, hash, bin
     *
     * context model for config import:
     * -(upper due threshold, lower due threshold)* // FlashcardboxConfig
     * 
     */
    internal class SyncCommandMessageContextManagement : IMessageContextModelLoader
    {
        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
        {
            throw new System.NotImplementedException();
        }
    }
    
    /*
     * flashcards.csv is loaded and compared against the already imported cards:
     * - a file card has to be imported, if it has no id yet
     * - a stream card needs to be registered as deleted if its id is missing in the file
     * - a stream card needs to be changed if its hash is differing from its corresponding file card
     *   the hash is calculated from question+answer+tags
     *
     * flashcards.csv on the other hand is updated from the event stream:
     * - if a card gets imported an id is assigned to it
     * - for every card in the file the current bin is updated
     *
     * finally the config is loaded. if has changed it's updated in the event stream.
     */
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