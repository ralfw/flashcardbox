using flashcardbox.events;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands.registeranswer
{
    /*
     * The card is moved to the next bin if answered correctly.
     * Otherwise it's put back in bin 1.
     */
    internal class RegisterAnswerProcessor : ICommandProcessor
    {
        public (CommandStatus, Event[], string, Notification[]) Process(IMessage msg, IMessageContextModel ctx, string version)
        {
            var cmd = msg as RegisterAnswerCommand;
            var model = ctx as RegisterAnswerContextModel;
            
            if (model.CardsInBins.ContainsKey(cmd.CardId) is false) 
                return (new Failure($"Cannot register answer for unknown card '{cmd.CardId}'!"), new Event[0], "", new Notification[0]);

            Event[] events;
            if (cmd.CorrectlyAnswered) {
                events = new Event[] {
                    new QuestionAnsweredCorrectly{CardId = cmd.CardId},
                    new CardMovedTo{CardId = cmd.CardId, BinIndex = model.CardsInBins[cmd.CardId] + 1} 
                };
            }
            else
            {
                events = new Event[] {
                    new QuestionAnsweredIncorrectly{CardId = cmd.CardId},
                    new CardMovedTo{CardId = cmd.CardId, BinIndex = 1} 
                };
            }

            return (new Success(), events, "", new Notification[0]);
        }
    }
}