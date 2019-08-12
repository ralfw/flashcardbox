using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.queries.duecard
{
    internal class DueCardProcessor : IQueryProcessor
    {
        public QueryResult Process(IMessage msg, IMessageContextModel ctx)
        {
            var model = ctx as DueCardContextModel;

            if (model.DueCard != null) return model.DueCard;
            
            return new DueCardNotFoundQueryResult();
        }
    }
}