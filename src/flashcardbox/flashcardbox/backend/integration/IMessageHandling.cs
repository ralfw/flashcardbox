using flashcardbox.messages.commands;
using flashcardbox.messages.queries;
using nsimplemessagepump.contract;

namespace flashcardbox.backend.integration
{
    public interface IMessageHandling
    {
        CommandStatus Handle(SyncCommand cmd);
        CommandStatus Handle(SelectDueCardCommand cmd);
        CommandStatus Handle(RegisterAnswerCommand cmd);
        DueCardQueryResult Handle(DueCardQuery query);
        ProgressQueryResult Handle(ProgressQuery query);
    }
}