using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using flashcardbox.events;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;

namespace flashcardbox.messages.queries
{
    internal class DueCardQueryContextModel : IMessageContextModel
    {
        public DueCardQueryResult DueCard;
    }
    
    
    internal class DueCardQueryContextManager : IMessageContextModelLoader
    {
        private readonly IEventstore _es;

        public DueCardQueryContextManager(IEventstore es) { _es = es; }
        
        
        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
        {
            var flashcards = new Dictionary<string,DueCardQueryResult>();
            var dueCardId = "";

            foreach (var e in _es.Replay().Events)
                switch (e) {
                    case NewCardEncountered nce:
                        flashcards[nce.Id] = new DueCardQueryResult {
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
                        break;
                    
                    case DueCardSelected dcs:
                        dueCardId = dcs.CardId;
                        break;
                }

            return flashcards.TryGetValue(dueCardId, out var dueCard) 
                    ? (new DueCardQueryContextModel{DueCard = dueCard}, "") 
                    : (null, "");
        }
    }
}