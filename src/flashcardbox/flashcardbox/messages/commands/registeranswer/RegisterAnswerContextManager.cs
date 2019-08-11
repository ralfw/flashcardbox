using System.Collections.Generic;
using System.Linq;
using flashcardbox.events;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;

namespace flashcardbox.messages.commands.registeranswer
{
    /*
     * For all messages only their current bin is needed.
     * The processor then decides where to move the card based on correctness of answer.
     */
    internal class RegisterAnswerContextModel : IMessageContextModel
    {
        public Dictionary<string, int> CardsInBins = new Dictionary<string, int>();
    }
    
    
    internal class RegisterAnswerContextManager : IMessageContextModelLoader
    {
        private readonly IEventstore _es;

        public RegisterAnswerContextManager(IEventstore es) { _es = es; }


        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
            => (
                    _es.Replay(typeof(CardMovedTo), typeof(CardFoundMissing))
                       .Events
                       .Aggregate(new RegisterAnswerContextModel(), Apply),
                    ""
               );

        
        private static RegisterAnswerContextModel Apply(RegisterAnswerContextModel model, Event e) {
            switch (e) {
                case CardMovedTo cm:
                    model.CardsInBins[cm.CardId] = cm.BinIndex;
                    break;
                case CardFoundMissing cfm:
                    model.CardsInBins.Remove(cfm.CardId);
                    break;
            }
            return model;
        }
    }
}