using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.queries.duecard
{
    internal class DueCardQueryProcessor : IQueryProcessor
    {
        public QueryResult Process(IMessage msg, IMessageContextModel ctx)
        {
            var model = ctx as DueCardQueryContextModel;

            if (model.DueCard != null) return model.DueCard;
            
            return new DueCardQueryResult {
                CardId = "",
                Question = "*** No due card found! ***",
                Answer = "*** No due card found! ***",
                Tags = "",
                BinIndex = -1
            };
        }
    }
}