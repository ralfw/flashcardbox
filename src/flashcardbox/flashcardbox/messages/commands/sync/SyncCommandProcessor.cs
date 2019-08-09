using System;
using System.Collections.Generic;
using System.Linq;
using flashcardbox.events;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands
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
            var events = new List<Event>();

            Sync_flashcards(model, events);
            Sync_config(model, events);

            return (new Success(), events.ToArray(), "", new Notification[0]);
        }

        
        void Sync_flashcards(SyncCommandMessageContextModel model, List<Event> events) {
            var flashcards = _db.LoadFlashcards().ToArray();
                
            Sync_new_and_changed_flashcards(model, flashcards, events);
            Sync_deleted_flashcards(model, flashcards, events);
                
            _db.StoreFlashcards(flashcards);
        }
        
        
        private void Sync_new_and_changed_flashcards(SyncCommandMessageContextModel model, FlashcardRecord[] flashcards, List<Event> events)
        {
            foreach (var fc in flashcards)
                Sync_flashcard(fc);
           

            void Sync_flashcard(FlashcardRecord fc) {
                if (string.IsNullOrWhiteSpace(fc.Id)) {
                    // new card found
                    var eImported = new CardImported {Question = fc.Question, Answer = fc.Answer, Tags = fc.Tags};
                    var eMoved = new CardMovedTo {CardId = eImported.Id};
                    events.AddRange(new Event[]{eImported,eMoved});
                    
                    fc.Id = eImported.Id;
                    fc.BinIndex = eMoved.BinIndex.ToString();
                }
                else {
                    // changed card found
                    var fcHash = FlashcardHash.Calculate(fc.Question, fc.Answer, fc.Tags);
                    if (model.Flashcards[fc.Id].hash != fcHash)
                        events.Add(new CardChanged{CardId = fc.Id, Question = fc.Question, Answer = fc.Answer, Tags = fc.Tags});
                }

                // always update the binIndex
                if (model.Flashcards.ContainsKey(fc.Id))
                    fc.BinIndex = model.Flashcards[fc.Id].binIndex;
            }
        }


        private void Sync_deleted_flashcards(SyncCommandMessageContextModel model, FlashcardRecord[] flashcards, List<Event> events) {
            foreach(var id in model.Flashcards.Keys)
                if (flashcards.Any(fc => fc.Id == id) is false)
                    events.Add(new CardDeleted{CardId = id});
        }


        private void Sync_config(SyncCommandMessageContextModel model, List<Event> events) {
            var config = _db.LoadConfig();
            if (config.Bins.Length > 0)
                if (Config_changed())
                    Record_config();

            
            bool Config_changed() {
                if (model.Config.Bins.Length == 0) return true;
                if (model.Config.Bins.Length != config.Bins.Length) return true;
                
                for(var i=0; i<model.Config.Bins.Length; i++)
                    if (model.Config.Bins[i].LowerDueThreshold != config.Bins[i].LowerDueThreshold ||
                        model.Config.Bins[i].UpperDueThreshold != config.Bins[i].UpperDueThreshold)
                        return false;
                
                return true;
            }

            void Record_config() {
                events.Add(new BoxConfigured {
                    Bins = config.Bins.Select(Map).ToArray()
                });
            }
            
            BoxConfigured.Bin Map(FlashcardboxConfig.Bin b)
                => new BoxConfigured.Bin {
                    LowerDueThreshold = b.LowerDueThreshold,
                    UpperDueThreshold = b.UpperDueThreshold
                };
        }
    }
}