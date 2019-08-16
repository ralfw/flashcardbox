using System.Collections.Generic;
using flashcardbox.backend.events;
using flashcardbox.messages;
using flashcardbox.messages.queries;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;

namespace flashcardbox.backend.pipelines.queries.duecard
{
    internal class DueCardContextModel : IMessageContextModel
    {
        public DueCardFoundQueryResult DueCard;
    }
    
    
    internal class DueCardContextManager : IMessageContextModelLoader
    {
        private readonly IEventstore _es;

        public DueCardContextManager(IEventstore es) { _es = es; }
        
        
        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
        {
            var flashcards = new Dictionary<string,DueCardFoundQueryResult>();
            var dueCardId = "";

            foreach (var e in _es.Replay().Events)
                switch (e) {
                    case NewCardEncountered nce:
                        flashcards[nce.Id] = new DueCardFoundQueryResult() {
                            CardId = nce.Id,
                            Question = nce.Question,
                            Answer = nce.Answer,
                            Tags = nce.Tags
                        };
                        break;
                    case CardWasChanged cwc:
                        var card = flashcards[cwc.CardId];
                        card.Question = cwc.Question;
                        card.Answer = cwc.Answer;
                        card.Tags = cwc.Tags;
                        break;
                    case CardFoundMissing cfm:
                        flashcards.Remove(cfm.CardId);
                        break;
                    
                    case CardMovedTo cmt:
                        flashcards[cmt.CardId].BinIndex = cmt.BinIndex;
                        if (cmt.CardId == dueCardId)
                            dueCardId = "";
                        break;
                    
                    case DueCardSelected dcs:
                        dueCardId = dcs.CardId;
                        break;
                }

            return flashcards.TryGetValue(dueCardId, out var dueCard) 
                    ? (new DueCardContextModel{DueCard = dueCard}, "") 
                    : (new DueCardContextModel{DueCard = null}, "");
        }
    }
}