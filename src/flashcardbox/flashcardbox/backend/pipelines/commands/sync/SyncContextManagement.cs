using System.Collections.Generic;
using System.Linq;
using flashcardbox.backend.adapters;
using flashcardbox.backend.events;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;

namespace flashcardbox.backend.pipelines.commands.sync
{
    /*
     * context model for flashcard import:
     * -id, hash, bin
     *
     * context model for config import:
     * -(upper due threshold, lower due threshold)* // FlashcardboxConfig
     * 
     */
    internal class SyncContextModel : IMessageContextModel
    {
        public Dictionary<string, (string binIndex, string hash)> Flashcards;
        public FlashcardboxConfig Config;
    }
    
    
    internal class SyncContextManagement : IMessageContextModelLoader
    {
        private readonly IEventstore _es;

        public SyncContextManagement(IEventstore es) { _es = es; }


        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
            => (new SyncContextModel {
                    Flashcards = Load_flashcards(_es),
                    Config = Load_config(_es)
                },
                "");


        private static Dictionary<string, (string binIndex, string hash)> Load_flashcards(IEventstore es)
            => es.Replay(typeof(NewCardEncountered), typeof(CardWasChanged), typeof(CardMovedTo), typeof(CardFoundMissing))
                .Events
                .Aggregate(new Dictionary<string, (string binIndex, string hash)>(), Apply);

        private static Dictionary<string, (string binIndex, string hash)> Apply(
            Dictionary<string, (string binIndex, string hash)> flashcards, 
            Event e)
        {
            switch (e) {
                case NewCardEncountered ci:
                    flashcards[ci.Id] = ("0", FlashcardHash.Calculate(ci.Question, ci.Answer, ci.Tags));
                    break;
                case CardWasChanged cc:
                    flashcards[cc.CardId] = (flashcards[cc.CardId].binIndex, FlashcardHash.Calculate(cc.Question, cc.Answer, cc.Tags));
                    break;
                case CardMovedTo cm:
                    flashcards[cm.CardId] = (cm.BinIndex.ToString(), flashcards[cm.CardId].hash);
                    break;
                case CardFoundMissing cd:
                    flashcards.Remove(cd.CardId);
                    break;
            }
            return flashcards;
        }
        
        private static FlashcardboxConfig Load_config(IEventstore es) {
            var e = es.Replay(typeof(BoxConfigured)).Events.LastOrDefault() as BoxConfigured;
            if (e == null) return new FlashcardboxConfig();
            
            return new FlashcardboxConfig {
                Bins = e.Bins.Select(Map).ToArray()
            };
            
            
            FlashcardboxConfig.Bin Map(BoxConfigured.Bin b)
                => new FlashcardboxConfig.Bin {
                    LowerDueThreshold = b.LowerDueThreshold,
                    UpperDueThreshold = b.UpperDueThreshold
                };
        }
    }
}