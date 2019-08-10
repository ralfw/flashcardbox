using System.Collections.Generic;
using System.Linq;
using flashcardbox.events;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands.sync
{
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
        
        
        public (CommandStatus, Event[], string, Notification[]) Process(IMessage msg, IMessageContextModel ctx, string version) {
            var model = ctx as SyncCommandMessageContextModel;

            var events0 = Sync_flashcards(model);
            var events1 = Sync_config(model);

            return (new Success(), events0.Concat(events1).ToArray(), "", new Notification[0]);
        }

        
        IEnumerable<Event> Sync_flashcards(SyncCommandMessageContextModel model) {
            var flashcards = _db.LoadFlashcards().ToArray();

            var events0 = Sync_new_and_changed_flashcards(model, flashcards);
            var events1 = Sync_deleted_flashcards(model, flashcards);
                
            _db.StoreFlashcards(flashcards);

            return events0.Concat(events1);
        }
        
        
        private Event[] Sync_new_and_changed_flashcards(SyncCommandMessageContextModel model, FlashcardRecord[] flashcards)
        {
            return flashcards.SelectMany(Sync_flashcard).ToArray();        

            IEnumerable<Event> Sync_flashcard(FlashcardRecord fc) {
                if (string.IsNullOrWhiteSpace(fc.Id)) {
                    // new card found
                    var eImported = new CardImported {Question = fc.Question, Answer = fc.Answer, Tags = fc.Tags};

                    var eMoved = new CardMovedTo {CardId = eImported.Id, BinIndex = 0};
                    if (string.IsNullOrWhiteSpace(fc.BinIndex) is false)
                        eMoved.BinIndex = int.Parse(fc.BinIndex);
                    
                    yield return eImported;
                    yield return eMoved;
                    
                    fc.Id = eImported.Id;
                    fc.BinIndex = eMoved.BinIndex.ToString();
                }
                else {
                    // changed card found
                    var fcHash = FlashcardHash.Calculate(fc.Question, fc.Answer, fc.Tags);
                    if (model.Flashcards[fc.Id].hash != fcHash)
                        yield return new CardChanged{CardId = fc.Id, Question = fc.Question, Answer = fc.Answer, Tags = fc.Tags};
                }

                // always update the binIndex
                if (model.Flashcards.ContainsKey(fc.Id))
                    fc.BinIndex = model.Flashcards[fc.Id].binIndex;
            }
        }


        private CardDeleted[] Sync_deleted_flashcards(SyncCommandMessageContextModel model, FlashcardRecord[] flashcards)
            => (from id in model.Flashcards.Keys
                        where flashcards.Any(fc => fc.Id == id) is false
                        select new CardDeleted {CardId = id}
               ).ToArray();


        private IEnumerable<Event> Sync_config(SyncCommandMessageContextModel model) {
            var config = _db.LoadConfig();
            if (config.Bins.Length > 0)
                if (Config_changed())
                    yield return Record_config();

            
            bool Config_changed() {
                if (model.Config.Bins.Length == 0) return true;
                if (model.Config.Bins.Length != config.Bins.Length) return true;
                
                for(var i=0; i<model.Config.Bins.Length; i++)
                    if (model.Config.Bins[i].LowerDueThreshold != config.Bins[i].LowerDueThreshold ||
                        model.Config.Bins[i].UpperDueThreshold != config.Bins[i].UpperDueThreshold)
                        return true;
                
                return false;
            }

            BoxConfigured Record_config()
                => new BoxConfigured {
                    Bins = config.Bins.Select(Map).ToArray()
                };
            
            BoxConfigured.Bin Map(FlashcardboxConfig.Bin b)
                => new BoxConfigured.Bin {
                    LowerDueThreshold = b.LowerDueThreshold,
                    UpperDueThreshold = b.UpperDueThreshold
                };
        }
    }
}