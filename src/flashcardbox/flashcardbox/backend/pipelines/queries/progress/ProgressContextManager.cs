using flashcardbox.backend.pipelines.commands.selectduecard;
using nsimpleeventstore;

namespace flashcardbox.backend.pipelines.queries.progress
{
    internal class ProgressContextManager : SelectDueCardContextManager {
        public ProgressContextManager(IEventstore es) : base(es) {}
    }
}