using System.Collections.Generic;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;

namespace flashcardbox.messages.commands.selectduecard
{
    internal class SelectDueCardContextModel : IMessageContextModel
    {
        public List<List<string>> Bins;
        public FlashcardboxConfig Config;
    }
    
    
    /*
     * For all cards find the current bin. Within the bins order them chronologically, ie
     * cards will be added at the end of all cards already present.
     *
     * There is always a bin 0, even if never any card was moved there.
     * 
     * Find the most recent config. If there is none then the command fails.
     */
    internal class SelectDueCardContextManager : IMessageContextModelLoader
    {
        private readonly IEventstore _es;

        public SelectDueCardContextManager(IEventstore es) { _es = es; }
        
        
        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
        {
            throw new System.NotImplementedException();
        }
    }
}