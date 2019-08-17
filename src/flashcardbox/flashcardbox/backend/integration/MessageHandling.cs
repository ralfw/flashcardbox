using flashcardbox.backend.adapters;
using flashcardbox.backend.pipelines.commands.registeranswer;
using flashcardbox.backend.pipelines.commands.selectduecard;
using flashcardbox.backend.pipelines.commands.sync;
using flashcardbox.backend.pipelines.queries.duecard;
using flashcardbox.backend.pipelines.queries.progress;
using flashcardbox.messages.commands;
using flashcardbox.messages.queries;
using nsimpleeventstore;
using nsimplemessagepump;
using nsimplemessagepump.contract;

namespace flashcardbox.backend.integration
{
    public class MessageHandling : IMessageHandling
    {
        private readonly MessagePump _mp;
        
        public MessageHandling(IEventstore es, FlashcardboxDb db) {
            _mp = new MessagePump(es);

            _mp.On<SyncCommand>().Load(new SyncContextManagement(es)).Do(new SyncProcessor(db));
            _mp.On<SelectDueCardCommand>().Load(new SelectDueCardContextManager(es)).Do(new SelectDueCardProcessor());
            _mp.On<RegisterAnswerCommand>().Load(new RegisterAnswerContextManager(es)).Do(new RegisterAnswerProcessor());
            
            _mp.On<DueCardQuery>().Load(new DueCardContextManager(es)).Do(new DueCardProcessor());
            _mp.On<ProgressQuery>().Load(new ProgressContextManager(es)).Do(new ProgressProcessor());
        }
        
        
        public CommandStatus Handle(SyncCommand cmd) => _mp.Handle(cmd).Msg as CommandStatus;
        public CommandStatus Handle(SelectDueCardCommand cmd) => _mp.Handle(cmd).Msg as CommandStatus;
        public CommandStatus Handle(RegisterAnswerCommand cmd) => _mp.Handle(cmd).Msg as CommandStatus;
        
        public DueCardQueryResult Handle(DueCardQuery query) => _mp.Handle(query).Msg as DueCardQueryResult;
        public ProgressQueryResult Handle(ProgressQuery query) => _mp.Handle(query).Msg as ProgressQueryResult;
    }
}