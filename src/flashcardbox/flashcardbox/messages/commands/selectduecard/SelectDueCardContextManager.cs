using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;

namespace flashcardbox.messages.commands.selectduecard
{
    internal class SelectDueCardContextModel : IMessageContextModel {}
    
    
    internal class SelectDueCardContextManager : IMessageContextModelLoader
    {
        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
        {
            throw new System.NotImplementedException();
        }
    }
}