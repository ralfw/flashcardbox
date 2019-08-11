using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.queries
{
    internal class DueCardQueryProcessor : IQueryProcessor
    {
        public QueryResult Process(IMessage msg, IMessageContextModel ctx)
        {
            throw new System.NotImplementedException();
        }
    }
}